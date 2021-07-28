using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ICA.Schematic;
using ICA.AutoCAD.Adapter.Windows.ViewModels;
using ICA.AutoCAD.Adapter.Windows.Views;
using System.Collections.Generic;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Autodesk.AutoCAD.Geometry;
using System.IO;

namespace ICA.AutoCAD.Adapter
{
    public static class Commands
    {
        private static bool? _mountMode;
        public static bool MountMode
        {
            get
            {
                if (_mountMode is null)
                {
                    LayerTableRecord mountingLayer = CurrentDocument.Database.GetLayer("MOUNTING");
                    return !mountingLayer.IsFrozen;
                }
                return (bool)_mountMode;
            }
            set
            {
                if (value)
                {
                    ShowMountingLayers();
                    PreviousGridSnap = SystemVariables.GridSnap;
                    SystemVariables.GridSnap = false;
                    PreviousObjectSnap = ObjectSnap.Value;
                    ObjectSnap.None();
                    ObjectSnap.Endpoint = true;
                    ObjectSnap.Perpendicular = true;
                }
                else
                {
                    HideMountingLayers();
                    if (PreviousGridSnap)
                        SystemVariables.GridSnap = true;
                    ObjectSnap.Value = PreviousObjectSnap;
                }
                _mountMode = value;
                CurrentDocument.Editor.Regen();
            }
        }
        private static bool PreviousGridSnap;
        public static short PreviousObjectSnap;

        public static Document CurrentDocument => Application.DocumentManager.MdiActiveDocument;

        [CommandMethod("EDITCOMPONENT", CommandFlags.UsePickSet)]
        public static async void EditAsync()
        {
            ISymbol symbol = SelectSymbol();

            switch (symbol)
            {
                case ParentSymbol parent:
                    var editView = new ParentSymbolEditView(parent);
                    Application.ShowModalWindow(editView);
                    break;
                case ChildSymbol child:
                    var childEditView = new ChildSymbolEditView(child);
                    Application.ShowModalWindow(childEditView);
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Toggles between layer states to help with manual mounting of panel components
        /// </summary>
        [CommandMethod("MOUNT")]
        public static void ToggleMountingLayers() => MountMode = !MountMode;

        public static void HideMountingLayers()
        {
            List<string> mountingLayers = new List<string>
            {
                "MOUNTING",
                "BOUNDS",
                "CLEARANCE"
            };
            List<string> viewingLayers = new List<string>
            {
                "COMPONENTS",
                "WIPEOUT"
            };
            foreach (string layerName in mountingLayers)
            {
                CurrentDocument.Database.GetLayer(layerName).Freeze();
            }
            foreach (string layerName in viewingLayers)
            {
                CurrentDocument.Database.GetLayer(layerName).Thaw();
            }
        }

        public static void ShowMountingLayers()
        {
            List<string> mountingLayers = new List<string>
            {
                "MOUNTING",
                "BOUNDS",
                "CLEARANCE"
            };
            List<string> viewingLayers = new List<string>
            {
                "COMPONENTS",
                "WIPEOUT"
            };
            foreach (string layerName in mountingLayers)
            {
                CurrentDocument.Database.GetLayer(layerName).Thaw();
            }
            foreach (string layerName in viewingLayers)
            {
                CurrentDocument.Database.GetLayer(layerName).Freeze();
            }
        }

        /// <summary>
        /// Prompts for selection of a schematic symbol from the current drawing, or selects the implied symbol
        /// </summary>
        /// <returns></returns>
        public static ISymbol SelectSymbol()
        {
            Editor currentEditor = CurrentDocument.Editor;
            try
            {
                PromptSelectionResult selectionResult = currentEditor.SelectImplied();
                if (selectionResult.Status == PromptStatus.Error)
                {
                    PromptSelectionOptions selectionOptions = new PromptSelectionOptions
                    {
                        SingleOnly = true
                    };
                    selectionResult = currentEditor.GetSelection(selectionOptions);
                }
                else
                {
                    currentEditor.SetImpliedSelection(new ObjectId[0]);
                }

                if (selectionResult.Status == PromptStatus.OK)
                {
                    try
                    {
                        ObjectId[] objectIds = selectionResult.Value.GetObjectIds();
                        foreach (ObjectId objectId in objectIds)
                        {
                            if (objectId.ObjectClass.Name == "AcDbBlockReference")
                            {
                                BlockReference reference = objectId.Open() as BlockReference;
                                if (reference.HasAttributeReference("TAG1"))
                                    return new ParentSymbol(reference);
                                else if (reference.HasAttributeReference("TAG2"))
                                    return new ChildSymbol(reference);
                            }
                        }
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        currentEditor.WriteMessage(ex.Message);
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                currentEditor.WriteMessage(ex.Message);
            }
            return null;
        }

        [CommandMethod("TESTINSERT")]
        public static void InsertSymbol()
        {
            InsertSymbol(PromptSymbolName());
        }

        public static void InsertSymbol(string symbolName)
        {
            try
            {
                ISymbol symbol = SchematicSymbolRecord.GetRecord(CurrentDocument.Database, symbolName).InsertSymbol();
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                CurrentDocument.Editor.WriteMessage(ex.Message);
            }
        }

        public static string PromptSymbolName()
        {
            PromptStringOptions options = new PromptStringOptions("Enter symbol name: ")
            {
                AllowSpaces = true
            };
            PromptResult result = CurrentDocument.Editor.GetString(options);
            return result.StringResult;
        }

        [CommandMethod("CURRENTPROJECT")]
        public static void GetCurrentProject()
        {
            if(!CurrentDocument.IsNamedDrawing)
            {
                CurrentDocument.Editor.WriteMessage("File does not belong to a project");
                return;
            }
            CurrentDocument.Editor.WriteMessage(CurrentDocument.Database.GetProject().Name);
        }

        #region Ladder

        [CommandMethod("NEWLADDER")]
        public static void CommandLineInsertLadder()
        {
            LadderTemplate template = Ladder.Prompt();
            if (template != null)
            {
                RemoveLadder();
                template.Insert();
            }
        }

        [CommandMethod("REMOVELADDER")]
        public static void RemoveLadder()
        {
            Ladder.RemoveFrom(CurrentDocument.Database);
        }

        [CommandMethod("LADDERCONFIG")]
        public static void ConfigureLadder()
        {

        }

        #endregion

        #region Title Block

        [CommandMethod("TITLEBLOCK")]
        public static void TitleBlockCommand()
        {
            TitleBlock oldTitleBlock = Application.DocumentManager.MdiActiveDocument.Database.GetTitleBlock();
            TitleBlock newTitleBlock = TitleBlock.Select();

            if(newTitleBlock != null)
            {
                RemoveLadder();
                if(oldTitleBlock != null)
                    oldTitleBlock.Purge();
                newTitleBlock.Insert();
                ZoomExtents(newTitleBlock.Reference.GeometricExtents);
            }
        }

        [CommandMethod("REMOVETITLEBLOCK")]
        public static void PurgeTitleBlock()
        {
            try
            {
                CurrentDocument.Database.GetTitleBlock().Purge();
            }
            catch { }
        }

        [CommandMethod("TITLEBLOCKCONFIG")]
        public static void ConfigureTitleBlock() { }

        #endregion

        #region Multiplexers

        [CommandMethod("BLOCKEDIT", CommandFlags.UsePickSet | CommandFlags.Redraw)]
        public static void AttributeBlockEdit()
        {
            Editor currentEditor = CurrentDocument.Editor;
            try
            {
                PromptSelectionResult selectionResult = currentEditor.SelectImplied();
                if (selectionResult.Status == PromptStatus.Error)
                {
                    PromptSelectionOptions selectionOptions = new PromptSelectionOptions
                    {
                        SingleOnly = true
                    };
                    selectionResult = currentEditor.GetSelection(selectionOptions);
                }
                else
                {
                    currentEditor.SetImpliedSelection(new ObjectId[0]);
                }

                if (selectionResult.Status == PromptStatus.OK)
                {
                    if (selectionResult.Value.Count > 1)
                        return;

                    if (selectionResult.Value[0].ObjectId.ObjectClass.Name != "AcDbBlockReference")
                        return;

                    BlockReference reference = selectionResult.Value[0].ObjectId.Open() as BlockReference;

                    switch (reference.Layer)
                    {
                        case "LADDER":
                            //Ladder();
                            break;
                        case "TITLE BLOCK":
                            TitleBlockCommand();
                            break;
                        case "SYMS":
                            EditAsync();
                            break;
                        default:
                            CurrentDocument.Editor.SetImpliedSelection(selectionResult.Value);
                            CurrentDocument.SendStringToExecute("_EATTEDIT ", false, false, false);
                            return;
                    }

                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                currentEditor.WriteMessage(ex.Message);
            }
        }

        #endregion

        public static void ZoomExtents(Extents3d extents)
        {
            using (ViewTableRecord view = CurrentDocument.Editor.GetCurrentView())
            {
                view.Width = extents.MaxPoint.X - extents.MinPoint.X;
                view.Height = extents.MaxPoint.Y - extents.MinPoint.Y;
                view.CenterPoint = new Point2d(
                    (extents.MaxPoint.X + extents.MinPoint.X) / 2.0,
                    (extents.MaxPoint.Y + extents.MinPoint.Y) / 2.0);
                CurrentDocument.Editor.SetCurrentView(view);
            }
        }
    }
}
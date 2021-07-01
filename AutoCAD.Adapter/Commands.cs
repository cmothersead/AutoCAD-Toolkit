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
using System.Linq;
using ICA.AutoCAD.Adapter.Windows.Models;
using System.Collections.ObjectModel;
using System;
using ICA.AutoCAD.IO;
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
            var editViewModel = new EditViewModel(SelectSymbol());
            await editViewModel.LoadFamilyDataAsync();
            var editWindow = new EditView(editViewModel);
            Application.ShowModalWindow(editWindow);
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
                                using (Transaction transaction = CurrentDocument.TransactionManager.StartTransaction())
                                {
                                    return new ParentSymbol((BlockReference)transaction.GetObject(objectId, OpenMode.ForRead));
                                }
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
                SchematicSymbolRecord.GetRecord(symbolName)
                                     .InsertSymbol();
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

        [CommandMethod("LADDER")]
        public static void CommandLineInsertLadder()
        {
            LadderTemplate template = Ladder.Prompt();
            RemoveLadder();
            template.Insert();
        }

        [CommandMethod("REMOVELADDER")]
        public static void RemoveLadder()
        {
            Ladder.RemoveFrom(CurrentDocument.Database);
        }

        [CommandMethod("TITLEBLOCK")]
        public static void TitleBlockCommand()
        {
            //CurrentDocument.Database.ObjectAppended += new ObjectEventHandler(TitleBlockInsertion.Handler); //Doesn't belong here.
            TitleBlock currentTitleBlock = CurrentDocument.Database.GetTitleBlock();
            try
            {
                TitleBlockView titleBlockWindow = new TitleBlockView(new TitleBlockViewModel(Paths.TitleBlocks));
                ObservableCollection<TitleBlockFile> validFiles = new ObservableCollection<TitleBlockFile>();
                Dictionary<string, string> errorList = new Dictionary<string, string>();
                foreach (TitleBlockFile file in titleBlockWindow.ViewModel.TitleBlocks)
                {
                    string path = file.Uri.LocalPath;
                    if (!TitleBlock.IsDefinitionFile(path))
                        errorList.Add(Path.GetFileName(path), TitleBlock.DefinitionFileException(path));
                    else
                        validFiles.Add(file);
                }

                titleBlockWindow.ViewModel.TitleBlocks = validFiles;

                if (errorList.Count > 0)
                {
                    string errorMessage = "The following files were not loaded:\n";

                    if (errorList.Count == 1)
                        errorMessage = "The following file was not loaded:\n";

                    foreach (var entry in errorList)
                    {
                        errorMessage += "\n\u2022 \"" + entry.Key + "\" : " + entry.Value;
                    }

                    Application.ShowAlertDialog(errorMessage);
                }

                if (currentTitleBlock != null)
                    titleBlockWindow.ViewModel.SelectedTitleBlock = titleBlockWindow.ViewModel.TitleBlocks.Where(titleBlock => titleBlock.Name == currentTitleBlock.Name).FirstOrDefault();

                Application.ShowModalWindow(titleBlockWindow);
                if ((bool)titleBlockWindow.DialogResult)
                {
                    TitleBlockFile SelectedTitleBlock = titleBlockWindow.ViewModel.SelectedTitleBlock;

                    if (currentTitleBlock != null && currentTitleBlock.Name == SelectedTitleBlock.Name)
                        return;

                    RemoveLadder();
                    PurgeTitleBlock();

                    TitleBlock newTitleBlock = new TitleBlock(CurrentDocument.Database, SelectedTitleBlock.Uri);
                    newTitleBlock.Insert();
                    ZoomExtents(newTitleBlock.Reference.GeometricExtents);
                }
            }
            catch (ArgumentException ex)
            {
                Application.ShowAlertDialog(ex.Message);
            }
        }

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

        [CommandMethod("PURGETITLEBLOCK")]
        public static void PurgeTitleBlock()
        {
            try
            {
                CurrentDocument.Database.GetTitleBlock().Purge();
            }
            catch { }
        }

        [CommandMethod("BLOCKEDIT", CommandFlags.UsePickSet)]
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
                        default:
                            return;
                    }

                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                currentEditor.WriteMessage(ex.Message);
            }
        }
    }
}
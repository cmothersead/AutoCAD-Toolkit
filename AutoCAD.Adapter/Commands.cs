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
using ICA.AutoCAD.IO;

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
        public static void ToggleMountingLayers()
        {
            MountMode = !MountMode;
        }

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
                SchematicSymbolRecord record = SchematicSymbolRecord.GetRecord(symbolName);
                record.InsertSymbol();
            }
            catch (Exception ex)
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

        [CommandMethod("TESTLADDER")]
        public static void CommandLineInsertLadder()
        {
            LadderTemplate test;
            PromptKeywordOptions typeOptions = new PromptKeywordOptions("\nChoose ladder type [1 Phase/3 Phase] <1>: ");
            typeOptions.Keywords.Add("1");
            typeOptions.Keywords.Add("3");
            PromptKeywordOptions countOptions = new PromptKeywordOptions("\nChoose number of ladders [1/2] <1>: ");
            countOptions.Keywords.Add("1");
            switch (CurrentDocument.Database.GetTitleBlock().Name)
            {
                case "8.5x11 Title Block":
                    test = new LadderTemplate()
                    {
                        Origin = new Point2d(2.5, 22.5),
                        Height = 19.5,
                        TotalWidth = 15
                    };
                    break;
                case "11x17 Title Block":
                    test = new LadderTemplate()
                    {
                        Origin = new Point2d(2.5, 22.5),
                        Height = 19.5,
                        TotalWidth = 32.5,
                        Gap = 2.5,
                    };
                    countOptions.Keywords.Add("2");
                    break;
                case "Nexteer Title Block":
                    test = new LadderTemplate()
                    {
                        Origin = new Point2d(2.5, 22.5),
                        Height = 20,
                        TotalWidth = 25,
                        Gap = 5,
                    };
                    countOptions.Keywords.Add("2");
                    break;
                default:
                    return;
            }
            PromptResult result = CurrentDocument.Editor.GetKeywords(typeOptions);
            if (result.Status == PromptStatus.OK)
            {
                test.PhaseCount = int.Parse(result.StringResult);
            }

            if (countOptions.Keywords.Count > 1)
            {
                result = CurrentDocument.Editor.GetKeywords(countOptions);
                if (result.Status == PromptStatus.OK)
                {
                    test.LadderCount = int.Parse(result.StringResult);
                }
            }

            test.Insert();
        }

        [CommandMethod("TITLEBLOCK")]
        public static void TitleBlock()
        {
            CurrentDocument.Database.ObjectAppended += new ObjectEventHandler(TitleBlockInsertion.Handler);
            TitleBlockRecord currentTitleBlock = CurrentDocument.Database.GetTitleBlock();
            ElectricalDocumentProperties properties = CurrentDocument.Database.ElectricalProperties();
            if (currentTitleBlock != null)
            {
                //Choose Title Block From List
            }
            //Settings, Attributes, and changeout
            //Is TB inserted? If not, insert at origin
        }
    }
}
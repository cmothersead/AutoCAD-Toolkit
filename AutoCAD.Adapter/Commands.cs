﻿using Autodesk.AutoCAD.Runtime;
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
        public static void TitleBlock()
        {
            //CurrentDocument.Database.ObjectAppended += new ObjectEventHandler(TitleBlockInsertion.Handler); //Doesn't belong here.
            TitleBlockRecord currentTitleBlock = CurrentDocument.Database.GetTitleBlock();
            var titleBlockWindow = new TitleBlockView(new TitleBlockViewModel()
            {
                TitleBlocks = new ObservableCollection<TitleBlock>()
                    {
                        new TitleBlock($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\OneDrive - icacontrol.com\\Electrical Library\\templates\\title blocks\\ICA 8.5x11 Title Block.dwg"),
                        new TitleBlock($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\OneDrive - icacontrol.com\\Electrical Library\\templates\\title blocks\\ICA 11x17 Title Block.dwg"),
                        new TitleBlock($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\OneDrive - icacontrol.com\\Electrical Library\\templates\\title blocks\\Nexteer 11x17 Title Block.dwg")
                    }
            });

            if (currentTitleBlock != null)
                titleBlockWindow.ViewModel.SelectedTitleBlock = titleBlockWindow.ViewModel.TitleBlocks.Where(titleBlock => titleBlock.Name == currentTitleBlock.Name).FirstOrDefault();

            Application.ShowModalWindow(titleBlockWindow);
            if ((bool)titleBlockWindow.DialogResult)
            {
                TitleBlock SelectedTitleBlock = titleBlockWindow.ViewModel.SelectedTitleBlock;

                if (currentTitleBlock != null && currentTitleBlock.Name == SelectedTitleBlock.Name)
                    return;

                RemoveLadder();
                PurgeTitleBlock();

                TitleBlockRecord newTitleBlock = new TitleBlockRecord(CurrentDocument.Database.GetBlockTable().LoadExternalBlockTableRecord(SelectedTitleBlock.FilePath.LocalPath));
                newTitleBlock.Insert();
                ZoomExtents(newTitleBlock.Reference.GeometricExtents);
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
                            TitleBlock();
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
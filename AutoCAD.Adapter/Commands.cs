using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using ICA.Schematic;
using ICA.AutoCAD.Adapter.Windows.ViewModels;
using ICA.AutoCAD.Adapter.Windows.Views;
using System.Collections.Generic;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace ICA.AutoCAD.Adapter
{
    public static class Commands
    {
        private static bool _mountMode;
        public static bool MountMode
        {
            get
            {
                Document currentDocument = Application.DocumentManager.MdiActiveDocument;
                LayerTableRecord mountingLayer = currentDocument.GetLayer("MOUNTING");
                return !mountingLayer.IsFrozen;
            }
            set
            {
                if(value)
                {
                    ShowMountingLayers();
                    PreviousSnapMode = SystemVariables.GridSnap;
                    SystemVariables.GridSnap = false;
                }
                else
                {
                    HideMountingLayers();
                    if (PreviousSnapMode)
                        SystemVariables.GridSnap = true;
                }
                _mountMode = value;
                Application.DocumentManager.MdiActiveDocument.Editor.Regen();
            }
        }
        private static bool PreviousSnapMode;

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
            var test = ObjectSnap.Endpoint;
            ObjectSnap.Endpoint = false;
            //MountMode = !MountMode;
        }

        public static void HideMountingLayers()
        {
            Document currentDocument = Application.DocumentManager.MdiActiveDocument;
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
                currentDocument.GetLayer(layerName).Freeze();
            }
            foreach (string layerName in viewingLayers)
            {
                currentDocument.GetLayer(layerName).Thaw();
            }
        }

        public static void ShowMountingLayers()
        {
            Document currentDocument = Application.DocumentManager.MdiActiveDocument;
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
                currentDocument.GetLayer(layerName).Thaw();
            }
            foreach (string layerName in viewingLayers)
            {
                currentDocument.GetLayer(layerName).Freeze();
            }
        }

        /// <summary>
        /// Prompts for selection of a schematic symbol from the current drawing, or selects the implied symbol
        /// </summary>
        /// <returns></returns>
        public static ISymbol SelectSymbol()
        {
            Document currentDocument = Application.DocumentManager.MdiActiveDocument;
            Editor currentEditor = currentDocument.Editor;
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
                                using (Transaction transaction = currentDocument.TransactionManager.StartTransaction())
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
    }
}

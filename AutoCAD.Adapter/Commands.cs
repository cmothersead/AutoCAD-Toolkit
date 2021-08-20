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
using System;
using ICA.AutoCAD.IO;
using Autodesk.AutoCAD.Colors;
using System.Linq;

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
            symbol.AssignLayers();
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

        public static ObjectId? SelectSingleImplied()
        {
            Editor currentEditor = CurrentDocument.Editor;

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
                currentEditor.SetImpliedSelection(new ObjectId[0]);

            if (selectionResult.Status == PromptStatus.OK)
                return selectionResult.Value.GetObjectIds()[0];

            return null;
        }

        public static ObjectIdCollection SelectMultiple()
        {
            Editor currentEditor = CurrentDocument.Editor;

            PromptSelectionResult selectionResult = currentEditor.GetSelection();

            if (selectionResult.Status == PromptStatus.OK)
                return new ObjectIdCollection(selectionResult.Value.GetObjectIds());

            return null;
        }

        /// <summary>
        /// Prompts for selection of a schematic symbol from the current drawing, or selects the implied symbol
        /// </summary>
        /// <returns></returns>
        public static ISymbol SelectSymbol()
        {
            if (SelectSingleImplied()?.Open() is BlockReference reference)
            {
                if (reference.HasAttributeReference("TAG1"))
                    return new ParentSymbol(reference);
                else if (reference.HasAttributeReference("TAG2"))
                    return new ChildSymbol(reference);
            }

            return null;
        }

        public static SignalSymbol SelectSignal()
        {
            if (SelectSingleImplied()?.Open() is BlockReference reference)
                if (reference.HasAttributeReference("SIGCODE"))
                    return new SignalSymbol(reference);

            return null;
        }

        [CommandMethod("INSERTCOMPONENT")]
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

        [CommandMethod("ASSIGNLAYERS")]
        public static void AssignLayers()
        {
            SelectSymbol()?.AssignLayers();
        }

        [CommandMethod("UPDATETAG")]
        public static void UpdateTag()
        {
            if (SelectSymbol() is ParentSymbol symbol)
                symbol.Tag = $"{symbol.Family}{symbol.LineNumber}1";
        }

        [CommandMethod("UPDATETAG2")]
        public static void UpdateTag2()
        {
            if (SelectSymbol() is ParentSymbol symbol)
                symbol.Tag = $"{symbol.LineNumber}{symbol.Family}";
        }

        [CommandMethod("MATCHWIRES")]
        public static void Matchwires()
        {
            if (SelectSymbol() is ParentSymbol symbol)
                symbol.MatchWireNumbers();
        }

        [CommandMethod("XREFSIGS")]
        public static void XrefSignals()
        {
            if (SelectSignal() is SignalSymbol symbol1)
                if (SelectSignal() is SignalSymbol symbol2)
                    symbol1.CrossReference(symbol2);
        }

        #region Project

        [CommandMethod("CURRENTPROJECT")]
        public static void PrintCurrentProject()
        {
            Project test = CurrentProject();
            if(test != null)
            {
                CurrentDocument.Editor.WriteMessage(test.Name);
            }
            else
            {
                CurrentDocument.Editor.WriteMessage("Current document does not belong to a project.");
            }
        }

        public static Project CurrentProject()
        {
            if (!CurrentDocument.IsNamedDrawing)
                return null;
            return CurrentDocument.Database.GetProject();
        }

        [CommandMethod("EXPORTCURRENTPROJECT")]
        public static void ExportCurrentProject()
        {
            CurrentProject()?.ExportWDP("C:\\Users\\cmotherseadicacontro\\Documents\\test.wdp");
        }

        [CommandMethod("ADDPAGE")]
        public static void ProjectAddPage()
        {
            Project currentProject = CurrentProject();
            currentProject.Properties = new ProjectProperties()
            {
                SchematicTemplate = new Uri($"{Paths.Templates}\\ICA 8.5x11 Title Block.dwt"),
                PanelTemplate = new Uri($"{Paths.Templates}\\ICA 8.5x11 Title Block.dwt"),
                ReferenceTemplate = new Uri($"{Paths.Templates}\\ICA 8.5x11 Title Block.dwt"),
                Library = new Uri($"{Paths.Libraries}"),
                Ladder = new LadderProperties(),
                Component = new ComponentProperties(),
                Wire = new WireProperties()
            };
            PromptKeywordOptions options = new PromptKeywordOptions("\nChoose page type: ");
            options.Keywords.Add("Schematic");
            options.Keywords.Add("Panel");
            options.Keywords.Add("Reference");
            options.Keywords.Default = "Schematic";

            PromptResult result = CurrentDocument.Editor.GetKeywords(options);
            if (result.Status != PromptStatus.OK)
                return;

            switch (result.StringResult)
            {
                case "Schematic":
                    currentProject.AddPage(Project.DrawingType.Schematic, "test.dwg");
                    break;
                case "Panel":
                    currentProject.AddPage(Project.DrawingType.Panel, "test.dwg");
                    break;
                case "Reference":
                    currentProject.AddPage(Project.DrawingType.Reference, "test.dwg");
                    break;
                default:
                    return;
            }
        }

        #endregion

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
            TitleBlock newTitleBlock = TitleBlock.Select();
            if (newTitleBlock is null)
                return;

            RemoveLadder();
            Database database = CurrentDocument.Database;
            AddTitleBlock(database, newTitleBlock);
            ZoomExtents(CurrentDocument, newTitleBlock.Reference.GeometricExtents);
        }

        public static Database LoadDatabase(Uri uri)
        {
            foreach(Document document in Application.DocumentManager)
            {
                if (document.Name == uri.LocalPath)
                    return document.Database;
            }
            Database database = new Database(false, true);
            database.ReadDwgFile(uri.LocalPath, FileShare.ReadWrite, true, null);
            return database;
        }

        [CommandMethod("PROJECTTITLEBLOCK")]
        public static void ProjectTitleBlock()
        {
            TitleBlock titleBlock = TitleBlock.Select();
            Project project = CurrentDocument.Database.GetProject();
            project.Run(AddTitleBlock, new TitleBlock(titleBlock.FileUri));
        }

        public static void AddTitleBlock(Database database, TitleBlock titleBlock)
        {
            database.GetTitleBlock()?.Purge();
            titleBlock?.Load(database);
            titleBlock?.Insert();
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

        [CommandMethod("GETWIRE", CommandFlags.UsePickSet)]
        public static void HighlightWire()
        {
            if (SelectSingleImplied()?.Open() is Line selectedLine)
                if (selectedLine.Layer == ElectricalLayers.WireLayer.Name)
                {
                    LayerTableRecord wireLayer = selectedLine.Database.GetLayer(selectedLine.Layer);
                    List<Line> potentialLines = new List<Line>();
                    foreach (ObjectId id in wireLayer.GetEntities())
                        if (id.Open() is Line line)
                            potentialLines.Add(line);
                    Wire test = new Wire()
                    {
                        Lines = selectedLine.GetConnected(potentialLines)
                    };
                    test.Highlight();
                } 
        }

        public static void ZoomExtents(Document document, Extents3d extents)
        {
            using (ViewTableRecord view = document.Editor.GetCurrentView())
            {
                view.Width = extents.MaxPoint.X - extents.MinPoint.X;
                view.Height = extents.MaxPoint.Y - extents.MinPoint.Y;
                view.CenterPoint = new Point2d(
                    (extents.MaxPoint.X + extents.MinPoint.X) / 2.0,
                    (extents.MaxPoint.Y + extents.MinPoint.Y) / 2.0);
                document.Editor.SetCurrentView(view);
            }
        }
    }
}
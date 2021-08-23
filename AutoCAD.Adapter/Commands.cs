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
using ICA.AutoCAD.Adapter.Prompt;

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
                Editor.Regen();
            }
        }
        private static bool PreviousGridSnap;
        public static short PreviousObjectSnap;

        public static Document CurrentDocument => Application.DocumentManager.MdiActiveDocument;
        public static Editor Editor => CurrentDocument.Editor;

        [CommandMethod("EDITCOMPONENT", CommandFlags.UsePickSet)]
        public static void EditSymbol(BlockReference reference = null)
        {
            ISymbol symbol;

            if (reference is null)
                symbol = Select.Symbol(Editor);
            else if (reference.HasAttributeReference("TAG1"))
                symbol = new ParentSymbol(reference);
            else
                symbol = new ChildSymbol(reference);

            switch (symbol)
            {
                case ParentSymbol parent:
                    ParentSymbolEditView editView = new ParentSymbolEditView(parent);
                    Application.ShowModalWindow(editView);
                    break;
                case ChildSymbol child:
                    ChildSymbolEditView childEditView = new ChildSymbolEditView(child);
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
                Editor.WriteMessage(ex.Message);
            }
        }

        public static string PromptSymbolName()
        {
            PromptStringOptions options = new PromptStringOptions("Enter symbol name: ")
            {
                AllowSpaces = true
            };
            PromptResult result = Editor.GetString(options);
            return result.StringResult;
        }

        [CommandMethod("ASSIGNLAYERS")]
        public static void AssignLayers()
        {
            Select.Symbol(Editor)?.AssignLayers();
        }

        [CommandMethod("UPDATETAG")]
        public static void UpdateTag()
        {
            if (Select.Symbol(Editor) is ParentSymbol symbol)
                symbol.Tag = $"{symbol.Family}{symbol.LineNumber}1";
        }

        [CommandMethod("UPDATETAG2")]
        public static void UpdateTag2()
        {
            if (Select.Symbol(Editor) is ParentSymbol symbol)
                symbol.Tag = $"{symbol.LineNumber}{symbol.Family}";
        }

        [CommandMethod("MATCHWIRES")]
        public static void Matchwires()
        {
            if (Select.Symbol(Editor) is ParentSymbol symbol)
                symbol.MatchWireNumbers();
        }

        [CommandMethod("XREFSIGS")]
        public static void XrefSignals()
        {
            if (Select.Signal(Editor) is SignalSymbol symbol1)
                if (Select.Signal(Editor) is SignalSymbol symbol2)
                    symbol1.CrossReference(symbol2);
        }

        #region Project

        [CommandMethod("CURRENTPROJECT")]
        public static void PrintCurrentProject()
        {
            Project test = CurrentProject();
            if(test != null)
            {
                Editor.WriteMessage(test.Name);
            }
            else
            {
                Editor.WriteMessage("Current document does not belong to a project.");
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

            PromptResult result = Editor.GetKeywords(options);
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

        [CommandMethod("LADDER")]
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
            try
            {
                BlockReference reference = Select.SingleImplied(Editor)?.Open() as BlockReference;

                switch (reference.Layer)
                {
                    case "LADDER":
                        //Ladder();
                        break;
                    case "TITLE BLOCK":
                        TitleBlockCommand();
                        break;
                    case "SYMS":
                        EditSymbol(reference);
                        break;
                    default:
                        Editor.SetImpliedSelection(new ObjectId[1] { reference.ObjectId });
                        CurrentDocument.SendStringToExecute("_EATTEDIT ", false, false, false);
                        return;
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Editor.WriteMessage(ex.Message);
            }
        }

        #endregion

        [CommandMethod("GETWIRE", CommandFlags.UsePickSet)]
        public static void HighlightWire()
        {
            if (Select.SingleImplied(Editor)?.Open() is Line selectedLine)
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

        [CommandMethod("DRAWWIRE")]
        public static void DrawWire()
        {
            Wire.Draw(CurrentDocument);
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
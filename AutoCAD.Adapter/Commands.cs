using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using ICA.Schematic;
using ICA.AutoCAD.Adapter.Windows.Views;
using System.Collections.Generic;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Autodesk.AutoCAD.Geometry;
using System.IO;
using System;
using ICA.AutoCAD.Adapter.Prompt;
using System.Linq;
using Autodesk.AutoCAD.Windows;
using ICA.AutoCAD.Adapter.Windows.ViewModels;
using static ICA.AutoCAD.Adapter.ConnectionPoint;

namespace ICA.AutoCAD.Adapter
{
    public static class Commands
    {
        public static Document CurrentDocument => Application.DocumentManager.MdiActiveDocument;
        public static Editor Editor => CurrentDocument.Editor;
        public static Project CurrentProject => CurrentDocument.Database.GetProject();

        private static bool? _mountMode;
        public static bool? MountMode
        {
            get
            {
                if (_mountMode is null)
                {
                    LayerTableRecord mountingLayer = CurrentDocument.Database.GetLayer("MOUNTING");
                    return !mountingLayer?.IsFrozen;
                }
                return _mountMode;
            }
            set
            {
                if (value is true)
                {
                    ShowMountingLayers();
                    PreviousGridSnap = SystemVariables.GridSnap;
                    SystemVariables.GridSnap = false;
                    PreviousObjectSnap = SystemVariables.ObjectSnap;
                    SystemVariables.ObjectSnap = ObjectSnap.Endpoint | ObjectSnap.Perpendicular;
                }
                else if (value is false)
                {
                    HideMountingLayers();
                    SystemVariables.GridSnap = PreviousGridSnap;
                    SystemVariables.ObjectSnap = PreviousObjectSnap;
                }
                _mountMode = value;
                Editor.Regen();
            }
        }
        private static bool PreviousGridSnap;
        public static ObjectSnap PreviousObjectSnap;

        /// <summary>
        /// Toggles between layer states to help with manual mounting of panel components
        /// </summary>
        [CommandMethod("MOUNT")]
        public static void ToggleMountingLayers() => MountMode ^= true;

        private static List<string> MountingLayers => new List<string>
        {
            "MOUNTING",
            "BOUNDS",
            "CLEARANCE"
        };
        private static List<string> ViewingLayers => new List<string>
        {
            "COMPONENTS",
            "WIPEOUT"
        };

        public static void HideMountingLayers()
        {
            foreach (string layerName in MountingLayers)
                CurrentDocument.Database.GetLayer(layerName).Freeze();
            foreach (string layerName in ViewingLayers)
                CurrentDocument.Database.GetLayer(layerName).Thaw();
        }

        public static void ShowMountingLayers()
        {
            foreach (string layerName in MountingLayers)
                CurrentDocument.Database.GetLayer(layerName).Thaw();
            foreach (string layerName in ViewingLayers)
                CurrentDocument.Database.GetLayer(layerName).Freeze();
        }

        [CommandMethod("EDITCOMPONENT", CommandFlags.UsePickSet)]
        public static void EditSymbol(BlockReference reference = null)
        {
            ISymbol symbol;

            if (reference is null)
                symbol = Select.Symbol(Editor);
            else if (reference.HasAttributeReference("TAG2"))
                symbol = new ChildSymbol(reference);
            else
                symbol = new ParentSymbol(reference);

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

        [CommandMethod("INSERTCOMPONENT")]
        public static void InsertSymbol() => InsertSymbol(Symbol.PromptSymbolName(Editor));

        public static void InsertSymbol(string symbolName)
        {
            Symbol symbol = SchematicSymbolRecord.GetRecord(CurrentDocument.Database, symbolName)?.InsertSymbol() as Symbol;
            if (symbol is null)
                return;

            if (symbol is ParentSymbol parent)
                parent.UpdateTag();
            symbol.AssignLayers();
            symbol.CollapseAttributeStack();
        }

        [CommandMethod("INSERTMULTIPOLE")]
        public static void InsertMultipole()
        {
            string name = Symbol.PromptSymbolName(Editor);
            PromptStringOptions options = new PromptStringOptions("Number of poles:");
            PromptResult result = Editor.GetString(options);
            if (result.Status != PromptStatus.OK)
                return;
            if (int.TryParse(result.StringResult, out int poles))
            {
                SchematicSymbolRecord record = SchematicSymbolRecord.GetRecord(CurrentDocument.Database, name);
                if (record is null)
                    return;

                ParentSymbol parent = record.InsertSymbol(new Point2d(), Symbol.Type.Parent) as ParentSymbol;
                if (parent is null)
                    return;

                List<Symbol> symbols = new List<Symbol> { parent };
                for (int i = 1; i < poles; i++)
                {
                    symbols.Add(record.InsertSymbol(new Point2d(symbols.Last().Position.X, symbols.Last().Position.Y - CurrentDocument.Database.GetLadder().LineHeight), Symbol.Type.Child) as ChildSymbol);
                }
                parent.UpdateTag();
                symbols.ForEach(symbol =>
                { 
                    symbol.AssignLayers(); 
                    symbol.CollapseAttributeStack();
                });
                symbols.OfType<ChildSymbol>().ToList().ForEach(child => child.SetParent(parent));
                Symbol.Link(symbols);
            }
        }

        [CommandMethod("ASSIGNLAYERS")]
        public static void AssignLayers() => Select.Symbol(Editor)?.AssignLayers();

        [CommandMethod("UPDATETAGS")]
        public static void UpdateTag() => Select.Symbols(Editor).Where(symbol => symbol is ParentSymbol parent)
                                                                .ToList()
                                                                .ForEach(symbol => ((ParentSymbol)symbol).UpdateTag());

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

        [CommandMethod("UPDATELAYERS")]
        public static void UpdateLayers() => ElectricalLayers.Update(CurrentDocument.Database);

        [CommandMethod("FINDPARENT")]
        public static void FindParent()
        {
            if (Select.Symbol(Editor) is ChildSymbol symbol)
            {
                foreach (BlockReference reference in symbol.Database.GetLayer("SYMS").GetEntities().Where(entity => entity is BlockReference))
                {
                    ParentSymbol parent = new ParentSymbol(reference);
                    if (parent.Tag == symbol.Tag)
                        symbol.Xref = parent.LineNumber;
                }
            }
        }

        [CommandMethod("SETPARENT")]
        public static void SetParent()
        {
            if (Select.Symbol(Editor, "Select parent:") is ParentSymbol parent)
                foreach (ChildSymbol child in Select.Symbols(Editor, "Select children:"))
                    if (parent.Family == child.Family)
                    {
                        child.Tag = parent.Tag;
                        child.Xref = parent.LineNumber;
                        child.Description = parent.Description;
                    }
        }

        [CommandMethod("LINK")]
        public static void Link() => Symbol.Link(Select.Symbols(Editor, "Select symbols:")
                                                       .OfType<Symbol>()
                                                       .ToList());

        [CommandMethod("HIDETAG")]
        public static void HideTags()
        {
            foreach (ChildSymbol symbol in Select.Symbols(Editor))
            {
                symbol.TagHidden ^= true;
                symbol.CollapseAttributeStack();
            }
        }

        [CommandMethod("SETTYPE")]
        public static void SetType()
        {
            Select.Symbol(Editor);
        }

        [CommandMethod("SELECTCOMPONENT")]
        public static void SelectComponent()
        {
            var test = Select.Component(Editor);
        }

        [CommandMethod("TOGGLEOVERRULES")]
        public static void EnableOverrule()
        {
            Overrule.Overruling = true;
            //Overrule.AddOverrule(RXObject.GetClass(typeof(Group)), new SymbolGripOverrule(), false);
            Editor.WriteMessage(Overrule.Overruling ? "\nOverrules enabled." : "\nOverrules disabled.");
        }

        #region Project

        [CommandMethod("IMPORTPROJECT")]
        public static void ImportProject()
        {
            OpenFileDialog test = new OpenFileDialog("Load Project File", "", "", "", OpenFileDialog.OpenFileDialogFlags.AllowFoldersOnly);
            test.ShowDialog();
            if (test.Filename != "")
            {
                using (Project project = Project.Import(test.Filename))
                {
                    if (project is null)
                        return;

                    project.Drawings.ForEach(drawing => drawing.RemoveDescription(project.Job.Name.ToUpper()));
                    project.Drawings.ForEach(drawing => drawing.RemoveDescription(""));
                    project.Save();
                }
            }
        }

        //[CommandMethod("EXPORTCURRENTPROJECT")]
        //public static void ExportCurrentProject() => WDP.Export(CurrentProject, "C:\\Users\\cmotherseadicacontro\\Documents\\test.wdp");

        [CommandMethod("ADDPAGE")]
        public static void ProjectAddPage() => CurrentProject.AddPage(Project.PromptDrawingType(Editor));

        [CommandMethod("COMPONENTS")]
        public static void Components()
        {
            OpenFileDialog test = new OpenFileDialog("Load Project File", "", "", "", OpenFileDialog.OpenFileDialogFlags.AllowFoldersOnly);
            test.ShowDialog();
            if (test.Filename != "")
            {
                using (Project project = Project.Open(test.Filename))
                {
                    if (project is null)
                        return;

                    var test1 = project.Components.ToList();
                    var test2 = new ComponentsListView(test1);
                    Application.ShowModalWindow(test2);
                    var test3 = ((ComponentsListViewModel)test2.DataContext).SelectedComponent;
                }
            }
        }

        [CommandMethod("NEXTDRAWING", CommandFlags.Session)]
        public static void Next()
        {
            int nextIndex = CurrentProject.Drawings.FindIndex(drawing => drawing.FullPath == CurrentDocument.Name) + 1;
            if (nextIndex >= CurrentProject.Drawings.Count)
            {
                Editor.WriteMessage("\nProject contains no more drawings.");
                return;
            }
            ChangeDrawing(CurrentProject.Drawings[nextIndex]);
        }

        [CommandMethod("PREVIOUSDRAWING", CommandFlags.Session)]
        public static void Previous()
        {
            int previousIndex = CurrentProject.Drawings.FindIndex(drawing => drawing.FullPath == CurrentDocument.Name) - 1;
            if (previousIndex < 0)
            {
                Editor.WriteMessage("\nNo previous drawing.");
                return;
            }
            ChangeDrawing(CurrentProject.Drawings[previousIndex]);
        }

        [CommandMethod("FIRSTDRAWING", CommandFlags.Session)]
        public static void First() => ChangeDrawing(CurrentProject.Drawings.First());

        [CommandMethod("LASTDRAWING", CommandFlags.Session)]
        public static void Last() => ChangeDrawing(CurrentProject.Drawings.Last());

        public static void ChangeDrawing(Drawing changeTo)
        {
            CurrentDocument.CloseAndSave(CurrentDocument.Name);
            Application.DocumentManager.Open(changeTo.FullPath, false);
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
            foreach (Document document in Application.DocumentManager)
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
        }

        public static void AddTitleBlock(Database database, TitleBlock titleBlock)
        {
            database.GetTitleBlock()?.Purge();
            titleBlock?.Load(database);
            titleBlock?.Insert();
        }

        [CommandMethod("REMOVETITLEBLOCK")]
        public static void PurgeTitleBlock() => CurrentDocument.Database.GetTitleBlock()?.Purge();

        [CommandMethod("TITLEBLOCKCONFIG")]
        public static void ConfigureTitleBlock() { }

        [CommandMethod("SPARE")]
        public static void SpareSheet()
        {
            CurrentDocument.Database.GetTitleBlock().Spare ^= true;
            Editor.Regen();
        }

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

        #region Wire

        [CommandMethod("GETWIRE", CommandFlags.UsePickSet)]
        public static void HighlightWire()
        {
            if (Select.SingleImplied(Editor)?.Open() is Line selectedLine)
                if (selectedLine.Layer == ElectricalLayers.WireLayer.Name)
                {
                    List<Line> potentialLines = selectedLine.Database.GetEntities()
                                                                     .OfType<Line>()
                                                                     .Where(line => line.Layer == ElectricalLayers.WireLayer.Name)
                                                                     .ToList();
                    Wire wire = new Wire()
                    {
                        Lines = selectedLine.GetConnected(potentialLines)
                    };
                    wire.Highlight();
                }
        }

        [CommandMethod("DRAWWIRE")]
        public static void DrawWire() => Wire.Draw(CurrentDocument);

        #endregion

        [CommandMethod("INSERTSIGNAL")]
        public static void InsertSignal()
        {
            PromptPointOptions options = new PromptPointOptions("Select a point");
            PromptPointResult result = Editor.GetPoint(options);

            if (result.Status != PromptStatus.OK)
                return;

            Point3d point = result.Value;
            var entity = CurrentDocument.Database.GetEntities()
                                                 .OfType<Line>()
                                                 .Where(line => line.Layer == ElectricalLayers.WireLayer.Name)
                                                 .Select(line => new { Line = line, Dist = line.GetClosestPointTo(point, false).DistanceTo(point) })
                                                 .Aggregate((l1, l2) => l1.Dist < l2.Dist ? l1 : l2);

            if (entity != null)
                entity.Line.Highlight();
            //snap signal to nearest wire end as jig...

            //then insert
        }

        [CommandMethod("GROUND")]
        public static void InsertGround() => GroundSymbol.Insert(CurrentDocument)
                                                         .GroundConnectedWires();

        [CommandMethod("TESTPREFERENCES")]
        public static void TestPrefs()
        {
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
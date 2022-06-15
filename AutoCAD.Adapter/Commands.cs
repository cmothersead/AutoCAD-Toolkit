using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using ICA.AutoCAD.Adapter.Prompt;
using ICA.AutoCAD.Adapter.Windows.ViewModels;
using ICA.AutoCAD.Adapter.Windows.Views;
using ICA.Schematic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace ICA.AutoCAD.Adapter
{
    public static class Commands
    {
        #region Fields

        #region Private Fields

        private static bool? _mountMode;
        private static bool PreviousGridSnap;
        private static ObjectSnap PreviousObjectSnap;
        private static Project _currentProject;
        private static Drawing _currentDrawing;

        #endregion

        #endregion

        #region Properties

        #region Public Properties

        #region Public Static Properties

        public static Document CurrentDocument => Application.DocumentManager.MdiActiveDocument;
        public static Database CurrentDatabase => CurrentDocument.Database;
        public static Editor Editor => CurrentDocument.Editor;
        public static Project CurrentProject
        {
            get
            {
                if(_currentProject is null)
                    _currentProject = CurrentDatabase.GetProject();

                if (!_currentProject.Contains(CurrentDatabase.OriginalFileName))
                    _currentProject = CurrentDatabase.GetProject();

                return _currentProject;
            }
        }
        public static Drawing CurrentDrawing
        {
            get
            {
                if(_currentDrawing is null)
                    _currentDrawing = CurrentDatabase.GetDrawing(CurrentProject);

                if (_currentDrawing.FullPath != CurrentDatabase.OriginalFileName)
                    _currentDrawing = CurrentDatabase.GetDrawing(CurrentProject);

                return _currentDrawing;
            }
        }

        public static bool? MountMode
        {
            get
            {
                if (_mountMode is null)
                {
                    LayerTableRecord mountingLayer = CurrentDatabase.GetLayer("MOUNTING");
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

        public static bool SkipSpares { get; set; } = true;

        #endregion

        #endregion

        #endregion

        #region Methods

        #region Mounting

        /// <summary>
        /// Toggles between layer states to help with manual mounting of panel components
        /// </summary>
        [CommandMethod("MOUNT")]
        public static void ToggleMountingLayers() => MountMode ^= true;

        private static List<LayerTableRecord> MountingLayers => CurrentDatabase.GetLayers(new List<LayerTableRecord>
        {
            ElectricalLayers.MountingLayer,
            ElectricalLayers.BoundsLayer,
            ElectricalLayers.ClearanceLayer
        });

        private static List<LayerTableRecord> ViewingLayers => CurrentDatabase.GetLayers(new List<LayerTableRecord>
        {
            ElectricalLayers.ComponentsLayer,
            ElectricalLayers.WipeoutLayer
        });

        public static void HideMountingLayers()
        {
            MountingLayers.ForEach(layer => layer.Freeze());
            ViewingLayers.ForEach(layer => layer.Thaw());
        }

        public static void ShowMountingLayers()
        {
            MountingLayers.ForEach(layer => layer.Thaw());
            ViewingLayers.ForEach(layer => layer.Freeze());
        }

        #endregion

        #region Components

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

        [CommandMethod("INSERTMULTIPOLE")]
        public static void InsertMultipole()
        {
            string name = Symbol.PromptSymbolName(Editor);

            if (name is null)
                return;

            PromptStringOptions options = new PromptStringOptions("Number of poles:");
            PromptResult result = Editor.GetString(options);
            if (result.Status != PromptStatus.OK)
                return;
            if (int.TryParse(result.StringResult, out int poles))
            {
                SchematicSymbolRecord record = SchematicSymbolRecord.GetRecord(CurrentDatabase, name);
                if (record is null)
                    return;

                ParentSymbol parent = record.InsertSymbol(CurrentDocument, Symbol.Type.Parent) as ParentSymbol;
                if (parent is null)
                    return;

                List<Symbol> symbols = new List<Symbol> { parent };
                for (int i = 1; i < poles; i++)
                {
                    //LineHeight needs to be a database variable not a ladder-specific value
                    symbols.Add(record.InsertSymbol(new Point2d(symbols.Last().Position.X, symbols.Last().Position.Y - CurrentDatabase.GetLadders().First().LineHeight), Symbol.Type.Child) as ChildSymbol);
                }
                parent.UpdateTag();
                symbols.ForEach(symbol =>
                {
                    symbol.AssignLayers();
                    symbol.CollapseAttributeStack();
                    symbol.BreakWires();
                });
                symbols.OfType<ChildSymbol>().ForEach(child => child.SetParent(parent));
                Symbol.Link(CurrentDatabase, symbols);
            }
        }

        [CommandMethod("INSERTCOMPONENT")]
        public static void InsertSymbol() => InsertSymbol(Symbol.PromptSymbolName(Editor));

        [CommandMethod("DROP")]
        public static void DropPower() => InsertSymbol("WD_DROP");

        public static void InsertSymbol(string symbolName)
        {
            Symbol symbol = SchematicSymbolRecord.GetRecord(CurrentDatabase, symbolName)?.InsertSymbol(CurrentDocument) as Symbol;
            if (symbol is null)
                return;

            if (symbol is ParentSymbol parent)
            {
                if (parent.Family == "")
                {
                    PromptResult result = Editor.GetString("Enter family code:");
                    if (result.Status == PromptStatus.OK)
                        parent.Family = result.StringResult;
                }
                parent.UpdateTag();
            }
            symbol.AssignLayers();
            symbol.CollapseAttributeStack();
            symbol.BreakWires();
        }

        [CommandMethod("UPDATETAGS")]
        public static void UpdateTags() => Select.Symbols(Editor)
                                                 .Where(symbol => symbol is ParentSymbol parent)
                                                 .ForEach(symbol => ((ParentSymbol)symbol).UpdateTag());

        [CommandMethod("UPDATETAG")]
        public static void UpdateTag()
        {
            if (Select.Component(Editor, "Select component:") is Component component)
                component.UpdateTag();
        }

        [CommandMethod("XREFSIGS")]
        public static void XrefSignals()
        {
            if (Select.Signal(Editor) is SignalSymbol symbol1)
                if (Select.Signal(Editor) is SignalSymbol symbol2)
                    symbol1.CrossReference(symbol2);
        }

        [CommandMethod("UPDATELAYERS")]
        public static void UpdateLayers() => ElectricalLayers.Update(CurrentDatabase);

        [CommandMethod("LINK")]
        public static void Link() => Symbol.Link(CurrentDatabase, Select.Symbols(Editor, "Select symbols:")?
                                                                        .OfType<Symbol>()
                                                                        .ToList());

        #endregion

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

        [CommandMethod("SAVEXML")]
        public static void SaveProject()
        {
            CurrentProject.Save();
        }

        [CommandMethod("SAVEPROJECT")]
        public static void SaveProjectJson()
        {
            CurrentProject.SaveAsJSON();
        }

        //[CommandMethod("EXPORTCURRENTPROJECT")]
        //public static void ExportCurrentProject() => WDP.Export(CurrentProject, "C:\\Users\\cmotherseadicacontro\\Documents\\test.wdp");

        [CommandMethod("ADDPAGE")]
        public static void ProjectAddPage() => CurrentProject.AddPage(Project.PromptDrawingType(Editor));

        [CommandMethod("COMPONENTS")]
        public static void Components()
        {
            OpenFileDialog test = new OpenFileDialog("Select Project Folder", "", "", "", OpenFileDialog.OpenFileDialogFlags.AllowFoldersOnly);
            test.ShowDialog();
            if (test.Filename != "")
            {
                using (Project project = Project.OpenXML(test.Filename))
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

        [CommandMethod("DRAWINGCOMPONENTS")]
        public static void DrawingComponents()
        {
            Drawing currentDrawing = CurrentDatabase.GetDrawing();
            currentDrawing.LogSymbols();
            var test2 = new ComponentsListView(currentDrawing.Components.ToList());
            Application.ShowModalWindow(test2);
            var test3 = ((ComponentsListViewModel)test2.DataContext).SelectedComponent;
        }

        [CommandMethod("SKIPSPARES")]
        public static void ToggleSkipSpares()
        {
            SkipSpares ^= true;
            if (SkipSpares)
                Editor.WriteMessage("Now skipping spare sheets.");
            else
                Editor.WriteMessage("No longer skipping spare sheets.");
        }

        [CommandMethod("NEXTDRAWING", CommandFlags.Session)]
        public static void Next()
        {
            IEnumerable<Drawing> drawings = CurrentProject.Drawings;

            if(SkipSpares)
                drawings = drawings.Where(drawing => !drawing.Spare);

            Drawing nextDrawing = drawings.SkipWhile(drawing => int.Parse(drawing.PageNumber) <= int.Parse(CurrentDrawing.PageNumber))
                                          .FirstOrDefault();

            ChangeDrawing(nextDrawing);
        }

        [CommandMethod("PREVIOUSDRAWING", CommandFlags.Session)]
        public static void Previous()
        {
            IEnumerable<Drawing> drawings = CurrentProject.Drawings;

            if (SkipSpares)
                drawings = drawings.Where(drawing => !drawing.Spare);

            Drawing previousDrawing = drawings.Reverse()
                                              .SkipWhile(drawing => int.Parse(drawing.PageNumber) >= int.Parse(CurrentDrawing.PageNumber))
                                              .FirstOrDefault();

            ChangeDrawing(previousDrawing);
        }

        [CommandMethod("FIRSTDRAWING", CommandFlags.Session)]
        public static void First() => ChangeDrawing(CurrentProject.Drawings.First());

        [CommandMethod("LASTDRAWING", CommandFlags.Session)]
        public static void Last() => ChangeDrawing(CurrentProject.Drawings.Last());

        public static void ChangeDrawing(Drawing changeTo)
        {
            if (changeTo is null)
                return;

            if (Application.DocumentManager.Contains(changeTo.FileUri))
            {
                Document drawing = Application.DocumentManager.Get(changeTo.FullPath);
                Application.DocumentManager.MdiActiveDocument = drawing;
            }
            else
            {
                Application.DocumentManager.Open(changeTo.FullPath, false);
            }
        }

        [CommandMethod("GOTODRAWING", CommandFlags.Session)]
        public static void GoToDrawing()
        {
            PromptResult result = Editor.GetString("Sheet Number:");

            if (result.Status != PromptStatus.OK)
                return;

            Drawing drawing = GetDrawing(result.StringResult, CurrentProject);

            ChangeDrawing(drawing);
        }

        public static Drawing GetDrawing(string input, Project project)
        {
            if (!int.TryParse(input, out int sheetNumber))
                throw new ArgumentException("Invalid input. Must be a number.");

            if (sheetNumber < 1 || sheetNumber > project.SheetCount)
                throw new ArgumentOutOfRangeException("input", "Invalid input. Page number out of range.");

            Drawing found = project.GetDrawing(drawing => int.Parse(drawing.PageNumber) == sheetNumber);

            if (found is null)
                throw new ArgumentException("Page number not found within project.");

            return found;
        }

        [CommandMethod("PAGENUMBERS")]
        public static void PageNumbers() => CurrentProject.RunOnAllDrawings(drawing => drawing.UpdatePageNumber());

        [CommandMethod("PAGENUMBER")]
        public static void PageNumber() => CurrentDatabase.GetDrawing().UpdatePageNumber();

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
            Ladder.RemoveFrom(CurrentDatabase);
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
            Database database = CurrentDatabase;
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

        [CommandMethod("TITLEBLOCKSETTINGS")]
        public static void TitleBlockSettings()
        {
            TitleBlock titleBlock = CurrentDatabase.GetTitleBlock();
            TitleBlockSettingsView view = new TitleBlockSettingsView(titleBlock);
            Application.ShowModalWindow(view);
        }

        [CommandMethod("PROJECTTITLEBLOCK", CommandFlags.Session)]
        public static void ProjectTitleBlock()
        {
            //TitleBlock titleBlock = TitleBlock.Select();
            CurrentProject.RunOnAllDrawings(drawing => drawing.UpdateTitleBlock());
        }

        public static void AddTitleBlock(Database database, TitleBlock titleBlock)
        {
            database.GetTitleBlock()?.Purge();
            titleBlock?.Load(database);
            titleBlock?.Insert();
        }

        [CommandMethod("REMOVETITLEBLOCK")]
        public static void PurgeTitleBlock() => CurrentDatabase.GetTitleBlock()?.Purge();


        [CommandMethod("UPDATETITLEBLOCK")]
        public static void UpdateTitleBlock()
        {
            Drawing drawing = new Drawing()
            {
                Name = Path.GetFileNameWithoutExtension(CurrentDocument.Name),
                Project = CurrentProject,
                TitleBlockAttributes = CurrentProject.Settings.TitleBlockAttributes
            };
            drawing.UpdateTitleBlock();
        }

        [CommandMethod("SPARE")]
        public static void SpareSheet()
        {
            CurrentDrawing.Spare ^= true;
            Editor.Regen();
        }

        [CommandMethod("DESCRIPTION")]
        public static void SetDrawingDescription()
        {
            PromptStringOptions options = new PromptStringOptions("Value:")
            {
                AllowSpaces = true,
            };
            PromptResult result = Editor.GetString(options);

            if (result.Status != PromptStatus.OK)
                return;

            CurrentDrawing.Description = new List<string> { result.StringResult };

            CurrentDrawing.Save();
        }

        #endregion

        #region Multiplexers

        [CommandMethod("BLOCKEDIT", CommandFlags.UsePickSet | CommandFlags.Redraw | CommandFlags.Session)]
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
                        TitleBlockSettings();
                        break;
                    case "SYMS":
                        using (DocumentLock docLock = CurrentDocument.LockDocument())
                            EditSymbol(reference);
                        break;
                    case "SIGNAL":
                        FollowSignal(reference);
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
            //TODO: snap signal to nearest wire end as jig...
            PromptPointOptions options = new PromptPointOptions("Select a point");
            PromptPointResult result = Editor.GetPoint(options);

            if (result.Status != PromptStatus.OK)
                return;

            Point3d point = result.Value;
            var entity = CurrentDatabase.GetEntities()
                                        .OfType<Line>()
                                        .Where(line => line.Layer == ElectricalLayers.WireLayer.Name || line.Layer == ElectricalLayers.LadderLayer.Name)
                                        .Select(line => new { Line = line, Dist = line.GetClosestPointTo(point, false).DistanceTo(point) })
                                        .Aggregate((l1, l2) => l1.Dist < l2.Dist ? l1 : l2);


            Point3d close = entity.Line.StartPoint.DistanceTo(point) > entity.Line.EndPoint.DistanceTo(point) ? entity.Line.EndPoint : entity.Line.StartPoint;
            Point3d far = entity.Line.StartPoint.DistanceTo(point) > entity.Line.EndPoint.DistanceTo(point) ? entity.Line.StartPoint : entity.Line.EndPoint;

            int direction;

            double angle = close == entity.Line.StartPoint ? (entity.Line.Angle) % (2 * Math.PI) : (entity.Line.Angle + Math.PI) % (2 * Math.PI);

            switch (angle)
            {
                case Math.PI / 2:
                    direction = 4;
                    break;
                case Math.PI:
                    direction = 1;
                    break;
                case Math.PI * 3 / 2:
                    direction = 2;
                    break;
                default:
                    direction = 3;
                    break;
            }

            SignalSymbol.Insert(CurrentDatabase, close.ToPoint2D(), "1", direction).AssignLayers();
        }

        //[CommandMethod("MOVESIGNALS")]
        //public static void MoveSignals()
        //{
        //    CurrentProject.Drawings.ForEach(drawing =>
        //        {
        //            LayerTableRecord signalLayer = drawing.Database.GetLayer(ElectricalLayers.SignalLayer);
        //            if(!signalLayer.IsErased)
        //                signalLayer.UnlockWithoutWarning();
        //            drawing.Database.GetEntities()
        //                       .OfType<BlockReference>()
        //                       .Where(blockReference => blockReference.Name.StartsWith("HA1"))
        //                       .ForEach(blockReference => blockReference.SetLayer(ElectricalLayers.SignalLayer));
        //            if(!signalLayer.IsErased)
        //                signalLayer.LockWithWarning();
        //        });
        //}

        public static void FollowSignal(BlockReference reference)
        {
            SignalSymbol signal = new SignalSymbol(reference);
            if (int.TryParse(signal.XrefSheet, out int sheet))
                ChangeDrawing(CurrentProject.Drawings[sheet - 1]);
        }

        [CommandMethod("TOGGLEOVERRULES")]
        public static void EnableOverrule()
        {
            Overrule.RemoveOverrule(RXObject.GetClass(typeof(Entity)), new EraseLinksOverrule());
            //Overrule.Overruling = false;
            //Editor.WriteMessage(Overrule.Overruling ? "\nOverrules enabled." : "\nOverrules disabled.");
        }

        [CommandMethod("GROUND")]
        public static void InsertGround() => GroundSymbol.Insert(CurrentDocument)
                                                         .GroundConnectedWires();

        [CommandMethod("SCOOT")]
        public static void Scoot() => SymbolJig.Run(CurrentDocument, Select.Symbol(Editor) as Symbol);

        [CommandMethod("TOGGLEWIREBREAK")]
        public static void ToggleWireBreak()
        {
            var symbol = Select.Symbol(Editor) as Symbol;
            if (!symbol.IsInline)
                return;

            if (symbol.WireConnections.Any(connection => CurrentDatabase.GetLayer(ElectricalLayers.WireLayer).GetEntities().OfType<Line>().Any(line => connection.IsConnected(line))))
                symbol.UnbreakWires();
            else
                symbol.BreakWires();
        }

        [CommandMethod("GETXDATA")]
        public static void GetXData()
        {
            var test = Select.SingleImplied(Editor);

            if (test is null)
                return;

            Entity entity = test.Value.Open() as Entity;

            entity.GetXData().ForEach(data => Editor.WriteMessage($"{data.Value}"));
        }

        [CommandMethod("CLEARXDATA")]
        public static void ClearXData()
        {
            Application.ShowAlertDialog("Removing XData can cause unexpected behavior");
            Select.Multiple(Editor, "Select objects to clear XData from:")?.ForEach(obj => obj.ClearXData());
        }

        [CommandMethod("GETLINKED")]
        public static void GetLinked()
        {
            Entity entity = Select.SingleImplied(Editor, "Select linked object:")?.Open() as Entity;
            if (entity is null)
                return;
            GetLinked(entity).ForEach(ent => ent.Highlight());
        }

        [CommandMethod("ADDDOUBLECLICK", CommandFlags.Session)]
        public static void AddDoubleClick()
        {
            Application.BeginDoubleClick += Application_BeginDoubleClick;
        }

        [CommandMethod("REMOVEDOUBLECLICK")]
        public static void RemoveDoubleClick()
        {
            Application.BeginDoubleClick -= Application_BeginDoubleClick;
        }

        private static void Application_BeginDoubleClick(object sender, BeginDoubleClickEventArgs e)
        {
            var objects = Select.Implied(Editor);
            ParentSymbol parent = objects.OfType<BlockReference>()
                                         .Where(reference => reference.HasAttributeReference("TAG1"))
                                         .Select(reference => new ParentSymbol(reference))
                                         .FirstOrDefault();
            using (DocumentLock doclock = CurrentDocument.LockDocument())
            {
                ParentSymbolEditView editView = new ParentSymbolEditView(parent);
                Application.ShowModalWindow(editView);
            }
        }

        public static List<Entity> GetLinked(Entity entity)
        {
            Graph<EntityNode, Entity> linkGraph = new Graph<EntityNode, Entity>(new EntityNode(entity));
            return linkGraph.Nodes.Select(node => node.Value).ToList();
        }

        [CommandMethod("LOGSYMBOLS")]
        public static void LogSymbols(Database database) => CurrentDatabase.GetProject().Drawings
                                                                           .ForEach(drawing => drawing.LogSymbols());

        [CommandMethod("GETSYMBOLS")]
        public static void GetSymbols()
        {
            var test1 = CurrentDatabase.GetParentSymbols();
            test1.ForEach(symbol => symbol.BlockReference.Highlight());
            var test2 = CurrentDatabase.GetChildSymbols();
            test2.ForEach(symbol => symbol.BlockReference.Highlight());
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

        #endregion
    }
}
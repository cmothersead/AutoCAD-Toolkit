using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows;
using ICA.AutoCAD.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static ICA.AutoCAD.Adapter.ConnectionPoint;
using EntityGroup = Autodesk.AutoCAD.DatabaseServices.Group;

namespace ICA.AutoCAD.Adapter
{
    public abstract class Symbol
    {
        #region Fields

        #endregion

        #region Properties

        #region Private Properties

        private AttributeReference FamilyAttribute => BlockReference.GetAttributeReference("FAMILY");

        #endregion

        #region Protected Properties

        public BlockReference BlockReference { get; }

        protected virtual AttributeReference TagAttribute { get; }

        protected List<AttributeReference> DescAttributes
        {
            get
            {
                int index = 1;
                List<AttributeReference> list = new List<AttributeReference>();
                AttributeReference attributeReference;
                do
                {
                    attributeReference = BlockReference.GetAttributeReference($"DESC{index}");
                    if (attributeReference != null)
                    {
                        list.Add(attributeReference);
                        index++;
                    }
                } while (attributeReference != null);
                return list;
            }
        }

        protected static Dictionary<string, LayerTableRecord> AttributeLayers => new Dictionary<string, LayerTableRecord>()
        {
            { "TAG", ElectricalLayers.TagLayer },
            { "MFG", ElectricalLayers.ManufacturerLayer },
            { "CAT", ElectricalLayers.PartNumberLayer },
            { "TERMDESC", ElectricalLayers.MiscellaneousLayer },
            { "DESC", ElectricalLayers.DescriptionLayer },
            { "TERM", ElectricalLayers.TerminalLayer },
            { "CON", ElectricalLayers.ConductorLayer },
            { "RATING", ElectricalLayers.RatingLayer },
            { "WIRENO", ElectricalLayers.WireNumberLayer },
            { "XREF", ElectricalLayers.XrefLayer }
        };

        protected AttributeStack Stack { get; }

        #endregion

        #region Public Properties

        public Handle Handle => BlockReference.Handle;

        public Component Component { get; set; }

        public Database Database => BlockReference.Database;

        public Project Project => Database.GetProject();

        public Drawing Drawing => Project.Drawings.FirstOrDefault(drawing => drawing.Name == Path.GetFileNameWithoutExtension(Database.OriginalFileName));

        public string Family
        {
            get => FamilyAttribute?.TextString;
            set => FamilyAttribute?.SetValue(value);
        }

        public string FamilyView => Project.Settings.FamilyCodes?.SingleOrDefault(family => family.Replace == Family)?.Code ?? Family;

        public string Tag
        {
            get => TagAttribute?.TextString;
            set => TagAttribute?.SetValue(value);
        }

        public bool TagHidden
        {
            get => TagAttribute != null && TagAttribute.Invisible;
            set => TagAttribute?.SetVisibility(!value);
        }

        public List<string> Description
        {
            get => DescAttributes.Select(a => a.TextString)
                                 .ToList();
            set
            {
                if (value.Count == 0)
                    value.Add("");

                while (DescAttributes.Count != value.Count)
                {
                    if (DescAttributes.Count > value.Count)
                        Stack.Remove($"DESC{DescAttributes.Count}");
                    else
                        Stack.Add($"DESC{DescAttributes.Count + 1}");
                }
                int position = 0;
                value.ForEach(val => DescAttributes[position++].SetValue(val));
            }
        }

        public bool DescriptionHidden
        {
            get => DescAttributes.Count != 0 && DescAttributes[0].Invisible;
            set => DescAttributes.ForEach(a => a?.SetVisibility(!value));
        }

        public Point2d Position => BlockReference.Position.ToPoint2D();

        public string LineNumber => BlockReference.Database.GetLadder()?.ClosestLineNumber(BlockReference.Position);

        public string SheetNumber => BlockReference.Database.GetSheetNumber();

        public List<WireConnection> WireConnections => (BlockReference.BlockTableRecord.Open() as BlockTableRecord).GetAttributeDefinitions()
                                                                     .Where(definition => Regex.IsMatch(definition.Tag, @"X[1,2,4,8]TERM\d{2}"))
                                                                     .Select(definition => new WireConnection(BlockReference, definition))
                                                                     .ToList();

        public List<LinkConnection> LinkConnections => (BlockReference.BlockTableRecord.Open() as BlockTableRecord).GetAttributeDefinitions()
                                                                     .Where(definition => Regex.IsMatch(definition.Tag, @"X[1,2,4,8]LINK"))
                                                                     .Select(definition => new LinkConnection(BlockReference, definition))
                                                                     .ToList();

        public List<Line> ConnectedWires => WireConnections.SelectMany(connection => connection.Owner.Database.GetEntities()
                                                                                                              .Where(entity => entity.Layer == ElectricalLayers.WireLayer.Name)
                                                                                                              .OfType<Line>()
                                                                                                              .Where(line => connection.IsConnected(line))).ToList();

        public bool IsInline
        {
            get
            {
                if (WireConnections.Count != 2)
                    return false;

                return WireConnections[0].IsPairWith(WireConnections[1]);
            }
        }

        public bool IsInserted => Database != null;

        #endregion

        #endregion

        #region Constructors

        public Symbol(BlockReference blockReference)
        {
            BlockReference = blockReference;
            Stack = new AttributeStack(BlockReference, AttributeLayers);
            if (Database != null)
            {
                Stack.Position = FamilyAttribute.Justify == AttachmentPoint.BaseLeft ?
                           FamilyAttribute.Position.ToPoint2D() :
                           FamilyAttribute.AlignmentPoint.ToPoint2D();
                Stack.Justification = FamilyAttribute.Justify;
            }
        }

        #endregion

        #region Methods

        #region Protected Methods

        protected void AddToDictionary(string name)
        {
            if (Database is null)
                return;

            if (!Database.GetNamedDictionary(name).Contains(BlockReference.Handle.ToString()))
                using (Transaction transaction = Database.TransactionManager.StartTransaction())
                {
                    Database.GetNamedDictionary(transaction, name)
                            .GetForWrite(transaction)
                            .SetAt(BlockReference.Handle.ToString(), BlockReference.GetForWrite(transaction));
                    transaction.Commit();
                }
        }

        #endregion

        #region Public Methods

        public SymbolJig GetJig(Document document, Transaction transaction) => IsInserted ? new SymbolJig(document.Editor.CurrentUserCoordinateSystem,
                                                                                                          transaction,
                                                                                                          BlockReference.GetForWrite(transaction),
                                                                                                          "Specify new location:") : null;

        public virtual bool Insert(Transaction transaction, Database database)
        {
            if (IsInserted)
                return false;

            BlockReference.Insert(transaction, database, ElectricalLayers.SymbolLayer);

            Stack.Position = FamilyAttribute.Justify == AttachmentPoint.BaseLeft ?
                           FamilyAttribute.Position.ToPoint2D() :
                           FamilyAttribute.AlignmentPoint.ToPoint2D();
            Stack.Justification = FamilyAttribute.Justify;

            return true;
        }

        public bool Insert(Database database)
        {
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                bool result = Insert(transaction, database);
                transaction.Commit();
                return result;
            }
        }

        public void CollapseAttributeStack() => Stack.Collapse();

        public void AssignLayers()
        {
            using (Transaction transaction = BlockReference.Database.TransactionManager.StartTransaction())
            {
                BlockReference.GetAttributeReferences(transaction)
                              .ForEach(reference => reference.SetLayer(AttributeLayers.FirstOrDefault(pair => reference.Tag.Contains(pair.Key)).Value));
                transaction.Commit();
            }
        }

        public void BreakWires()
        {
            List<Line> forDelete = new List<Line>();
            foreach (WireConnection point in WireConnections)
            {
                List<Line> list = point.Owner.Database.GetEntities()
                                                        .Where(entity => entity.Layer == ElectricalLayers.WireLayer.Name)
                                                        .OfType<Line>()
                                                        .Where(line => line.IsOn(point.Location) && point.IsAligned(line.Angle))
                                                        .ToList();


                foreach (Line line in list)
                {
                    if (line.IsOn(point.Location))
                    {
                        var test = line.GetSplitCurves(new Point3dCollection() { point.Location.ToPoint3d() }).OfType<Line>().ToList();
                        foreach (Line splitLine in test)
                        {
                            if (point.IsConnected(splitLine))
                            {
                                splitLine.Insert(ElectricalLayers.WireLayer);
                                if (!forDelete.Contains(line))
                                    forDelete.Add(line);
                            }
                        }
                    }
                }
            }
            forDelete.ForEach(line => line.EraseObject());
        }

        public void UnbreakWires()
        {
            if (!IsInline)
                return;

            Line lineBetween = new Line(WireConnections.First().Location.ToPoint3d(), WireConnections.Last().Location.ToPoint3d());
            lineBetween.JoinEntities(ConnectedWires.ToArray());
            lineBetween.Insert(ElectricalLayers.WireLayer);
            ConnectedWires.ForEach(line => line.EraseObject());
        }

        public void Scoot(double magnitude)
        {
            Matrix3d transformation = Matrix3d.Displacement(new Vector3d(magnitude, 0, 0));

            foreach (Line wire in ConnectedWires)
            {
                if (WireConnections.Any(connection => wire.StartPoint == connection.Location.ToPoint3d()))
                    wire.StartPoint.TransformBy(transformation);
                else
                    wire.EndPoint.TransformBy(transformation);
            }
            if (BlockReference.Name.StartsWith("H"))
                BlockReference.TransformBy(transformation);
        }

        #endregion

        #region Public Static Methods

        public static string PromptSymbolName(Editor editor)
        {
            OpenFileDialog fileDialog = new OpenFileDialog("Open Symbol", "", "dwg", "", 0);
            fileDialog.ShowDialog();
            if (fileDialog.Filename != "")
                return Paths.GetRelativePath(Paths.SchematicLibrary, fileDialog.Filename);

            PromptStringOptions options = new PromptStringOptions("Enter symbol name: ")
            {
                AllowSpaces = true
            };
            PromptResult result = editor.GetString(options);
            return result.StringResult;
        }

        public static void Link(Database database, ICollection<Symbol> symbols)
        {
            if (symbols is null)
                return;

            List<Symbol> ordered = symbols.OrderBy(symbol => symbol.LineNumber).ToList();
            ordered.OfType<ChildSymbol>().ForEach(child =>
            {
                child.TagHidden = true;
                child.SetParent(ordered.First() as ParentSymbol);
            });
            LinkConnection top = null, bottom = null;
            Graph<EntityNode, Entity> linkGraph = new Graph<EntityNode, Entity>();

            foreach (Symbol symbol in ordered)
            {
                top = symbol.LinkConnections.Where(connection => connection.WireDirection == Orientation.Up)
                                            .Aggregate((next, highest) => next.Location.Y > highest.Location.Y ? next : highest);
                if (top != null && bottom != null)
                {
                    EntityNode topNode = linkGraph.AddNode(new EntityNode(top.Owner));
                    EntityNode bottomNode = linkGraph.AddNode(new EntityNode(bottom.Owner));
                    if (top.Location.X == bottom.Location.X)
                    {
                        Line link = new Line(top.Location.ToPoint3d(), bottom.Location.ToPoint3d());
                        link.Insert(database, ElectricalLayers.LinkLayer);
                        EntityNode linkNode = linkGraph.AddNode(new EntityNode(link));
                        linkGraph.AddEdge(topNode, linkNode);
                        linkGraph.AddEdge(bottomNode, linkNode);
                    }
                    else
                    {
                        Polyline link = new Polyline();
                        link.AddVertexAt(0, top.Location, 0, 0, 0);
                        link.AddVertexAt(1, new Point2d(top.Location.X, (top.Location.Y + bottom.Location.Y) / 2), 0, 0, 0);
                        link.AddVertexAt(2, new Point2d(bottom.Location.X, (top.Location.Y + bottom.Location.Y) / 2), 0, 0, 0);
                        link.AddVertexAt(3, bottom.Location, 0, 0, 0);
                        link.Insert(database, ElectricalLayers.LinkLayer);
                        EntityNode linkNode = linkGraph.AddNode(new EntityNode(link));
                        linkGraph.AddEdge(topNode, linkNode);
                        linkGraph.AddEdge(bottomNode, linkNode);
                    }
                    top = null;
                    bottom = null;
                }
                bottom = symbol.LinkConnections.Where(connection => connection.WireDirection == Orientation.Down)
                                               .Aggregate((next, lowest) => next.Location.Y < lowest.Location.Y ? next : lowest);
            }
            EntityGroup group = new EntityGroup();
            linkGraph.Nodes.Select(node => node.Value).ForEach(entity => group.Append(entity.Id));
            database.AddGroup(ordered.First().Tag, group);
        }

        #endregion

        #endregion

        #region Enums

        public enum Type
        {
            Parent,
            Child
        }

        #endregion
    }
}

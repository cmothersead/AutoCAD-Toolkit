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

        #endregion

        #endregion

        #region Constructors

        public Symbol(BlockReference blockReference)
        {
            BlockReference = blockReference;
            Stack = new AttributeStack(BlockReference, AttributeLayers)
            {
                Position = FamilyAttribute.Justify == AttachmentPoint.BaseLeft ?
                           FamilyAttribute.Position.ToPoint2D() :
                           FamilyAttribute.AlignmentPoint.ToPoint2D(),
                Justification = FamilyAttribute.Justify,
            };
        }

        #endregion

        #region Methods

        #region Public Methods

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
            foreach(WireConnection point in WireConnections)
            {
                Line intersectLine;
                switch (point.WireDirection)
                {
                    case Orientation.Left | Orientation.Right:
                        intersectLine = new Line(point.Location.ToPoint3d(), new Point3d(point.Location.X, point.Location.Y + 1, 0));
                        break;
                    default:
                        intersectLine = new Line(point.Location.ToPoint3d(), new Point3d(point.Location.X + 1, point.Location.Y, 0));
                        break;
                }
                List<Entity> list = point.Owner.Database.GetEntities()
                                                        .Where(entity => entity.Layer == ElectricalLayers.WireLayer.Name)
                                                        .Where(entity => entity.IntersectsWith(intersectLine))
                                                        .ToList();
                foreach(Entity entity in list)
                {
                    Line clone = entity.Clone() as Line;
                }
            }
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

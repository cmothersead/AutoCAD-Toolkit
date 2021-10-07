using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows;
using ICA.AutoCAD.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static ICA.AutoCAD.Adapter.ConnectionPoint;

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

        protected BlockReference BlockReference { get; }

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

        protected Dictionary<string, string> Replacements => new Dictionary<string, string>()
        {
            { "%F", $"{Family}" },
            { "%S", $"" }, //Fix so that sheet is not included in line number
            { "%N", $"{LineNumber}" },
            { "%X", "1" } //suffix character for reference based tagging
        };

        protected AttributeStack Stack { get; }

        #endregion

        #region Public Properties

        public string Family
        {
            get => FamilyAttribute?.TextString;
            set => FamilyAttribute?.SetValue(value);
        }

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
                foreach (string val in value)
                {
                    DescAttributes[position++].SetValue(val);
                }
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

        public List<WireConnection> WireConnections => BlockReference.GetAttributeReferences()
                                                                     .Where(reference => Regex.IsMatch(reference.Tag, @"X[1,2,4,8]TERM\d{2}"))
                                                                     .Select(reference => new WireConnection(reference))
                                                                     .ToList();

        public List<LinkConnection> LinkConnections => BlockReference.GetAttributeReferences()
                                                                     .Where(reference => Regex.IsMatch(reference.Tag, @"X[1,2,4,8]LINK"))
                                                                     .Select(reference => new LinkConnection(reference))
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
                foreach (AttributeReference reference in BlockReference.GetAttributeReferences(transaction))
                {
                    KeyValuePair<string, LayerTableRecord> match = AttributeLayers.FirstOrDefault(pair => reference.Tag.Contains(pair.Key));
                    if (match.Key != null)
                        reference.SetLayer(transaction, match.Value);
                }
                transaction.Commit();
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

        public static void Link(List<Symbol> symbols)
        {
            List<Symbol> ordered = symbols.OrderBy(symbol => symbol.LineNumber).ToList();
            ordered.OfType<ChildSymbol>().ToList().ForEach(child => 
            { 
                child.TagHidden = true;
                child.SetParent(ordered.First() as ParentSymbol);
            });
            LinkConnection top = null, bottom = null;

            foreach (Symbol symbol in ordered)
            {
                top = symbol.LinkConnections.Where(connection => connection.WireDirection == Orientation.Up)
                                            .Aggregate((next, highest) => next.Location.Y > highest.Location.Y ? next : highest);
                if (top != null && bottom != null)
                {
                    if (top.Location.X == bottom.Location.X)
                    {
                        Line link = new Line(top.Location.ToPoint3d(), bottom.Location.ToPoint3d());
                        link.Insert();
                        link.SetLayer(ElectricalLayers.LinkLayer);
                    }
                    else
                    {
                        Polyline link = new Polyline();
                        link.AddVertexAt(0, top.Location, 0, 0, 0);
                        link.AddVertexAt(1, new Point2d(top.Location.X, (top.Location.Y + bottom.Location.Y) / 2), 0, 0, 0);
                        link.AddVertexAt(2, new Point2d(bottom.Location.X, (top.Location.Y + bottom.Location.Y) / 2), 0, 0, 0);
                        link.AddVertexAt(3, bottom.Location, 0, 0, 0);
                        link.Insert();
                        link.SetLayer(ElectricalLayers.LinkLayer);
                    }
                    top = null;
                    bottom = null;
                }
                bottom = symbol.LinkConnections.Where(connection => connection.WireDirection == Orientation.Down)
                                               .Aggregate((next, lowest) => next.Location.Y < lowest.Location.Y ? next : lowest);
            }
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

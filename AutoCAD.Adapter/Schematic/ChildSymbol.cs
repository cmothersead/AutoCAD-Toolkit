using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ICA.AutoCAD.Adapter.Windows.ViewModels;
using ICA.AutoCAD.Adapter.Windows.Views;
using ICA.Schematic;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ICA.AutoCAD.Adapter
{
    public class ChildSymbol : Symbol, IChildSymbol
    {
        #region Properties

        #region Private Properties

        private AttributeReference TagAttribute => BlockReference.GetAttributeReference("TAG2");

        private List<AttributeReference> DescAttributes
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

        private AttributeReference MfgAttribute => BlockReference.GetAttributeReference("MFG");

        private AttributeReference CatAttribute => BlockReference.GetAttributeReference("CAT");

        private AttributeReference InstAttribute => BlockReference.GetAttributeReference("INST");

        private AttributeReference LocAttribute => BlockReference.GetAttributeReference("LOC");

        private AttributeReference XrefAttribute => BlockReference.GetAttributeReference("XREF");

        private List<AttributeReference> InstallationAttributes => new List<AttributeReference>() { InstAttribute, LocAttribute };

        private List<AttributeReference> PartAttributes => new List<AttributeReference>() { MfgAttribute, CatAttribute };

        private static new Dictionary<string, LayerTableRecord> AttributeLayers => new Dictionary<string, LayerTableRecord>()
        {
            { "TAG", ElectricalLayers.TagLayer },
            { "MFG", ElectricalLayers.ManufacturerLayer },
            { "CAT", ElectricalLayers.PartNumberLayer },
            { "TERMDESC", ElectricalLayers.MiscellaneousLayer },
            { "DESC", ElectricalLayers.ChildDescriptionLayer },
            { "TERM", ElectricalLayers.TerminalLayer },
            { "CON", ElectricalLayers.ConductorLayer },
            { "RATING", ElectricalLayers.RatingLayer },
            { "WIRENO", ElectricalLayers.WireNumberLayer },
            { "XREF", ElectricalLayers.ChildXrefLayer }
        };

        private List<string> AttributeNames => new List<string>
        {
            "TAG",
            "MFG",
            "CAT",
            "DESC",
            "LOC",
            "INST"
        };

        private List<string> RequiredAttributes => new List<string>
        {
            "TAG2",
            "DESC1",
            "LOC",
            "INST"
        };

        #endregion

        #region Public Properties

        public Component Component { get; set; }

        public Database Database => BlockReference.Database;

        public string Tag
        {
            get => TagAttribute?.TextString;
            set => TagAttribute?.SetValue(value);
        }

        public string ManufacturerName
        {
            get => MfgAttribute?.TextString;
            set => MfgAttribute?.SetValue(value);
        }

        public string PartNumber
        {
            get => CatAttribute?.TextString;
            set => CatAttribute?.SetValue(value);
        }

        public string Enclosure
        {
            get => InstAttribute?.TextString;
            set => InstAttribute?.SetValue(value);
        }

        public string Location
        {
            get => LocAttribute?.TextString;
            set => LocAttribute?.SetValue(value);
        }

        public string Xref
        {
            get => XrefAttribute?.TextString;
            set => XrefAttribute?.SetValue(value);
        }

        public List<string> Description
        {
            get => DescAttributes.Select(a => a.TextString).ToList();
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

        public bool TagHidden
        {
            get => TagAttribute != null && TagAttribute.Invisible;
            set => TagAttribute?.SetVisibility(!value);
        }

        public bool DescriptionHidden
        {
            get => DescAttributes.Count != 0 && DescAttributes[0].Invisible;
            set => DescAttributes.ForEach(a => a?.SetVisibility(!value));
        }

        public bool InstallationHidden
        {
            get => InstAttribute != null && InstAttribute.Invisible;
            set => InstallationAttributes.ForEach(a => a?.SetVisibility(!value));
        }

        public bool PartInfoHidden
        {
            get => true;
            set => PartAttributes.ForEach(a => a?.SetVisibility(!value));
        }

        public Point2d Position => BlockReference.Position.ToPoint2D();

        public List<WireConnection> WireConnections => BlockReference.GetAttributeReferences()
                                                                     .Where(reference => Regex.IsMatch(reference.Tag, @"X[1,2,4,8]TERM\d{2}"))
                                                                     .Select(reference => new WireConnection(reference))
                                                                     .ToList();

        #endregion

        #endregion

        #region Construtctor

        public ChildSymbol(BlockReference blockReference) : base(blockReference)
        {
            Stack.Add(BlockReference.GetAttributeReferences()
                                    .Select(att => att.Tag)
                                    .Where(tag => AttributeNames.Any(att => tag.Contains(att)))
                                    .Union(RequiredAttributes)
                                    .ToList());
        }

        #endregion

        #region Methods

        #region Public

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

        public IParentSymbol SelectParent()
        {
            var components = Database.GetProject().Components
                                 .Where(component => component.Family == Family)
                                 .ToList();
            var view = new ComponentsListView(components);
            if (Application.ShowModalWindow(view) == true)
                return ((ComponentsListViewModel)view.DataContext).SelectedComponent.Symbol;

            return null;
        }

        #endregion

        #endregion
    }
}

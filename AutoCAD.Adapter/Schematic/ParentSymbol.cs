using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ICA.Schematic;

namespace ICA.AutoCAD.Adapter
{
    public class ParentSymbol : IParentSymbol
    {
        #region Private Properties

        private BlockReference BlockReference { get; }

        private AttributeReference TagAttribute => BlockReference.GetAttributeReference("TAG1");

        private AttributeReference FamilyAttribute => BlockReference.GetAttributeReference("FAMILY");

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

        private List<AttributeReference> InstallationAttributes => new List<AttributeReference>() { InstAttribute, LocAttribute };

        private List<AttributeReference> PartAttributes => new List<AttributeReference>() { MfgAttribute, CatAttribute };

        private AttributeReference NewDescAttribute => new AttributeReference()
        {
            Tag = $"DESC{DescAttributes.Count + 1}",
            Position = TagAttribute.Justify == AttachmentPoint.BaseLeft ? TagAttribute.Position : TagAttribute.AlignmentPoint,
            TextString = "",
            Justify = TagAttribute.Justify,
            LockPositionInBlock = true,
            Layer = BlockReference.Database.GetLayer(ElectricalLayers.DescriptionLayer).Name,
            Invisible = DescriptionHidden
        };

        #endregion

        #region Public Properties

        public string Tag
        {
            get => TagAttribute?.TextString;
            set => TagAttribute?.SetValue(value);
        }

        public string Family
        {
            get => FamilyAttribute?.TextString;
            set => FamilyAttribute?.SetValue(value);
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
                        RemoveDescription();
                    else
                        AddDescription();
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
            get => DescAttributes[0].Invisible;
            set => DescAttributes.ForEach(a => a.SetVisibility(!value));
        }

        public bool InstallationHidden
        {
            get => InstAttribute.Invisible;
            set => InstallationAttributes.ForEach(a => a.SetVisibility(!value));
        }

        public bool PartInfoHidden
        {
            get => MfgAttribute.Invisible;
            set => PartAttributes.ForEach(a => a.SetVisibility(!value));
        }

        public string LineNumber => BlockReference.Database.GetLadder()?.ClosestLineNumber(BlockReference.Position);

        public List<WireConnection> WireConnections => BlockReference.GetAttributeReferences()
                                                                     .Where(reference => Regex.IsMatch(reference.Tag, @"X[1,2,4,8]TERM\d{2}"))
                                                                     .Select(reference => new WireConnection(reference))
                                                                     .ToList();

        #endregion

        #region Construtctor

        public ParentSymbol(BlockReference blockReference) => BlockReference = blockReference;

        #endregion

        #region Public Methods

        public void CollapseAttributeStack() => CollapseAttributeStack(TagAttribute.Justify == AttachmentPoint.BaseLeft ? TagAttribute.Position : TagAttribute.AlignmentPoint);

        public void CollapseAttributeStack(Point3d position)
        {
            List<AttributeReference> list = DescAttributes;
            list.Add(MfgAttribute);
            list.Add(CatAttribute);
            list.Add(TagAttribute);
            list.Reverse();
            foreach (AttributeReference attributeReference in list)
            {
                attributeReference.SetPosition(position);
                if (!(attributeReference.Invisible | attributeReference.TextString == ""))
                {
                    position = new Point3d(position.X, position.Y + 0.15625, position.Z);
                }
            }
        }

        public void AddDescription() => BlockReference.AddAttributeReference(NewDescAttribute);

        public void RemoveDescription() => BlockReference.RemoveAttributeReference(DescAttributes.Last().Tag);

        public void AssignLayers() => ElectricalLayers.Assign(BlockReference);

        private Dictionary<string, string> Replacements => new Dictionary<string, string>()
        {
            { "%F", $"{Family}" },
            { "%S", $"SheetNumber" },
            { "%N", $"{LineNumber}" },
            { "%X", "1" } //suffix character for reference based tagging
        };

        public void UpdateTag(string format) => Tag = Replacements.Keys.Aggregate(format, (current, toReplace) => current.Replace(toReplace, Replacements[toReplace]));

        public void MatchWireNumbers()
        {
            foreach (AttributeReference reference in BlockReference.GetAttributeReferences().Where(r => r.Tag.Contains("WIRENO")))
            {
                if (BlockReference.GetAttributeReference($"X1TERM{reference.Tag.Substring(6, 2)}") is AttributeReference term1)
                    reference.SetValue(term1.TextString);
                else if (BlockReference.GetAttributeReference($"X2TERM{reference.Tag.Substring(6, 2)}") is AttributeReference term2)
                    reference.SetValue(term2.TextString);
                else if (BlockReference.GetAttributeReference($"X4TERM{reference.Tag.Substring(6, 2)}") is AttributeReference term4)
                    reference.SetValue(term4.TextString);
                else if (BlockReference.GetAttributeReference($"X8TERM{reference.Tag.Substring(6, 2)}") is AttributeReference term8)
                    reference.SetValue(term8.TextString);
            }
        }

        #endregion
    }
}

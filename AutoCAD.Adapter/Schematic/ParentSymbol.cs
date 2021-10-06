using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.DatabaseServices;
using ICA.Schematic;

namespace ICA.AutoCAD.Adapter
{
    public class ParentSymbol : Symbol, IParentSymbol
    {
        #region Fields

        #region Private Fields

        private Component component;

        #endregion

        #endregion

        #region Properties

        #region Private Properties

        private AttributeReference TagAttribute => BlockReference.GetAttributeReference("TAG1");

        private AttributeReference MfgAttribute => BlockReference.GetAttributeReference("MFG");

        private AttributeReference CatAttribute => BlockReference.GetAttributeReference("CAT");

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

        private AttributeReference InstAttribute => BlockReference.GetAttributeReference("INST");

        private AttributeReference LocAttribute => BlockReference.GetAttributeReference("LOC");

        private List<string> AttributeNames => new List<string>
        {
            "TAG",
            "MFG",
            "CAT",
            "DESC",
            "LOC",
            "INST"
        };

        private List<string> Exclude => new List<string>
        {
            "TERMDESC"
        };

        private List<string> RequiredAttributes => new List<string>
        {
            "TAG1",
            "MFG",
            "CAT",
            "DESC1",
            "LOC",
            "INST"
        };

        private List<AttributeReference> InstallationAttributes => new List<AttributeReference>() { InstAttribute, LocAttribute };

        private List<AttributeReference> PartAttributes => new List<AttributeReference>() { MfgAttribute, CatAttribute };

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
            get => MfgAttribute != null && MfgAttribute.Invisible;
            set => PartAttributes.ForEach(a => a?.SetVisibility(!value));
        }

        public List<WireConnection> WireConnections => BlockReference.GetAttributeReferences()
                                                                     .Where(reference => Regex.IsMatch(reference.Tag, @"X[1,2,4,8]TERM\d{2}"))
                                                                     .Select(reference => new WireConnection(reference))
                                                                     .ToList();

        #endregion

        #endregion

        #region Constructor

        public ParentSymbol(BlockReference blockReference) : base(blockReference) => Stack.Add(BlockReference.GetAttributeReferences()
                                                                                                             .Select(att => att.Tag)
                                                                                                             .Where(tag => AttributeNames.Any(att => tag.Contains(att)) && !Exclude.Any(att => tag.Contains(att)))
                                                                                                             .Union(RequiredAttributes)
                                                                                                             .ToList());

        #endregion

        #region Methods

        #region Public Methods

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

        #endregion
    }
}

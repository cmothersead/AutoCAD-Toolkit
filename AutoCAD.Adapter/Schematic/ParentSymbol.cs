using System.Collections.Generic;
using System.Linq;
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

        private AttributeReference MfgAttribute => BlockReference.GetAttributeReference("MFG");

        private AttributeReference CatAttribute => BlockReference.GetAttributeReference("CAT");

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

        #region Protected Properties

        protected override AttributeReference TagAttribute => BlockReference.GetAttributeReference("TAG1");

        #endregion

        #region Public Properties

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

        public void UpdateTag(string format = null)
        {
            if (format is null)
                format = Database.GetProject().Settings.Component.Format;
            Tag = Replacements.Keys.Aggregate(format, (current, toReplace) => current.Replace(toReplace, Replacements[toReplace]));
        } 
            

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

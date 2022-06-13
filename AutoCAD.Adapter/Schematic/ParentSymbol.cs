using Autodesk.AutoCAD.DatabaseServices;
using ICA.AutoCAD.Adapter.Extensions;
using ICA.Schematic;
using System.Collections.Generic;
using System.Linq;

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

        private AttributeReference RatingAttribute => BlockReference.GetAttributeReference("RATING");

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

        private Dictionary<string, string> Replacements => new Dictionary<string, string>()
        {
            { "%F", $"{FamilyView}" },
            { "%S", $"" }, //Fix so that sheet is not included in line number
            { "%N", $"{LineNumber}" },
            { "%X", $"{Index}" }
        };

        private List<AttributeReference> InstallationAttributes => new List<AttributeReference>() { InstAttribute, LocAttribute };

        private List<AttributeReference> PartAttributes => new List<AttributeReference>() { MfgAttribute, CatAttribute };

        private string DictionaryName => "Parents";

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
        public string Rating
        {
            get => RatingAttribute?.TextString;
            set => RatingAttribute?.SetValue(value);
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

        public bool RatingHidden
        {
            get => RatingAttribute != null && RatingAttribute.Invisible;
            set => RatingAttribute?.SetVisibility(!value);
        }

        public int Index
        {
            get
            {
                List<ParentSymbol> indexList = Database.GetParentSymbols().Where(parent => parent.Family == Family && parent.LineNumber == LineNumber)
                                                              .OrderBy(parent => parent.Position.X)
                                                              .ToList();
                ParentSymbol found = indexList.Find(parent => parent.Handle == Handle);
                if (found is null)
                    return indexList.Count + 1;
                else
                    return indexList.IndexOf(found) + 1;
            }
        }

        #endregion

        #endregion

        #region Constructor

        public ParentSymbol(BlockReference blockReference) : base(blockReference)
        {
            if (Database != null)
            {
                Stack.Add(BlockReference.GetAttributeReferences()
                                    .Select(att => att.Tag)
                                    .Where(tag => AttributeNames.Any(att => tag.Contains(att)) && !Exclude.Any(att => tag.Contains(att)))
                                    .Union(RequiredAttributes)
                                    .ToList());
                AddToDictionary(DictionaryName);
            }
        }

        #endregion

        #region Methods

        #region Public Methods

        public override bool Insert(Transaction transaction, Database database)
        {
            bool result = base.Insert(transaction, database);
            Stack.Add(BlockReference.GetAttributeReferences()
                                    .Select(att => att.Tag)
                                    .Where(tag => AttributeNames.Any(att => tag.Contains(att)) && !Exclude.Any(att => tag.Contains(att)))
                                    .Union(RequiredAttributes));
            AddToDictionary(DictionaryName);
            return result;
        }

        public void UpdateTag(string format = null)
        {
            if (format is null)
                format = Database.GetProject()?.Settings.Component.Format ?? "%F%S%N%X"; // TODO: Replace hard-coded default with application setting default.
            Tag = format.Replace(Replacements);
        }

        #endregion

        #endregion
    }
}

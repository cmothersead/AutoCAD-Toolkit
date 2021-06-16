using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public static class WDM
    {
        #region Private Properties

        /// <summary>
        /// Dictionary storing Electrical properties by their WD_M attribute tag
        /// </summary>
        private static readonly Dictionary<string, string> Names = new Dictionary<string, string>
        {
            //Sheet Properties
            { "SHEET", nameof(ElectricalDocumentProperties.Sheet.SheetNumber) },
            { "IEC_PROJ", nameof(ElectricalDocumentProperties.Sheet.IECProjectCode) },
            { "IEC_INST", nameof(ElectricalDocumentProperties.Sheet.IECInstallationCode) },
            { "IEC_LOC", nameof(ElectricalDocumentProperties.Sheet.IECLocationCode) },
            { "UNIT_SCL", nameof(ElectricalDocumentProperties.Sheet.UnitScale) },
            { "FEATURE_SCL", nameof(ElectricalDocumentProperties.Sheet.FeatureScale) },

            //Ladder Properties
            { "RUNGHORV", nameof(ElectricalDocumentProperties.Ladder.RungOrientation) },
            { "RUNGDIST", nameof(ElectricalDocumentProperties.Ladder.RungSpacing) },
            { "RUNGINC", nameof(ElectricalDocumentProperties.Ladder.RungIncrement) },
            { "DRWRUNG", nameof(ElectricalDocumentProperties.Ladder.DrawRungs) },
            { "PH3SPACE", nameof(ElectricalDocumentProperties.Ladder.ThreePhaseSpacing) },

            //Component Properties
            { "TAGMODE", nameof(ElectricalDocumentProperties.Component.TagMode) },
            { "TAG-START", nameof(ElectricalDocumentProperties.Component.TagStart) },
            { "TAG-RSUF", nameof(ElectricalDocumentProperties.Component.TagSuffixes) },
            { "TAGFMT", nameof(ElectricalDocumentProperties.Component.TagFormat) },

            //Wire Properties
            { "WIREMODE", nameof(ElectricalDocumentProperties.Wire.WireMode) },
            { "WIRE-START", nameof(ElectricalDocumentProperties.Wire.WireStart) },
            { "WIRE-RSUF", nameof(ElectricalDocumentProperties.Wire.WireSuffixes) },
            { "WIREFMT", nameof(ElectricalDocumentProperties.Wire.WireFormat) },
            { "WINC", nameof(ElectricalDocumentProperties.Wire.WireIncremement) },
            { "WLEADERS", nameof(ElectricalDocumentProperties.Wire.WireLeaders) },
            { "GAP_STYLE", nameof(ElectricalDocumentProperties.Wire.WireGapStyle) },
            { "SORTMODE", nameof(ElectricalDocumentProperties.Wire.WireSortMode) },
            { "WNUM_OFFSET", nameof(ElectricalDocumentProperties.Wire.WireOffsetDistance) },
            { "WNUM_FLAGS", nameof(ElectricalDocumentProperties.Wire.WireFlags) }
        };

        #endregion

        #region Public Methods

        public static Dictionary<string, string> Read(Database database)
        {
            return RenameKeys(WDMToDictionary(database));
        }

        #endregion

        #region Private Methods

        private static Dictionary<string, string> WDMToDictionary(Database database)
        {
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                ObjectIdCollection references = database.GetBlockTableRecord("WD_M").GetBlockReferenceIds(true, false);
                if (references.Count == 0)
                    return null;

                BlockReference wd_m = transaction.GetObject(references[0], OpenMode.ForRead) as BlockReference;
                Dictionary<string, string> properties = new Dictionary<string, string>();

                foreach (ObjectId id in wd_m.AttributeCollection)
                {
                    AttributeReference reference = transaction.GetObject(id, OpenMode.ForRead) as AttributeReference;
                    properties.Add(reference.Tag, reference.TextString);
                }

                return properties;
            }
        }

        private static Dictionary<string, string> RenameKeys(Dictionary<string, string> wdm)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (var entry in wdm)
                if (Names.ContainsKey(entry.Key))
                    dictionary.Add(Names[entry.Key], entry.Value);
            return dictionary;
        }

        #endregion
    }
}

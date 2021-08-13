using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public static class WDM
    {
        #region Private Properties

        /// <summary>
        /// Dictionary storing properties by their WD_M attribute tag
        /// </summary>
        private static readonly Dictionary<string, string> Names = new Dictionary<string, string>
        {
            //Sheet Properties
            { "SHEET", "Sheet" + nameof(ElectricalDocumentProperties.Sheet.Number) },

            //Ladder Properties
            { "RUNGHORV", "Ladder" + nameof(ElectricalDocumentProperties.Ladder.RungOrientation) },
            { "RUNGDIST", "Ladder" + nameof(ElectricalDocumentProperties.Ladder.RungSpacing) },
            { "RUNGINC", "Ladder" + nameof(ElectricalDocumentProperties.Ladder.RungIncrement) },
            { "DRWRUNG", "Ladder" + nameof(ElectricalDocumentProperties.Ladder.DrawRungs) },
            { "PH3SPACE", "Ladder" + nameof(ElectricalDocumentProperties.Ladder.ThreePhaseSpacing) },

            //Component Properties
            { "TAGMODE", "Tag" + nameof(ElectricalDocumentProperties.Component.Mode) },
            { "TAG-START", "Tag" + nameof(ElectricalDocumentProperties.Component.Start) },
            { "TAG-RSUF", "Tag" + nameof(ElectricalDocumentProperties.Component.Suffixes) },
            { "TAGFMT", "Tag" + nameof(ElectricalDocumentProperties.Component.Format) },

            //Wire Properties
            { "WIREMODE", "Wire" + nameof(ElectricalDocumentProperties.Wire.Mode) },
            { "WIRE-START", "Wire" + nameof(ElectricalDocumentProperties.Wire.Start) },
            { "WIRE-RSUF", "Wire" + nameof(ElectricalDocumentProperties.Wire.Suffixes) },
            { "WIREFMT", "Wire" + nameof(ElectricalDocumentProperties.Wire.Format) },
            { "WINC", "Wire" + nameof(ElectricalDocumentProperties.Wire.Incremement) },
            { "WLEADERS", "Wire" + nameof(ElectricalDocumentProperties.Wire.LeaderMode) },
            { "GAP_STYLE", "Wire" + nameof(ElectricalDocumentProperties.Wire.GapStyle) },
            { "SORTMODE", "Wire" + nameof(ElectricalDocumentProperties.Wire.SortMode) },
            { "WNUM_OFFSET", "Wire" + nameof(ElectricalDocumentProperties.Wire.Offset) },
            { "WNUM_FLAGS", "Wire" + nameof(ElectricalDocumentProperties.Wire.Flags) }
        };

        #endregion

        #region Public Methods

        public static Dictionary<string, string> Read(Database database)
        {
            database.SetCustomProperty("WD_MRead", "1");
            return RenameKeys(WDMToDictionary(database));
        }

        #endregion

        #region Private Methods

        private static Dictionary<string, string> WDMToDictionary(Database database)
        {
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = database.GetBlockTable(transaction);

                if (!blockTable.Has("WD_M"))
                    return null;

                ObjectIdCollection references = blockTable.GetRecord("WD_M")
                                                          .GetBlockReferenceIds(true, false);

                if (references is null | references.Count == 0)
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
            if (wdm is null)
                return new Dictionary<string, string>();

            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (var entry in wdm)
                if (Names.ContainsKey(entry.Key))
                    dictionary.Add(Names[entry.Key], entry.Value);
            return dictionary;
        }

        #endregion
    }
}

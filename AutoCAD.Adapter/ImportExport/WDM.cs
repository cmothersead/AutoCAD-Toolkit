using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

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
            //Ladder Settings
            { "RUNGHORV", LadderSettings.Prefix + nameof(LadderSettings.RungOrientation) },
            { "RUNGDIST", LadderSettings.Prefix + nameof(LadderSettings.RungSpacing) },
            { "RUNGINC", LadderSettings.Prefix + nameof(LadderSettings.RungIncrement) },
            { "DRWRUNG", LadderSettings.Prefix + nameof(LadderSettings.DrawRungs) },
            { "PH3SPACE", LadderSettings.Prefix + nameof(LadderSettings.ThreePhaseSpacing) },

            //Component Settings
            { "TAGMODE", ComponentSettings.Prefix + nameof(ComponentSettings.Mode) },
            { "TAG-START", ComponentSettings.Prefix + nameof(ComponentSettings.Start) },
            { "TAG-RSUF", ComponentSettings.Prefix + nameof(ComponentSettings.Suffixes) },
            { "TAGFMT", ComponentSettings.Prefix + nameof(ComponentSettings.Format) },

            //Wire Settings
            { "WIREMODE", WireSettings.Prefix + nameof(WireSettings.Mode) },
            { "WIRE-START", WireSettings.Prefix + nameof(WireSettings.Start) },
            { "WIRE-RSUF", WireSettings.Prefix + nameof(WireSettings.Suffixes) },
            { "WIREFMT", WireSettings.Prefix + nameof(WireSettings.Format) },
            { "WINC", WireSettings.Prefix + nameof(WireSettings.Incremement) },
            { "WLEADERS", WireSettings.Prefix + nameof(WireSettings.LeaderMode) },
            { "GAP_STYLE", WireSettings.Prefix + nameof(WireSettings.GapStyle) },
            { "SORTMODE", WireSettings.Prefix + nameof(WireSettings.SortMode) },
            { "WNUM_OFFSET", WireSettings.Prefix + nameof(WireSettings.Offset) },
            { "WNUM_FLAGS", WireSettings.Prefix + nameof(WireSettings.Flags) },

            //Cross Reference Settings
            { "XREFFMT", CrossReferenceSettings.Prefix + nameof(CrossReferenceSettings.Format) },
            { "ALT_XREFFMT", CrossReferenceSettings.Prefix + nameof(CrossReferenceSettings.ExternalFormat) },
            { "XREF_STYLE", CrossReferenceSettings.Prefix + nameof(CrossReferenceSettings.Style) },
            { "XREF_FILLWITH", CrossReferenceSettings.Prefix + nameof(CrossReferenceSettings.FillWith) },
            { "XREF_SORT", CrossReferenceSettings.Prefix + nameof(CrossReferenceSettings.SortMode) },
            { "XREF_TXTBTWN", CrossReferenceSettings.Prefix + nameof(CrossReferenceSettings.Delimiter) }
        };

        #endregion

        #region Public Methods

        public static Dictionary<string, string> Read(Database database)
        {
            database.SetCustomProperty("WD_MRead", "1");
            return RenameKeys(ToDictionary(database));
        }

        #endregion

        #region Private Methods

        private static Dictionary<string, string> ToDictionary(Database database)
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

                wd_m.AttributeCollection.Cast<ObjectId>()
                                        .Select(id => id.Open(transaction) as AttributeReference)
                                        .ForEach(reference => properties.Add(reference.Tag, reference.TextString));

                return properties;
            }
        }

        private static Dictionary<string, string> RenameKeys(Dictionary<string, string> wdm) => wdm?.Where(e => Names.ContainsKey(e.Key))
                                                                                                    .Select(p => (Key: Names[p.Key], p.Value))
                                                                                                    .ToDictionary(p => p.Key, p => p.Value);

        #endregion
    }
}

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Linq;

namespace ICA.AutoCAD.Adapter
{
    public static class DocumentExtensions
    {
        #region Private Methods

        private static int SafeParseInt(string s, int defaultValue)
        {
            try
            {
                return int.Parse(s);
            }
            catch
            {
                return defaultValue;
            }
        }

        private static double SafeParseDouble(string s, double defaultValue)
        {
            try
            {
                return double.Parse(s);
            }
            catch
            {
                return defaultValue;
            }
            
        }

        #endregion

        #region Public Extension Methods

        public static ElectricalDocumentProperties GetWDMProperties(this Document document)
        {
            using(Transaction transaction = document.Database.TransactionManager.StartTransaction())
            {
                ObjectIdCollection references = document.Database.GetBlockTableRecord("WD_M").GetBlockReferenceIds(true, false);
                if (references.Count == 0)
                    return null;

                BlockReference wd_m = transaction.GetObject(references[0], OpenMode.ForRead) as BlockReference;

                ElectricalDocumentProperties properties = new ElectricalDocumentProperties()
                {
                    SheetProperties = new SheetProperties()
                    {
                        SheetNumber = wd_m.GetAttributeValue("SHEET"),
                        IECProjectCode = wd_m.GetAttributeValue("IEC_PROJ"),
                        IECInstallationCode = wd_m.GetAttributeValue("IEC_INST"),
                        IECLocationCode = wd_m.GetAttributeValue("IEC_LOC"),
                        UnitScale = SafeParseDouble(wd_m.GetAttributeValue("UNIT_SCL"), 1),
                        FeatureScale = SafeParseDouble(wd_m.GetAttributeValue("FEATURE_SCL"), 0)
                    },
                    LadderProperties = new LadderProperties()
                    {
                        RungOrientation = RungOrientation.Horizontal,
                        RungSpacing = SafeParseDouble(wd_m.GetAttributeValue("RUNGDIST"), 0.5),
                        LadderWidth = SafeParseDouble(wd_m.GetAttributeValue("DLADW"), 32.5),
                        RungIncrement = SafeParseInt(wd_m.GetAttributeValue("RUNGINC"), 1),
                        DrawRungs = SafeParseInt(wd_m.GetAttributeValue("DRWRUNG"), 0),
                        ThreePhaseSpacing = SafeParseDouble(wd_m.GetAttributeValue("PH3SPACE"), 1)
                    },
                    ComponentProperties = new ComponentProperties()
                    {
                        TagMode = NumberMode.Referential, /* remove hard coding with converter later*/
                        TagStart = SafeParseInt(wd_m.GetAttributeValue("TAG-START"), 1),
                        TagSuffixes = wd_m.GetAttributeValue("TAG-RSUF").Split(',').ToList(),
                        TagFormat = wd_m.GetAttributeValue("TAGFMT")
                    },
                    WireProperties = new WireProperties()
                    {
                        WireMode = NumberMode.Referential, /* remove hard coding with converter later*/
                        WireStart = SafeParseInt(wd_m.GetAttributeValue("WIRE-START"), 100),
                        WireSuffixes = wd_m.GetAttributeValue("WIRE-RSUF").Split(',').ToList(),
                        WireFormat = wd_m.GetAttributeValue("WIREFMT"),
                        WireIncremement = SafeParseInt(wd_m.GetAttributeValue("WINC"), 1),
                        WireLeaders = LeaderInsertMode.Never, /* remove hard coding with converter later*/
                        WireGapStyle = WireGapStyle.Loop, /* remove hard coding with converter later*/
                        WireSortMode = null, /* remove hard coding with converter later*/
                        WireOffsetDistance = SafeParseDouble(wd_m.GetAttributeValue("WNUM_OFFSET"), 0),
                        WireFlags = 0 /* remove hard coding with converter later*/
                    }
                };

                return properties;
            }
        }

        public static string GetPageNumber(this Document document)
        {
            return GetWDMProperties(document).SheetProperties.SheetNumber;
        }

        #endregion
    }
}

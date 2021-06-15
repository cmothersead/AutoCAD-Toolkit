using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
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

        private static Dictionary<string, string> ReadWDM(this Document document)
        {
            using(Transaction transaction = document.Database.TransactionManager.StartTransaction())
            {
                ObjectIdCollection references = document.Database.GetBlockTableRecord("WD_M").GetBlockReferenceIds(true, false);
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

        public static ElectricalDocumentProperties ElectricalProperties(this Document document)
        {
            Dictionary<string, string> wd_mProperties = document.ReadWDM();

            return new ElectricalDocumentProperties()
            {
                SheetProperties = new SheetProperties()
                {
                    SheetNumber = wd_mProperties["SHEET"],
                    IECProjectCode = wd_mProperties["IEC_PROJ"],
                    IECInstallationCode = wd_mProperties["IEC_INST"],
                    IECLocationCode = wd_mProperties["IEC_LOC"],
                    UnitScale = SafeParseDouble(wd_mProperties["UNIT_SCL"], 1),
                    FeatureScale = SafeParseDouble(wd_mProperties["FEATURE_SCL"], 0)
                },
                LadderProperties = new LadderProperties()
                {
                    RungOrientation = RungOrientation.Horizontal, /* remove hard coding with converter later*/
                    RungSpacing = SafeParseDouble(wd_mProperties["RUNGDIST"], 0.5),
                    LadderWidth = SafeParseDouble(wd_mProperties["DLADW"], 32.5),
                    RungIncrement = SafeParseInt(wd_mProperties["RUNGINC"], 1),
                    DrawRungs = SafeParseInt(wd_mProperties["DRWRUNG"], 0),
                    ThreePhaseSpacing = SafeParseDouble(wd_mProperties["PH3SPACE"], 1)
                },
                ComponentProperties = new ComponentProperties()
                {
                    TagMode = NumberMode.Referential, /* remove hard coding with converter later*/
                    TagStart = SafeParseInt(wd_mProperties["TAG-START"], 1),
                    TagSuffixes = wd_mProperties["TAG-RSUF"].Split(',').ToList(),
                    TagFormat = wd_mProperties["TAGFMT"]
                },
                WireProperties = new WireProperties()
                {
                    WireMode = NumberMode.Referential, /* remove hard coding with converter later*/
                    WireStart = SafeParseInt(wd_mProperties["WIRE-START"], 100),
                    WireSuffixes = wd_mProperties["WIRE-RSUF"].Split(',').ToList(),
                    WireFormat = wd_mProperties["WIREFMT"],
                    WireIncremement = SafeParseInt(wd_mProperties["WINC"], 1),
                    WireLeaders = LeaderInsertMode.Never, /* remove hard coding with converter later*/
                    WireGapStyle = WireGapStyle.Loop, /* remove hard coding with converter later*/
                    WireSortMode = null, /* remove hard coding with converter later*/
                    WireOffsetDistance = SafeParseDouble(wd_mProperties["WNUM_OFFSET"], 0),
                    WireFlags = 0 /* remove hard coding with converter later*/
                }
            };
        }

        public static string GetPageNumber(this Document document)
        {
            return document.ElectricalProperties().SheetProperties.SheetNumber;
        }

        #endregion
    }
}

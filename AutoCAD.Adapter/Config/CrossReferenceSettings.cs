using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class CrossReferenceSettings : Settings
    {
        #region Public Properties

        public static string Prefix => "Xref";

        public string Format { get; set; } = "%S%N";
        public string ExternalFormat { get; set; } = "%S%N";
        public CrossReferenceStyle Style { get; set; } = CrossReferenceStyle.Text;
        public bool IncludeUnused { get; set; } = false;
        public bool IncludeParent { get; set; } = false;
        public bool ContactCount { get; set; } = false;
        public string FillWith { get; set; } = "";
        public CrossReferenceSortMode SortMode { get; set; } = CrossReferenceSortMode.ByLineReference;
        public string Delimiter { get; set; } = ",";

        #endregion

        #region Constructors

        public CrossReferenceSettings() { }

        public CrossReferenceSettings(Dictionary<string, string> dictionary) : base(Prefix, dictionary) { }

        #endregion

        #region Public Methods

        public Dictionary<string, string> ToDictionary() => ToDictionary(Prefix);

        #endregion
    }
}

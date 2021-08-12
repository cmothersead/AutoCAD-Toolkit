using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class CrossReferenceProperties : Properties
    {
        #region Private Properties

        private static string Prefix => "Xref";

        #endregion

        #region Public Properties

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

        public CrossReferenceProperties() { }

        public CrossReferenceProperties(Dictionary<string, string> dictionary) : base(Prefix, dictionary) { }

        #endregion

        #region Public Methods

        public Dictionary<string, string> ToDictionary() => ToDictionary(Prefix);

        #endregion
    }
}

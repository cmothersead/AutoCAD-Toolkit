using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class ComponentProperties : Properties
    {
        #region Private Properties

        private static string Prefix => "Tag";

        #endregion

        #region Public Properties

        public NumberMode Mode { get; set; } = NumberMode.Referential;
        public int Start { get; set; } = 1;
        public List<string> Suffixes { get; set; } = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        public string Format { get; set; } = "%F%S%N%X";

        #endregion

        #region Constructors

        public ComponentProperties() { }

        public ComponentProperties(Dictionary<string, string> dictionary) : base(Prefix, dictionary) { }

        #endregion

        #region Public Methods

        public Dictionary<string, string> ToDictionary() => ToDictionary(Prefix);

        #endregion
    }
}

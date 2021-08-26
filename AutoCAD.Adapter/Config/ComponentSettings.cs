using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class ComponentSettings : Settings
    {
        #region Public Properties

        public static string Prefix => "Tag";

        public NumberMode Mode { get; set; } = NumberMode.Referential;
        public int Start { get; set; } = 1;
        public List<string> Suffixes { get; set; } = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        public string Format { get; set; } = "%F%S%N%X";

        #endregion

        #region Constructors

        public ComponentSettings() { }

        public ComponentSettings(Dictionary<string, string> dictionary) : base(Prefix, dictionary) { }

        #endregion

        #region Public Methods

        public Dictionary<string, string> ToDictionary() => ToDictionary(Prefix);

        #endregion
    }
}

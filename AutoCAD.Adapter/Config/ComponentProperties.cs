using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class ComponentProperties : Properties
    {
        #region Public Properties

        public NumberMode TagMode { get; set; } = NumberMode.Referential;
        public int TagStart { get; set; } = 1;
        public List<string> TagSuffixes { get; set; } = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        public string TagFormat { get; set; } = "%F%S%N%X";

        #endregion

        #region Constructors

        public ComponentProperties() { }

        public ComponentProperties(Dictionary<string, string> dictionary) : base(dictionary) { }

        #endregion
    }
}

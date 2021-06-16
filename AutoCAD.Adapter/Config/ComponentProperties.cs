using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class ComponentProperties : Properties
    {
        public NumberMode TagMode { get; set; } = NumberMode.Referential;
        public int TagStart { get; set; } = 1;
        public List<string> TagSuffixes { get; set; }
        public string TagFormat { get; set; }

        public ComponentProperties() { }

        public ComponentProperties(Dictionary<string, string> dictionary) : base(dictionary) { }
    }
}

using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class ComponentProperties : Properties
    {
        public NumberMode TagMode { get; set; }
        public int TagStart { get; set; }
        public List<string> TagSuffixes { get; set; }
        public string TagFormat { get; set; }
    }
}

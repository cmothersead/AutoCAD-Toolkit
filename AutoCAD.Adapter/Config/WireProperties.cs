using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class WireProperties : Properties
    {
        public NumberMode WireMode { get; set; } = NumberMode.Referential;
        public int WireStart { get; set; } = 100;
        public List<string> WireSuffixes { get; set; } = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
        public string WireFormat { get; set; } = "%S%N";
        public int WireIncremement { get; set; } = 1;
        public LeaderInsertMode WireLeaders { get; set; } = LeaderInsertMode.Never;
        public WireGapStyle WireGapStyle { get; set; } = WireGapStyle.Loop;
        public WireSortMode? WireSortMode { get; set; } = null;
        public double WireOffsetDistance { get; set; } = 0;
        public int WireFlags { get; set; } = 1;

        public WireProperties () { }

        public WireProperties (Dictionary<string, string> dictionary) : base(dictionary) { }
    }
}

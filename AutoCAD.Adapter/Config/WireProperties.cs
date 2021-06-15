using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class WireProperties
    {
        public NumberMode WireMode { get; set; }
        public int WireStart { get; set; }
        public List<string> WireSuffixes { get; set; }
        public string WireFormat { get; set; }
        public int WireIncremement { get; set; }
        public LeaderInsertMode WireLeaders { get; set; }
        public WireGapStyle WireGapStyle { get; set; }
        public WireSortMode? WireSortMode { get; set; }
        public double WireOffsetDistance { get; set; }
        public int WireFlags { get; set; }
    }
}

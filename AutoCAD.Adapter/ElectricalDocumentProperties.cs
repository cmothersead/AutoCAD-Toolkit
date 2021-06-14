using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter
{
    public class ElectricalDocumentProperties
    {
        public string Sheet { get; set; }
        public string IECProj { get; set; }
        public string IECInst { get; set; }
        public string IECLoc { get; set; }
        public float UnitScale { get; set; }
        public float FeatureScale { get; set; }

        public bool HorizontalRungs { get; set; }
        public float RungSpacing { get; set; }
        public float LadderWidth { get; set; }
        public float RungIncrement { get; set; }
        public int DrawRungs { get; set; }
        public float ThreePhaseSpacing { get; set; }

        public bool TagMode { get; set; }
        public int TagStart { get; set; }
        public List<string> TagSuffixes { get; set; }
        public string TagFormat { get; set; }

        public bool WireMode { get; set; }
        public int WireStart { get; set; }
        public List<string> WireSuffixes { get; set; }
        public string WireFormat { get; set; }
        public int WireIncremement { get; set; }
        public int WireLeaders { get; set; }
        public int WireGapStyle { get; set; }
        public string WireSortMode { get; set; }
        public float WireOffsetDistance { get; set; }
        public int WireFlags { get; set; }
        

    }
}

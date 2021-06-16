using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class LadderProperties : Properties
    {
        public RungOrientation RungOrientation { get; set; }
        public double RungSpacing { get; set; }
        public double LadderWidth { get; set; }
        public int RungIncrement { get; set; }
        public int DrawRungs { get; set; }
        public double ThreePhaseSpacing { get; set; }

        public LadderProperties() { }

        public LadderProperties(Dictionary<string, string> dictionary) : base (dictionary) { }
    }
}

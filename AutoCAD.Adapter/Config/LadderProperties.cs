using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class LadderProperties : Properties
    {
        public RungOrientation RungOrientation { get; set; } = RungOrientation.Horizontal;
        public double RungSpacing { get; set; } = 0.5;
        public int RungIncrement { get; set; } = 1;
        public int DrawRungs { get; set; } = 0;
        public double ThreePhaseSpacing { get; set; } = 1;

        public LadderProperties() { }

        public LadderProperties(Dictionary<string, string> dictionary) : base (dictionary) { }
    }
}

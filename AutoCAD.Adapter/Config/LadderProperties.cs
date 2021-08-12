using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class LadderProperties : Properties
    {
        #region Private Properties

        private static string Prefix => "Ladder";

        #endregion

        #region Public Properties

        public RungOrientation RungOrientation { get; set; } = RungOrientation.Horizontal;
        public double RungSpacing { get; set; } = 0.5;
        public int RungIncrement { get; set; } = 1;
        public bool DrawRungs { get; set; } = false;
        public double ThreePhaseSpacing { get; set; } = 1;

        #endregion

        #region Constructors

        public LadderProperties() { }

        public LadderProperties(Dictionary<string, string> dictionary) : base (Prefix, dictionary) { }

        #endregion

        #region Public Methods

        public Dictionary<string, string> ToDictionary() => ToDictionary(Prefix);

        #endregion
    }
}

using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class WireSettings : Settings
    {
        #region Public Properties

        public static string Prefix => "Wire";

        public NumberMode Mode { get; set; } = NumberMode.Referential;
        public int Start { get; set; } = 100;
        public List<string> Suffixes { get; set; } = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
        public string Format { get; set; } = "%S%N";
        public int Incremement { get; set; } = 1;
        public LeaderInsertMode LeaderMode { get; set; } = LeaderInsertMode.Never;
        public WireGapStyle GapStyle { get; set; } = WireGapStyle.Loop;
        public WireSortMode? SortMode { get; set; } = null;
        public double Offset { get; set; } = 0;
        public WireNumberPosition NumberPosition { get; set; } = WireNumberPosition.Above;
        public int Flags
        {
            get
            {
                switch (NumberPosition)
                {
                    case WireNumberPosition.InLine:
                        return 5;
                    case WireNumberPosition.Below:
                        return 3;
                    default:
                        return 1;
                }
            }
            set
            {
                switch (value)
                {
                    case 5:
                        NumberPosition = WireNumberPosition.InLine;
                        break;
                    case 3:
                        NumberPosition = WireNumberPosition.Below;
                        break;
                    default:
                        NumberPosition = WireNumberPosition.Above;
                        break;
                }
            }
        }
        public double GapDistance { get; set; }
        public double GapIncrement { get; set; }
        public double GapMinimum { get; set; }
        public double[] GapValues => new double[3] { GapMinimum, GapDistance, GapIncrement };

        #endregion

        #region Constructors

        public WireSettings() { }

        public WireSettings(Dictionary<string, string> dictionary) : base(Prefix, dictionary) { }

        #endregion

        #region Public Methods

        public Dictionary<string, string> ToDictionary() => ToDictionary(Prefix);

        #endregion
    }
}

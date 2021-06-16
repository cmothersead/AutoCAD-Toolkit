using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class SheetProperties : Properties
    {
        public string SheetNumber { get; set; }
        public string IECProjectCode { get; set; }
        public string IECInstallationCode { get; set; }
        public string IECLocationCode { get; set; }
        public double UnitScale { get; set; } = 1;
        public double FeatureScale { get; set; } = 0;

        public SheetProperties () { }

        public SheetProperties (Dictionary<string, string> dictionary) : base(dictionary) { }
    }
}

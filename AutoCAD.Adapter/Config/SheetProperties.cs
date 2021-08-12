using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class SheetProperties : Properties
    {
        #region Private Properties

        private static string Prefix => "Sheet";

        #endregion

        #region Public Properties

        public string Number { get; set; }

        #endregion

        #region Constructors

        public SheetProperties () { }

        public SheetProperties (Dictionary<string, string> dictionary) : base(Prefix, dictionary) { }

        #endregion

        #region Public Methods

        public Dictionary<string, string> ToDictionary() => ToDictionary(Prefix);

        #endregion
    }
}

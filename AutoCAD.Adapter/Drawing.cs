using System;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class Drawing
    {
        #region Public Properties

        public Uri FileUri { get; set; }
        public List<string> Description { get; set; } = new List<string>();
        public ElectricalDocumentProperties Properties { get; set; }

        #endregion
    }
}

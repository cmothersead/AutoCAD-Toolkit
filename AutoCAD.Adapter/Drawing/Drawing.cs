using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class Drawing
    {
        #region Private Properties

        private Document Document { get; }

        #endregion

        #region Public Properties

        public Uri FileUri { get; set; }
        public Project Project { get; set; }
        public List<string> Description { get; set; } = new List<string>();
        public DrawingSettings Settings { get; set; }
        public string SheetNumber { get; set; }

        #endregion
    }
}

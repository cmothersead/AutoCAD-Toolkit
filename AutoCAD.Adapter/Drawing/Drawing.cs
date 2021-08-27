using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ICA.AutoCAD.Adapter
{
    public class Drawing
    {
        #region Private Properties

        private Document Document { get; }

        #endregion

        #region Public Properties

        [XmlIgnore]
        public Uri Uri { get; set; }
        [XmlIgnore]
        public Project Project { get; set; }

        public string Path => Project.Uri.MakeRelativeUri(Uri).ToString();
        public List<string> Description { get; set; } = new List<string>();
        public DrawingSettings Settings { get; set; }
        public string SheetNumber { get; set; }

        #endregion
    }
}

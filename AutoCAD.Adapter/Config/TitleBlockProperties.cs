﻿using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class TitleBlockProperties : Properties
    {
        #region Private Properties

        private static string Prefix => "";

        #endregion

        #region Public Properties

        public Point2d LadderOrigin { get; set; }
        public double LadderTotalHeight { get; set; }
        public double LadderTotalWidth { get; set; }
        public double LadderGap { get; set; }

        #endregion

        #region Constructors

        public TitleBlockProperties() { }

        public TitleBlockProperties(Dictionary<string, string> dictionary) : base(Prefix, dictionary) { }

        #endregion

        #region Public Methods

        public Dictionary<string, string> ToDictionary() => ToDictionary(Prefix);

        #endregion
    }
}

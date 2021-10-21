using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace ICA.AutoCAD.Adapter
{
    public class ConnectionPoint
    {
        #region Properties

        #region Public Properties

        public Point2d Location { get; set; }
        public Orientation WireDirection { get; set; }
        public BlockReference Owner { get; }

        #endregion

        #endregion

        #region Constructors

        public ConnectionPoint() { }

        public ConnectionPoint(BlockReference blockReference)
        {
            Owner = blockReference;
        }

        #endregion

        #region Enums

        [Flags]
        public enum Orientation
        {
            Right = 1,
            Up = 2,
            Left = 4,
            Down = 8,
            All = 15
        }

        #endregion
    }
}
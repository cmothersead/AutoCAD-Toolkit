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

        #region Methods

        #region Private Methods

        protected double OrientationToAngle(Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.Right:
                    return 0;
                case Orientation.Up:
                    return Math.PI / 2;
                case Orientation.Left:
                    return Math.PI;
                case Orientation.Down:
                    return Math.PI * 3 / 2;
                default:
                    throw new ArgumentException("Orientation must only have one flag value raised to be converted.");
            }
        }

        protected Orientation AngleToOrientation(double angle)
        {
            switch (angle % (Math.PI * 2))
            {
                case 0:
                    return Orientation.Right;
                case Math.PI / 2:
                    return Orientation.Up;
                case Math.PI:
                    return Orientation.Left;
                case Math.PI * 3 / 2:
                    return Orientation.Down;
                default:
                    throw new ArgumentException("Invalid angle, must be multiple of PI/2.");
            }
        }

        #endregion

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
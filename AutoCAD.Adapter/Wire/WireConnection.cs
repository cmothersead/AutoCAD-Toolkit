using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Text.RegularExpressions;

namespace ICA.AutoCAD.Adapter
{
    public class WireConnection : ConnectionPoint
    {
        #region Constructors

        public WireConnection(BlockReference blockReference, AttributeDefinition definition) : base(blockReference)
        {
            Regex format = new Regex(@"X([1,2,4,8])TERM\d{2}");
            Match match = format.Match(definition.Tag);
            if (!match.Success)
                throw new ArgumentException("Attribute definition is not formatted as a valid wire connection.");
            WireDirection = (Orientation)int.Parse(match.Groups[1].ToString());
            Location = blockReference.Position.TransformBy(Matrix3d.Displacement(definition.Position.GetAsVector())).ToPoint2D();
        }

        public WireConnection(Point2d location, double entryAngle)
        {
            Location = location;
            switch (entryAngle)
            {
                case 0:
                    WireDirection = Orientation.All ^ Orientation.Left;
                    break;
                case Math.PI / 2:
                    WireDirection = Orientation.All ^ Orientation.Down;
                    break;
                case Math.PI:
                    WireDirection = Orientation.All ^ Orientation.Right;
                    break;
                case Math.PI * 3 / 2:
                    WireDirection = Orientation.All ^ Orientation.Up;
                    break;
            }
        }

        #endregion

        #region Methods

        public bool IsAligned(double angle)
        {
            switch (angle)
            {
                case 0:
                case Math.PI:
                    return WireDirection == (Orientation.Left | Orientation.Right);
                case Math.PI / 2:
                case Math.PI * 3 / 2:
                    return WireDirection == (Orientation.Up | Orientation.Down);
                default:
                    return false;
            }
        }

        #endregion
    }
}

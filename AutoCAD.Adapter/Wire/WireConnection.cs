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
            WireDirection = Orientation.All ^ AngleToOrientation(entryAngle);
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// True if a line along the given angle aligns wih one of the orienations of this connection.
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public bool IsAligned(double angle)
        {
            switch (angle)
            {
                case 0:
                case Math.PI:
                    return WireDirection.HasFlag(Orientation.Left) || WireDirection.HasFlag(Orientation.Right);
                case Math.PI / 2:
                case Math.PI * 3 / 2:
                    return WireDirection.HasFlag(Orientation.Up) || WireDirection.HasFlag(Orientation.Down);
                default:
                    return false;
            }
        }

        /// <summary>
        /// True is the given line terminates at the connection point and is the correct direction
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool IsConnected(Line line)
        {
            if (!IsAligned(line.Angle))
                return false;

            if (Location != line.StartPoint.ToPoint2D() && Location != line.EndPoint.ToPoint2D())
                return false;

            if (Location == line.StartPoint.ToPoint2D())
                return line.Angle == OrientationToAngle(WireDirection);
            else
                return (line.Angle + Math.PI) % (Math.PI * 2) == OrientationToAngle(WireDirection);
        }

        /// <summary>
        /// True if orientation is 180 degrees opposite
        /// </summary>
        /// <param name="orienation"></param>
        /// <returns></returns>
        public bool IsOpposite(Orientation orienation)
        {
            switch (WireDirection)
            {
                case Orientation.Right:
                    if (orienation != Orientation.Left)
                        return false;
                    return true;
                case Orientation.Up:
                    if (orienation != Orientation.Down)
                        return false;
                    return true;
                case Orientation.Left:
                    if (orienation != Orientation.Right)
                        return false;
                    return true;
                case Orientation.Down:
                    if (orienation != Orientation.Up)
                        return false;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// True if the two connection points form an inline pair
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public bool IsPairWith(WireConnection connection)
        {
            if (!IsOpposite(connection.WireDirection))
                return false;

            Line lineBetween = new Line(Location.ToPoint3d(), connection.Location.ToPoint3d());
            if (IsConnected(lineBetween) || connection.IsConnected(lineBetween))
                return false;

            return true;
        }

        #endregion

        #endregion
    }
}

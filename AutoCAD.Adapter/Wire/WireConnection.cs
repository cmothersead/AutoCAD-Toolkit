using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Text.RegularExpressions;

namespace ICA.AutoCAD.Adapter
{
    public class WireConnection
    {
        public Point2d Location { get; set; }
        public Orientation WireDirection { get; set; }
        
        [Flags]
        public enum Orientation
        {
            Right = 1,
            Up = 2,
            Left = 4,
            Down = 8,
            All = 15
        }

        public WireConnection(AttributeReference reference)
        {
            Regex format = new Regex(@"X([1,2,4,8])TERM\d{2}");
            Match match = format.Match(reference.Tag);
            if (!match.Success)
                throw new ArgumentException("Attribute reference is not formatted as a valid wire connection.");
            WireDirection = (Orientation)int.Parse(match.Groups[1].ToString());
            Location = reference.GetPosition();
        }

        public WireConnection(Point2d location, double entryAngle)
        {
            Location = location;
            switch(entryAngle)
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
    }
}

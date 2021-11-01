using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD
{
    public static class LineExtensions
    {
        public static bool IsOn(this Line line, Point2d point)
        {
            List<double> x = new List<double> { line.StartPoint.X, line.EndPoint.X };
            if (point.X < x.Min() || point.X > x.Max())
                return false;

            List<double> y = new List<double> { line.StartPoint.Y, line.EndPoint.Y };
            if (point.Y < y.Min() || point.Y > y.Max())
                return false;

            return new Line2d(line.StartPoint.ToPoint2D(), line.EndPoint.ToPoint2D()).IsOn(point);
        }
    }
}

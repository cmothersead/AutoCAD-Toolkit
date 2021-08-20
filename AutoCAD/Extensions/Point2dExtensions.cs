using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD
{
    public static class Point2dExtensions
    {
        public static Point3d ToPoint3d(this Point2d point) => new Point3d(point.X, point.Y, 0);

        public static Point2d Closest(this Point2d point, List<Point2d> points) => points.OrderBy(p => p.GetDistanceTo(point)).First();
    }
}

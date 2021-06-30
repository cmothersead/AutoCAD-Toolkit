using Autodesk.AutoCAD.Geometry;

namespace ICA.AutoCAD
{
    public static class Point2dExtensions
    {
        public static Point3d ToPoint3d(this Point2d point) => new Point3d(point.X, point.Y, 0);
    }
}

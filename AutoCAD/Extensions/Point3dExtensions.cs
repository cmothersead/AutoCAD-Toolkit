using Autodesk.AutoCAD.Geometry;

namespace ICA.AutoCAD
{
    public static class Point3dExtensions
    {
        public static Point2d ToPoint2D(this Point3d point) => new Point2d(point.X, point.Y);
    }
}

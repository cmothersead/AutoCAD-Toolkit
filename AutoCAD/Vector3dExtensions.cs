using Autodesk.AutoCAD.Geometry;

namespace ICA.AutoCAD
{
    public static class Vector3dExtensions
    {
        public static Vector2d ToVector2d(this Vector3d vector) => new Vector2d(vector.X, vector.Y);
    }
}

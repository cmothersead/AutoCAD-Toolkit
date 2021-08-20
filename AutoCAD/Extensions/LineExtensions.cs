using Autodesk.AutoCAD.DatabaseServices;

namespace ICA.AutoCAD
{
    public static class LineExtensions
    {
        /// <summary>
        /// Returns the angle between the X axis and this line's 2d delta in the range [0, 2*pi)
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static double GetAngle2d(this Line line) => line.Delta.ToVector2d().Angle;
    }
}

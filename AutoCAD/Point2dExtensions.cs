using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD
{
    public static class Point2dExtensions
    {
        public static Point3d ToPoint3d(this Point2d point)
        {
            return new Point3d(point.X, point.Y, 0);
        }
    }
}

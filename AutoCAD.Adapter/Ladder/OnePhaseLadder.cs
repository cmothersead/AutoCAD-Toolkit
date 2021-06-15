using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter
{
    public class OnePhaseLadder : Ladder
    {
        public OnePhaseLadder(Point2d origin, double height, double width, double lineHeight, int firstReference) : base(origin, height, width, lineHeight, firstReference)
        {
        }

        public new int NumberOfPhases => 1;
    }
}

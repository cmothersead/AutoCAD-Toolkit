using Autodesk.AutoCAD.Geometry;

namespace ICA.AutoCAD.Adapter
{
    public class ThreePhaseLadder : Ladder
    {
        public ThreePhaseLadder(Point2d origin, double height, double width, double lineHeight, int firstReference) : base(origin, height, width, lineHeight, firstReference)
        {
        }

        public new int NumberOfPhases => 3;
    }
}

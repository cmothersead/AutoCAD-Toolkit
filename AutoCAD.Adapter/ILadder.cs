using Autodesk.AutoCAD.Geometry;

namespace ICA.AutoCAD.Adapter
{
    public interface ILadder
    {
        Point2d Origin { get; set; }
        double Height { get; set; }
        double Width { get; set; }
        int FirstReference { get; set; }
        double LineHeight { get; set; }
        int NumberOfPhases { get; }
    }
}

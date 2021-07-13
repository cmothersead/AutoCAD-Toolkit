using Autodesk.AutoCAD.Geometry;

namespace ICA.AutoCAD.Adapter
{
    public interface ILadder
    {
        Point2d Origin { get; }
        double Height { get; }
        double Width { get; }
        int FirstReference { get; }
        int LastReference { get; }
        double LineHeight { get; }
        int PhaseCount { get; }
    }
}

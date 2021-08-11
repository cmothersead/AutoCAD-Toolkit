using Autodesk.AutoCAD.Geometry;

namespace ICA.AutoCAD.Adapter
{
    public class TitleBlockProperties : Properties
    {
        #region Public Properties

        public Point2d LadderOrigin { get; set; }
        public RungOrientation LadderOrientation { get; set; }
        public double LadderHeight { get; set; }
        public double TotalWidth { get; set; }
        public double Gap { get; set; }

        #endregion
    }
}

using Autodesk.AutoCAD.Geometry;

namespace ICA.AutoCAD.Adapter
{
    public class LadderTemplate
    {
        public Point2d Origin { get; set; }
        public double Height { get; set; }
        public double TotalWidth { get; set; }
        public double Gap { get; set; }

        public int FirstReference { get; set; } = 0;
        public double LineHeight { get; set; } = 0.5;
        public double PhaseWidth { get; set; } = 1;

        public int LadderCount { get; set; } = 1;
        public int PhaseCount { get; set; } = 1;

        public void Insert()
        {
            double width = PhaseWidth;

            if (LadderCount == 2)
            {
                double separation = ((TotalWidth - Gap) / 2) + Gap;

                if (PhaseCount != 3)
                    width = (TotalWidth - Gap) / 2;
                
                Ladder firstLadder = new Ladder()
                {
                    Origin = Origin,
                    Height = Height,
                    Width = width,
                    FirstReference = FirstReference,
                    LineHeight = LineHeight,
                    PhaseCount = PhaseCount
                };
                Ladder secondLadder = new Ladder()
                {
                    Origin = new Point2d(Origin.X + separation, Origin.Y),
                    Height = Height,
                    Width = width,
                    FirstReference = firstLadder.LastReference + 1,
                    LineHeight = LineHeight,
                    PhaseCount = PhaseCount
                };
                firstLadder.Insert();
                secondLadder.Insert();
            }
            else
            {
                if (PhaseCount != 3)
                    width = TotalWidth;

                new Ladder()
                {
                    Origin = Origin,
                    Height = Height,
                    Width = width,
                    FirstReference = FirstReference,
                    LineHeight = LineHeight,
                    PhaseCount = PhaseCount
                }
                .Insert();
            }
        }
    }
}

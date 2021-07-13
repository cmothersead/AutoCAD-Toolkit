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
                
                Ladder firstLadder = new Ladder(Origin,
                                                Height,
                                                width,
                                                LineHeight,
                                                FirstReference,
                                                PhaseCount);
                firstLadder.Insert();

                Ladder secondLadder = new Ladder(new Point2d(Origin.X + separation, Origin.Y),
                                                 Height,
                                                 width,
                                                 LineHeight,
                                                 firstLadder.LastReference + 1,
                                                 PhaseCount);
                secondLadder.Insert();
            }
            else
            {
                if (PhaseCount != 3)
                    width = TotalWidth;

                new Ladder(Origin,
                           Height,
                           width,
                           LineHeight,
                           FirstReference,
                           PhaseCount)
                .Insert();
            }
        }
    }
}

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class Ladder : Insertable, ILadder
    {
        public Point2d Origin { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public double LineHeight { get; set; } = 0.5;
        public int FirstReference { get; set; } = 0;
        public int LastReference => FirstReference + (int)(Height / LineHeight);
        public string RailLayer { get; set; } = Defaults.WireLayer;
        public int PhaseCount { get; set; } = 1;

        private string _sheetNumber;
        private string SheetNumber
        {
            get
            {
                if (_sheetNumber != null)
                    return _sheetNumber;

                _sheetNumber = Application.DocumentManager.MdiActiveDocument.Database.GetPageNumber();
                return _sheetNumber;
            }
        }

        private List<Point2d> Origins
        {
            get
            {
                List<Point2d> value = new List<Point2d>();
                for (int i = 0; i <= PhaseCount; i++)
                    value.Add(new Point2d(Origin.X + i * Width, Origin.Y));
                if (PhaseCount == 3)
                    value.RemoveAt(PhaseCount);
                return value;
            }
        }
        private List<Rail> Rails
        {
            get
            {
                List<Rail> value = new List<Rail>();
                foreach (Point2d origin in Origins)
                    value.Add(new Rail(origin, Height));
                return value;
            }
        }

        private class Rail : Line
        {
            public Rail(Point2d top, Point2d bottom, string layer = Defaults.WireLayer)
                : base(top.ToPoint3d(), bottom.ToPoint3d())
            {
                base.Layer = layer;
            }

            public Rail(Point2d top, double length, string layer = Defaults.WireLayer)
                : base(top.ToPoint3d(), new Point3d(top.X, top.Y - length, 0))
            {
                base.Layer = layer;
            }
        }

        private List<LineNumber> LineNumbers
        {
            get
            {
                List<LineNumber> list = new List<LineNumber>();
                int reference = FirstReference;
                for(double y = Origin.Y; Origin.Y - y <= Height; y -= LineHeight)
                {
                    list.Add(new LineNumber(SheetNumber + reference.ToString("D2"), new Point2d(Origin.X, y)));
                    reference++;
                }
                return list;
            }
        }

        private class LineNumber : BlockReference
        {
            private readonly string _number;

            public LineNumber(string number, Point2d location) : base(location.ToPoint3d(), SchematicSymbolRecord.GetRecord("LINENUMBER").ObjectId)
            {
                _number = number;
            }

            public void Insert(Database database, Transaction transaction)
            {
                this.Insert(database, transaction, new Dictionary<string, string>
                {
                    { "LINENUMBER", _number }
                });
            }
        }

        public Ladder() : base(null) { }

        public override void Insert(Database database, Transaction transaction)
        {
            foreach (Rail rail in Rails)
                rail.Insert(database, transaction);

            foreach (LineNumber lineNumber in LineNumbers)
                lineNumber.Insert(database, transaction);
        }
    }
}

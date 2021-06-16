using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public abstract class Ladder : Insertable, ILadder
    {
        public Point2d Origin { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public double LineHeight { get; set; }
        public int FirstReference { get; set; }
        public string RailLayer { get; set; }

        public int NumberOfPhases { get; }

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

        private Point2d TopLeft => Origin;
        private Point2d TopRight => new Point2d(Origin.X + Width, Origin.Y);
        private List<Rail> Rails => new List<Rail>
        {
            new Rail(TopLeft, Height, RailLayer),
            new Rail(TopRight, Height, RailLayer)
        };

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

        public Ladder(Point2d origin, double height, double width, double lineHeight, int firstReference, string railLayer = Defaults.WireLayer) : base(null)
        {
            Origin = origin;
            Height = height;
            Width = width;
            LineHeight = lineHeight;
            FirstReference = firstReference;
            RailLayer = railLayer;
        }

        public override void Insert(Database database, Transaction transaction)
        {
            foreach (Rail rail in Rails)
                rail.Insert(database, transaction);

            foreach (LineNumber lineNumber in LineNumbers)
                lineNumber.Insert(database, transaction);
        }
    }
}

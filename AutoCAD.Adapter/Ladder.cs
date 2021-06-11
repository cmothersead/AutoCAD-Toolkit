using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private Point2d TopLeft => Origin;
        private Point2d TopRight => new Point2d(Origin.X + Width, Origin.Y);
        private List<Rail> Rails => new List<Rail>
        {
            new Rail(TopLeft, Height, RailLayer),
            new Rail(TopRight, Height, RailLayer)
        };

        private class Rail : Insertable
        {
            public Rail(Line line) : base(line) { }

            public Rail(Point2d top, Point2d bottom, string layer = Defaults.WireLayer)
                : base(new Line(top.ToPoint3d(), bottom.ToPoint3d()) { Layer = layer }) { }

            public Rail(Point2d top, double length, string layer = Defaults.WireLayer)
                : base(new Line(top.ToPoint3d(), new Point3d(top.X, top.Y - length, 0)) { Layer = layer }) { }
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
        }
    }
}

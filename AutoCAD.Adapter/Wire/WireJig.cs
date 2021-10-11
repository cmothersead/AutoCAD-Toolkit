using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using static ICA.AutoCAD.Adapter.ConnectionPoint;

namespace ICA.AutoCAD.Adapter
{
    public class WireJig : EntityJig
    {
        #region Fields

        #region Private Fields

        private Point2d _position;
        private List<WireConnection> _startPoints;

        #endregion

        #endregion

        #region Constructors

        public WireJig(Line line, List<WireConnection> startPoints) : base(line)
        {
            _position = line.EndPoint.ToPoint2D();
            _startPoints = startPoints;
        }

        #endregion

        #region Methods

        #region Protected Methods

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions options = new JigPromptPointOptions("\nNext wire point:")
            {
                UserInputControls = UserInputControls.NoZeroResponseAccepted
            };

            PromptPointResult result = prompts.AcquirePoint(options);

            if (_position == result.Value.ToPoint2D())
                return SamplerStatus.NoChange;

            _position = result.Value.ToPoint2D();
            return SamplerStatus.OK;
        }

        protected override bool Update()
        {
            Line line = Entity as Line;
            if (_startPoints != null)
            {
                WireConnection closest = _startPoints.Aggregate((wc1, wc2) => _position.GetDistanceTo(wc1.Location) < _position.GetDistanceTo(wc2.Location) ? wc1 : wc2);
                line.StartPoint = closest.Location.ToPoint3d();
                double dX = _position.X - line.StartPoint.X;
                double dY = _position.Y - line.StartPoint.Y;

                if (dX < 0 && !closest.WireDirection.HasFlag(Orientation.Left))
                    _position = new Point2d(line.StartPoint.X, _position.Y);
                if (dX > 0 && !closest.WireDirection.HasFlag(Orientation.Right))
                    _position = new Point2d(line.StartPoint.X, _position.Y);
                if (dY < 0 && !closest.WireDirection.HasFlag(Orientation.Down))
                    _position = new Point2d(_position.X, line.StartPoint.Y);
                if (dY > 0 && !closest.WireDirection.HasFlag(Orientation.Up))
                    _position = new Point2d(_position.X, line.StartPoint.Y);

                dX = _position.X - line.StartPoint.X;
                dY = _position.Y - line.StartPoint.Y;


                if (Math.Abs(dX) > Math.Abs(dY))
                    if (closest.WireDirection.HasFlag(Orientation.Left) | closest.WireDirection.HasFlag(Orientation.Right))
                        line.EndPoint = new Point3d(_position.X, line.StartPoint.Y, 0);
                    else
                        line.EndPoint = line.StartPoint;
                else
                    if (closest.WireDirection.HasFlag(Orientation.Down) | closest.WireDirection.HasFlag(Orientation.Up))
                    line.EndPoint = new Point3d(line.StartPoint.X, _position.Y, 0);
                else
                    line.EndPoint = line.StartPoint;
            }
            return true;
        }

        #endregion

        #region Public Methods

        public PromptStatus Run()
        {
            Document document = Application.DocumentManager.MdiActiveDocument;
            if (document == null)
                return PromptStatus.Error;

            return document.Editor.Drag(this).Status;
        }

        #endregion

        #endregion
    }
}

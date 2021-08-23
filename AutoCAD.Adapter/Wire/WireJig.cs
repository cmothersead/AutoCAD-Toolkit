using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD.Adapter
{
    public class WireJig : EntityJig
    {
        private Point2d _position;
        private List<WireConnection> _startPoints;

        public WireJig(Line line, List<WireConnection> startPoints) : base(line)
        {
            _position = line.EndPoint.ToPoint2D();
            _startPoints = startPoints;
        }

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
            if(_startPoints != null)
                line.StartPoint = _position.Closest(_startPoints.Select(wc => wc.Location).ToList()).ToPoint3d();
            if (Math.Abs(line.StartPoint.X - _position.X) > Math.Abs(line.StartPoint.Y - _position.Y))
                line.EndPoint = new Point3d(_position.X, line.StartPoint.Y, line.StartPoint.Z);
            else
                line.EndPoint = new Point3d(line.StartPoint.X, _position.Y, line.StartPoint.Z);
            return true;
        }

        public PromptStatus Run()
        {
            Document document = Application.DocumentManager.MdiActiveDocument;
            if (document == null)
                return PromptStatus.Error;

            return document.Editor.Drag(this).Status;
        }
    }
}

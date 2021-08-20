using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter
{
    public class WireJig : EntityJig
    {
        private Point3d _position;

        public WireJig(Line line) : base(line)
        {
            _position = line.EndPoint;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions options = new JigPromptPointOptions("\nNext wire point:")
            {
                UserInputControls = UserInputControls.NoZeroResponseAccepted
            };

            PromptPointResult result = prompts.AcquirePoint(options);

            if (_position == result.Value)
                return SamplerStatus.NoChange;

            _position = result.Value;
            return SamplerStatus.OK;
        }

        protected override bool Update()
        {
            Line line = Entity as Line;
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

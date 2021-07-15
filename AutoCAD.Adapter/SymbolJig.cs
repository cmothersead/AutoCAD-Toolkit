using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class SymbolJig : EntityJig
    {
        private Matrix3d _ucs;
        private Point3d _position;
        private Transaction _transaction;

        public SymbolJig(Matrix3d ucs, Transaction transaction, BlockReference blockReference) : base(blockReference)
        {
            _ucs = ucs;
            _position = blockReference.Position;
            _transaction = transaction;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions options = new JigPromptPointOptions("\nSelect insertion point:")
            {
                BasePoint = Point3d.Origin,
                UserInputControls = UserInputControls.NoZeroResponseAccepted
            };

            PromptPointResult result = prompts.AcquirePoint(options);
            Point3d ucsPoint = result.Value.TransformBy(_ucs.Inverse());

            if (_position == ucsPoint)
                return SamplerStatus.NoChange;

            _position = ucsPoint;
            return SamplerStatus.OK;
        }

        protected override bool Update()
        {
            BlockReference blockReference = Entity as BlockReference;
            blockReference.MoveTo(_transaction, _position);
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

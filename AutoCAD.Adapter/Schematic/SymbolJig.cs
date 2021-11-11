using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace ICA.AutoCAD.Adapter
{
    public class SymbolJig : EntityJig
    {
        private Matrix3d _ucs;
        private Point3d _position;
        private Transaction _transaction;
        private string _prompt;

        public SymbolJig(Matrix3d ucs, Transaction transaction, BlockReference blockReference, string prompt) : base(blockReference)
        {
            _ucs = ucs;
            _position = blockReference.Position;
            _transaction = transaction;
            _prompt = prompt;
        }

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions options = new JigPromptPointOptions(_prompt)
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
            blockReference.TransformBy(Matrix3d.Displacement(blockReference.Position.GetVectorTo(_position)));
            blockReference.Position = _position;
            return true;
        }

        public static PromptStatus Run(Document document, Symbol symbol)
        {
            if (document == null)
                return PromptStatus.Error;

            if (symbol.Database != null && document.Database != symbol.Database)
                return PromptStatus.Error;

            using (Transaction transaction = document.Database.TransactionManager.StartTransaction())
            {
                PromptStatus result = document.Editor.Drag(symbol.GetJig(document, transaction)).Status;
                transaction.Commit();
                return result;
            }
        }

        public PromptStatus Run(Document document)
        {
            if (document == null)
                return PromptStatus.Error;

            if (document.Database != Entity.Database)
                return PromptStatus.Error;

            return document.Editor.Drag(this).Status;
        }
    }
}

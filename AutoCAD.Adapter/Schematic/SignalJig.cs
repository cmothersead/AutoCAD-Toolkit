using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace ICA.AutoCAD.Adapter
{
    public class SignalJig : EntityJig
    {
        #region Fields

        #region Private Fields

        private Editor _editor;
        private Matrix3d _ucs;
        private Point3d _position;
        private Transaction _transaction;

        #endregion

        #endregion

        #region Constructors

        public SignalJig(Editor editor, Matrix3d ucs, Transaction transaction, BlockReference blockReference) : base(blockReference)
        {
            _editor = editor;
            _ucs = ucs;
            _position = blockReference.Position;
            _transaction = transaction;
        }

        #endregion

        #region Methods

        #region Protected Methods

        protected override SamplerStatus Sampler(JigPrompts prompts)
        {
            JigPromptPointOptions options = new JigPromptPointOptions("\nSelect insertion point:")
            {
                BasePoint = Point3d.Origin,
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

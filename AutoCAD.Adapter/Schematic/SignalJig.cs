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
        private Point2d _position;
        private Transaction _transaction;

        #endregion

        #endregion

        #region Constructors

        public SignalJig(Editor editor, Matrix3d ucs, Transaction transaction, BlockReference blockReference) : base(blockReference)
        {
            _editor = editor;
            _ucs = ucs;
            _position = blockReference.Position.ToPoint2D();
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
            Point2d ucsPoint = result.Value.TransformBy(_ucs.Inverse()).ToPoint2D();


            if (_position == ucsPoint)
                return SamplerStatus.NoChange;

            _position = ucsPoint;
            return SamplerStatus.OK;
        }

        protected override bool Update()
        {
            BlockReference blockReference = Entity as BlockReference;
            
            return true;
        }

        #endregion

        #region Public Methods

        public PromptStatus Run() => _editor?.Drag(this).Status ?? PromptStatus.Error;

        #endregion

        #endregion
    }
}

using Autodesk.AutoCAD.DatabaseServices;
using ICA.Schematic;

namespace ICA.AutoCAD.Adapter
{
    public class SignalSymbol : IReference
    {
        #region Private Properties

        private BlockReference BlockReference { get; }

        private AttributeReference SignalCodeAttribute => BlockReference.GetAttributeReference("SIGCODE");

        private AttributeReference DescriptionAttribute => BlockReference.GetAttributeReference("DESC1");

        private AttributeReference XrefSheetAttribute => BlockReference.GetAttributeReference("SHEET");

        private AttributeReference XrefLineAttribute => BlockReference.GetAttributeReference("XREF");

        private AttributeReference WireNumberAttribute => BlockReference.GetAttributeReference("WIRENO");

        #endregion

        #region Public Properties

        public string SignalCode
        {
            get => SignalCodeAttribute?.TextString;
            set => SignalCodeAttribute?.SetValue(value);
        }

        public string Description
        {
            get => DescriptionAttribute?.TextString;
            set => DescriptionAttribute?.SetValue(value);
        }

        public string XrefSheet
        {
            get => XrefSheetAttribute?.TextString;
            set => XrefSheetAttribute?.SetValue(value);
        }

        public string XrefLine
        {
            get => XrefLineAttribute?.TextString;
            set => XrefLineAttribute?.SetValue(value);
        }

        public string WireNumber
        {
            get => WireNumberAttribute?.TextString;
            set => WireNumberAttribute?.SetValue(value);
        }

        public string Sheet => BlockReference.Database.GetSheetNumber();

        public string Line => BlockReference.Database.GetLadder()?.ClosestLineNumber(BlockReference.Position).Replace(Sheet, "");

        #endregion

        #region Constructors

        public SignalSymbol(BlockReference blockReference) => BlockReference = blockReference;

        #endregion

        #region Public Methods

        public void CrossReference(SignalSymbol referenceSymbol)
        {
            XrefSheet = referenceSymbol.Sheet;
            XrefLine = referenceSymbol.Line;
            referenceSymbol.XrefSheet = Sheet;
            referenceSymbol.XrefLine = Line;
            referenceSymbol.SignalCode = SignalCode;
        }

        #endregion
    }
}

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ICA.AutoCAD
{
    public static class AttributeReferenceExtensions
    {
        #region Public Transactionless Extension Methods

        public static Point2d GetPosition(this AttributeReference attributeReference) => attributeReference.Justify
                                                                                         == AttachmentPoint.BaseLeft ?
                                                                                         attributeReference.Position.ToPoint2D() :
                                                                                         attributeReference.AlignmentPoint.ToPoint2D();

        #endregion

        #region Public Extenstion Methods

        public static void Hide(this AttributeReference attributeReference, Transaction transaction) => attributeReference.GetForWrite(transaction).Invisible = true;

        public static void Unhide(this AttributeReference attributeReference, Transaction transaction) => attributeReference.GetForWrite(transaction).Invisible = false;

        public static void ToggleVisibility(this AttributeReference attributeReference, Transaction transaction) => attributeReference.GetForWrite(transaction).Invisible ^= true;

        public static void SetVisibility(this AttributeReference attributeReference, Transaction transaction, bool value) => attributeReference.GetForWrite(transaction).Invisible = !value;

        public static void SetValue(this AttributeReference attributeReference, Transaction transaction, string value)
        {
            Database previousDatabase = HostApplicationServices.WorkingDatabase;
            HostApplicationServices.WorkingDatabase = attributeReference.Database;
            AttributeReference forWrite = attributeReference.GetForWrite(transaction);
            forWrite.TextString = value;
            forWrite.AdjustAlignment(attributeReference.Database);
            HostApplicationServices.WorkingDatabase = previousDatabase;
        }

        public static void SetPosition(this AttributeReference attributeReference, Transaction transaction, Point3d position)
        {
            if (attributeReference.Justify == AttachmentPoint.BaseLeft)
                attributeReference.GetForWrite(transaction).Position = position;
            else
                attributeReference.GetForWrite(transaction).AlignmentPoint = position;
        }

        #endregion

        #region Transacted Overloads

        public static void Hide(this AttributeReference attributeReference) => attributeReference.Transact(Hide);

        public static void Unhide(this AttributeReference attributeReference) => attributeReference.Transact(Unhide);

        public static void ToggleVisibility(this AttributeReference attributeReference) => attributeReference.Transact(ToggleVisibility);

        public static void SetVisibility(this AttributeReference attributeReference, bool value) => attributeReference.Transact(SetVisibility, value);

        public static void SetValue(this AttributeReference attributeReference, string value) => attributeReference.Transact(SetValue, value);

        public static void SetPosition(this AttributeReference attributeReference, Point3d position) => attributeReference.Transact(SetPosition, position);

        #endregion
    }
}

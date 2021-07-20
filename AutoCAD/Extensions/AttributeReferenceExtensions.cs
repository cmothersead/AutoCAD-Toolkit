using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ICA.AutoCAD
{
    public static class AttributeReferenceExtensions
    {
        #region Public Extenstion Methods

        public static void Hide(this AttributeReference attributeReference, Transaction transaction) => attributeReference.GetForWrite(transaction).Invisible = true;

        public static void Unhide(this AttributeReference attributeReference, Transaction transaction) => attributeReference.GetForWrite(transaction).Invisible = false;

        public static void SetValue(this AttributeReference attributeReference, Transaction transaction, string value) => attributeReference.GetForWrite(transaction).TextString = value;

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

        public static void SetValue(this AttributeReference attributeReference, string value) => attributeReference.Transact(SetValue, value);

        public static void SetPosition(this AttributeReference attributeReference, Point3d position) => attributeReference.Transact(SetPosition, position);

        #endregion
    }
}

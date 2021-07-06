using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace ICA.AutoCAD
{
    public static class AttributeReferenceExtensions
    {
        public static void Transact(this AttributeReference attributeReference, Action<AttributeReference, Transaction> action)
        {
            using (Transaction transaction = attributeReference.Database.TransactionManager.StartTransaction())
            {
                action(attributeReference, transaction);
                transaction.Commit();
            }
        }

        public static void Transact(this AttributeReference attributeReference, Action<AttributeReference, Transaction, string> action, string value)
        {
            using (Transaction transaction = attributeReference.Database.TransactionManager.StartTransaction())
            {
                action(attributeReference, transaction, value);
                transaction.Commit();
            }
        }

        public static void Transact(this AttributeReference attributeReference, Action<AttributeReference, Transaction, Point3d> action, Point3d value)
        {
            using (Transaction transaction = attributeReference.Database.TransactionManager.StartTransaction())
            {
                action(attributeReference, transaction, value);
                transaction.Commit();
            }
        }

        public static AttributeReference GetForWrite(this AttributeReference attributeReference, Transaction transaction) => transaction.GetObject(attributeReference.ObjectId, OpenMode.ForWrite) as AttributeReference;

        public static void Hide(this AttributeReference attributeReference, Transaction transaction) => attributeReference.GetForWrite(transaction).Invisible = true;

        public static void Unhide(this AttributeReference attributeReference, Transaction transaction) => attributeReference.GetForWrite(transaction).Invisible = false;

        public static void SetValue(this AttributeReference attributeReference, Transaction transaction, string value) => attributeReference.GetForWrite(transaction).TextString = value;

        public static void SetPosition(this AttributeReference attributeReference, Transaction transaction, Point3d position) => attributeReference.GetForWrite(transaction).AlignmentPoint = position;

        public static void Hide(this AttributeReference attributeReference) => attributeReference.Transact(Hide);

        public static void Unhide(this AttributeReference attributeReference) => attributeReference.Transact(Unhide);

        public static void SetValue(this AttributeReference attributeReference, string value) => attributeReference.Transact(SetValue, value);

        public static void SetPosition(this AttributeReference attributeReference, Point3d position) => attributeReference.Transact(SetPosition, position);
    }
}

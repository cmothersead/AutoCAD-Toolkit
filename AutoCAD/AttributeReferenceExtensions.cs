using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ICA.AutoCAD
{
    public static class AttributeReferenceExtensions
    {
        public static bool Hide(this AttributeReference attributeReference)
        {
            try
            {
                using (Transaction transaction = attributeReference.Database.TransactionManager.StartTransaction())
                {

                    AttributeReference attributeReferenceForWrite = transaction.GetObject(attributeReference.ObjectId, OpenMode.ForWrite) as AttributeReference;
                    attributeReferenceForWrite.Invisible = true;
                    transaction.Commit();
                    return true;

                }
            }
            catch
            {
                return false;
            }
        }

        public static bool Unhide(this AttributeReference attributeReference)
        {
            try
            {
                using (Transaction transaction = attributeReference.Database.TransactionManager.StartTransaction())
                {

                    AttributeReference atttributeReferenceForWrite = transaction.GetObject(attributeReference.ObjectId, OpenMode.ForWrite) as AttributeReference;
                    atttributeReferenceForWrite.Invisible = false;
                    transaction.Commit();
                    return true;

                }
            }
            catch
            {
                return false;
            }
        }

        public static bool SetValue(this AttributeReference attributeReference, string value)
        {
            try
            {
                using (Transaction transaction = attributeReference.Database.TransactionManager.StartTransaction())
                {
                    AttributeReference attributeReferenceForWrite = transaction.GetObject(attributeReference.ObjectId, OpenMode.ForWrite) as AttributeReference;
                    attributeReferenceForWrite.TextString = value;
                    transaction.Commit();
                    return true;

                }
            }
            catch
            {
                return false;
            }
        }

        public static bool SetPosition(this AttributeReference attributeReference, Point3d position)
        {
            try
            {
                using (Transaction transaction = attributeReference.Database.TransactionManager.StartTransaction())
                {

                    AttributeReference attributeReferenceForWrite = transaction.GetObject(attributeReference.ObjectId, OpenMode.ForWrite) as AttributeReference;
                    attributeReferenceForWrite.AlignmentPoint = position;
                    transaction.Commit();
                    return true;

                }
            }
            catch
            {
                return false;
            }
        }
    }
}

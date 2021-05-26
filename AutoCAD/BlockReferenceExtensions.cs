using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace ICA.AutoCAD
{
    public static class BlockReferenceExtensions
    {
        public static AttributeReference GetAttributeReference(this BlockReference blockReference, string tag)
        {
            using (Transaction transaction = blockReference.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId attRefID in blockReference.AttributeCollection)
                {
                    AttributeReference attRef = transaction.GetObject(attRefID, OpenMode.ForRead) as AttributeReference;
                    if (attRef.Tag == tag)
                    {
                        return attRef;
                    }
                }
            }
            return null;
        }

        public static AttributeReference GetAttributeReference(this BlockReference blockReference, ObjectId id)
        {
            using (Transaction transaction = blockReference.Database.TransactionManager.StartTransaction())
            {
                return transaction.GetObject(id, OpenMode.ForRead) as AttributeReference;
            }
        }

        public static bool HasAttributes(this BlockReference blockReference)
        {
            return blockReference.AttributeCollection.Count > 0;
        }

        public static void MoveTo(this BlockReference blockRefrerence, Point3d position)
        {
            using (Transaction transaction = blockRefrerence.Database.TransactionManager.StartTransaction())
            {
                blockRefrerence.MoveTo(position, transaction);
                transaction.Commit();
            }
        }

        public static void MoveTo(this BlockReference blockRefrerence, Point3d position, Transaction transaction)
        {
            Matrix3d transform = Matrix3d.Displacement(blockRefrerence.Position.GetVectorTo(position));
            BlockReference blockReferenceWriteMode = transaction.GetObject(blockRefrerence.ObjectId, OpenMode.ForWrite) as BlockReference;
            blockReferenceWriteMode.TransformBy(transform);
        }

        public static void Insert(this BlockReference blockReference, Database database)
        {
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                blockReference.Insert(database, transaction);
                transaction.Commit();
            }
        }

        public static void Insert(this BlockReference blockReference, Database database, Transaction transaction)
        {
            BlockTableRecord modelSpace = transaction.GetObject(database.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
            modelSpace.AppendEntity(blockReference);
            transaction.AddNewlyCreatedDBObject(blockReference, true);

            BlockTableRecord record = transaction.GetObject(blockReference.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
            if (record.HasAttributeDefinitions)
            {
                foreach (ObjectId id in record)
                {
                    AttributeDefinition attributeDefinition = transaction.GetObject(id, OpenMode.ForRead) as AttributeDefinition;
                    if (attributeDefinition != null && !attributeDefinition.Constant)
                    {
                        AttributeReference attributeReference = new AttributeReference();
                        attributeReference.SetAttributeFromBlock(attributeDefinition, blockReference.BlockTransform);
                        attributeReference.TextString = attributeDefinition.TextString;
                        attributeReference.AdjustAlignment(database);
                        blockReference.AttributeCollection.AppendAttribute(attributeReference);
                        transaction.AddNewlyCreatedDBObject(attributeReference, true);
                    }
                }
            }
        }
    }
}

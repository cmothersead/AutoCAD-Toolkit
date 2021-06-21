using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;

namespace ICA.AutoCAD
{
    public static class BlockReferenceExtensions
    {
        /// <summary>
        /// Gets readonly <see cref="AttributeReference"/> with the given tag, if it exists. Returns null if none found.
        /// </summary>
        /// <param name="blockReference"></param>
        /// <param name="tag">Name of the attribute reference to retrieve</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets TextString value of the <see cref="AttributeReference"/> with the given tag, if it exists. Returns null if none found.
        /// </summary>
        /// <param name="blockReference"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string GetAttributeValue(this BlockReference blockReference, string tag)
        {
            try
            {
                return blockReference.GetAttributeReference(tag).TextString;
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns true if the <see cref="BlockReference"/>'s <see cref="AttributeCollection"/> contains any elements
        /// </summary>
        /// <param name="blockReference"></param>
        /// <returns></returns>
        public static bool HasAttributes(this BlockReference blockReference)
        {
            return blockReference.AttributeCollection.Count > 0;
        }

        /// <summary>
        /// Moves <see cref="BlockReference"/> to given <see cref="Point3d"/> within self-contained transaction.
        /// </summary>
        /// <param name="blockRefrerence"></param>
        /// <param name="position">New origin point for the reference</param>
        public static void MoveTo(this BlockReference blockRefrerence, Point3d position)
        {
            using (Transaction transaction = blockRefrerence.Database.TransactionManager.StartTransaction())
            {
                blockRefrerence.MoveTo(position, transaction);
                transaction.Commit();
            }
        }

        /// <summary>
        /// Moves <see cref="BlockReference"/> to given <see cref="Point3d"/> within passed transaction.
        /// </summary>
        /// <param name="blockRefrerence"></param>
        /// <param name="position">New origin point for the reference</param>
        /// <param name="transaction">Transaction dependency for the operation</param>
        public static void MoveTo(this BlockReference blockRefrerence, Point3d position, Transaction transaction)
        {
            Matrix3d transform = Matrix3d.Displacement(blockRefrerence.Position.GetVectorTo(position));
            BlockReference blockReferenceWriteMode = transaction.GetObject(blockRefrerence.ObjectId, OpenMode.ForWrite) as BlockReference;
            blockReferenceWriteMode.TransformBy(transform);
        }

        /// <summary>
        /// Adds reference to the passed <see cref="Database"/> within passed transaction.
        /// </summary>
        /// <param name="blockReference"></param>
        /// <param name="database">Database to append the reference to</param>
        /// <param name="transaction">Transaction dependency for the operation</param>
        public static void Insert(this BlockReference blockReference, Database database, Transaction transaction)
        {
            Insert(blockReference, database, transaction, null);
        }

        public static void Insert(this BlockReference blockReference, Database database, Transaction transaction, Dictionary<string, string> attributes)
        {
            BlockTableRecord modelSpace = transaction.GetObject(database.GetModelSpace().ObjectId, OpenMode.ForWrite) as BlockTableRecord;
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
                        if (attributes.ContainsKey(attributeDefinition.Tag))
                            attributeReference.TextString = attributes[attributeDefinition.Tag];
                        else
                            attributeReference.TextString = attributeDefinition.TextString;
                        attributeReference.AdjustAlignment(database);
                        blockReference.AttributeCollection.AppendAttribute(attributeReference);
                        transaction.AddNewlyCreatedDBObject(attributeReference, true);
                    }
                }
            }
        }

        public static BlockTableRecord GetBlockTableRecord(this BlockReference blockReference)
        {
            if (blockReference.Database is null)
                return null;

            return blockReference.Database.GetBlockTable().GetRecord(blockReference.BlockTableRecord) as BlockTableRecord;
        }
    }
}

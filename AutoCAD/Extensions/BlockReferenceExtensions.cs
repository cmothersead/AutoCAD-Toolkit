using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;

namespace ICA.AutoCAD
{
    public static class BlockReferenceExtensions
    {
        #region Transactionless Public Extension Methods

        /// <summary>
        /// Returns true if the <see cref="BlockReference"/>'s <see cref="AttributeCollection"/> contains any elements
        /// </summary>
        /// <param name="blockReference"></param>
        /// <returns></returns>
        public static bool HasAttributes(this BlockReference blockReference) => blockReference.AttributeCollection.Count > 0;

        #endregion

        #region Public Extension Methods

        /// <summary>
        /// Gets readonly <see cref="AttributeReference"/> with the given tag, if it exists. Returns null if none found.
        /// </summary>
        /// <param name="blockReference"></param>
        /// <param name="tag">Name of the attribute reference to retrieve</param>
        /// <returns></returns>
        public static AttributeReference GetAttributeReference(this BlockReference blockReference, Transaction transaction, string tag)
        {
            foreach (ObjectId attRefID in blockReference.AttributeCollection)
            {
                AttributeReference attRef = transaction.GetObject(attRefID, OpenMode.ForRead) as AttributeReference;
                if (attRef.Tag == tag)
                {
                    return attRef;
                }
            }

            return null;
        }

        public static List<AttributeReference> GetAttributeReferences(this BlockReference blockReference, Transaction transaction)
        {
            List<AttributeReference> list = new List<AttributeReference>();
            foreach (ObjectId attRefID in blockReference.AttributeCollection)
                list.Add(transaction.GetObject(attRefID, OpenMode.ForRead) as AttributeReference);
            return list;
        }

        public static void AddAttributeReference(this BlockReference blockReference, Transaction transaction, AttributeReference attributeReference)
        {
            BlockReference blockReferenceForWrite = transaction.GetObject(blockReference.Id, OpenMode.ForWrite) as BlockReference;
            blockReferenceForWrite.AttributeCollection.AppendAttribute(attributeReference);
            transaction.AddNewlyCreatedDBObject(attributeReference, true);
        }

        public static void RemoveAttributeReference(this BlockReference blockReference, Transaction transaction, string tag)
        {
            AttributeReference attributeReference = blockReference.GetAttributeReference(transaction, tag);
            if (attributeReference == null)
                return;
            attributeReference.UpgradeOpen();
            attributeReference.Erase();
        }

        /// <summary>
        /// Gets TextString value of the <see cref="AttributeReference"/> with the given tag, if it exists. Returns null if none found.
        /// </summary>
        /// <param name="blockReference"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string GetAttributeValue(this BlockReference blockReference, Transaction transaction, string tag)
        {
            try
            {
                return blockReference.GetAttributeReference(transaction, tag).TextString;
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        public static void SetAttributeValue(this BlockReference blockReference, Transaction transaction, string tag, string value)
        {
            AttributeReference attributeReference = blockReference.GetAttributeReference(transaction, tag);
            if(attributeReference != null)
            {
                attributeReference.UpgradeOpen();
                attributeReference.TextString = value;
                attributeReference.DowngradeOpen();
            }
        }

        public static void SetAttributeValues(this BlockReference blockReference, Transaction transaction, Dictionary<string, string> values)
        {
            foreach (KeyValuePair<string, string> pair in values)
                blockReference.SetAttributeValue(transaction, pair.Key, pair.Value);
        }

        /// <summary>
        /// Moves <see cref="BlockReference"/> to given <see cref="Point3d"/> within passed transaction.
        /// </summary>
        /// <param name="blockRefrerence"></param>
        /// <param name="position">New origin point for the reference</param>
        /// <param name="transaction">Transaction dependency for the operation</param>
        public static void MoveTo(this BlockReference blockRefrerence, Transaction transaction, Point3d position)
        {
            Matrix3d transform = Matrix3d.Displacement(blockRefrerence.Position.GetVectorTo(position));
            BlockReference blockReferenceWriteMode = transaction.GetObject(blockRefrerence.ObjectId, OpenMode.ForWrite) as BlockReference;
            blockReferenceWriteMode.TransformBy(transform);
        }

        public static BlockTableRecord GetBlockTableRecord(this BlockReference blockReference, Transaction transaction)
        {
            if (blockReference.Database is null)
                return null;

            return blockReference.Database.GetBlockTable(transaction).GetRecord(transaction, blockReference.BlockTableRecord) as BlockTableRecord;
        }

        public static void Insert(this BlockReference blockReference, Transaction transaction, Database database) => blockReference.Insert(transaction, database, null);

        /// <summary>
        /// Adds reference to the passed <see cref="Database"/> within passed transaction.
        /// </summary>
        /// <param name="blockReference"></param>
        /// <param name="database">Database to append the reference to</param>
        /// <param name="transaction">Transaction dependency for the operation</param>
        public static void Insert(this BlockReference blockReference, Transaction transaction, Database database, Dictionary<string, string> attributes)
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
                        if (attributes != null && attributes.ContainsKey(attributeDefinition.Tag))
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

        #endregion

        #region Transacted Overloads

        public static AttributeReference GetAttributeReference(this BlockReference blockReference, string tag) => blockReference.Transact(GetAttributeReference, tag);

        public static List<AttributeReference> GetAttributeReferences(this BlockReference blockReference) => blockReference.Transact(GetAttributeReferences);

        public static void AddAttributeReference(this BlockReference blockReference, AttributeReference attributeReference) => blockReference.Transact(AddAttributeReference, attributeReference);

        public static void RemoveAttributeReference(this BlockReference blockReference, string tag) => blockReference.Transact(RemoveAttributeReference, tag);

        public static string GetAttributeValue(this BlockReference blockReference, string tag) => blockReference.Transact(GetAttributeValue, tag);

        public static void SetAttributeValue(this BlockReference blockReference, string tag, string value) => blockReference.Transact(SetAttributeValue, tag, value);

        public static void SetAttributeValues(this BlockReference blockReference, Dictionary<string, string> values) => blockReference.Transact(SetAttributeValues, values);

        public static void MoveTo(this BlockReference blockRefrerence, Point3d position) => blockRefrerence.Transact(MoveTo, position);

        public static BlockTableRecord GetBlockTableRecord(this BlockReference blockReference) => blockReference.Transact(GetBlockTableRecord);

        public static void Insert(this BlockReference blockReference, Database database) => blockReference.Transact(Insert, database);

        public static void Insert(this BlockReference blockReference, Database database, Dictionary<string, string> attributes) => blockReference.Transact(Insert, database, attributes);

        #endregion
    }
}

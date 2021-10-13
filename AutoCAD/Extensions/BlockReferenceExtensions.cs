using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static bool HasAttributeReference(this BlockReference blockReference, Transaction transaction, string tag)
        {
            return blockReference.GetAttributeReference(transaction, tag) != null;
        }

        /// <summary>
        /// Gets readonly <see cref="AttributeReference"/> with the given tag, if it exists. Returns null if none found.
        /// </summary>
        /// <param name="blockReference"></param>
        /// <param name="tag">Name of the attribute reference to retrieve</param>
        /// <returns></returns>
        public static AttributeReference GetAttributeReference(this BlockReference blockReference, Transaction transaction, string tag) => blockReference.GetAttributeReferences(transaction)
                                                                                                                                                         .FirstOrDefault(att => att.Tag == tag);

        public static IEnumerable<AttributeReference> GetAttributeReferences(this BlockReference blockReference, Transaction transaction) =>
            blockReference.AttributeCollection.Cast<ObjectId>()
                                              .Select(id => id.Open(transaction) as AttributeReference);

        public static AttributeReference AddAttributeReference(this BlockReference blockReference, Transaction transaction, AttributeReference attributeReference)
        {
            blockReference.GetForWrite(transaction).AttributeCollection
                          .AppendAttribute(attributeReference);
            transaction.AddNewlyCreatedDBObject(attributeReference, true);
            return attributeReference;
        }

        public static AttributeReference AddAttributeReference(this BlockReference blockReference, Transaction transaction, AttributeDefinition attributeDefinition, string value = null)
        {
            AttributeReference attributeReference = new AttributeReference();
            attributeReference.SetAttributeFromBlock(attributeDefinition, blockReference.BlockTransform);
            blockReference.GetForWrite(transaction).AttributeCollection
                          .AppendAttribute(attributeReference);
            transaction.AddNewlyCreatedDBObject(attributeReference, true);
            if (value != null)
                attributeReference.SetValue(transaction, value);
            return attributeReference;
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

        public static void SetAttributeValue(this BlockReference blockReference, Transaction transaction, string tag, string value) => blockReference.GetAttributeReference(transaction, tag)
                                                                                                                                                     .SetValue(transaction, value);

        public static void SetAttributeValues(this BlockReference blockReference, Transaction transaction, Dictionary<string, string> values) => values.ForEach(pair => blockReference.SetAttributeValue(transaction, pair.Key, pair.Value));

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

        public static void Insert(this BlockReference blockReference, Transaction transaction, Database database, LayerTableRecord layer = null) => blockReference.Insert(transaction, database, null, layer);

        /// <summary>
        /// Adds reference to the passed <see cref="Database"/> within passed transaction.
        /// </summary>
        /// <param name="blockReference"></param>
        /// <param name="database">Database to append the reference to</param>
        /// <param name="transaction">Transaction dependency for the operation</param>
        public static void Insert(this BlockReference blockReference, Transaction transaction, Database database, Dictionary<string, string> attributes, LayerTableRecord layer = null)
        {
            BlockTableRecord modelSpace = transaction.GetObject(database.GetModelSpace().ObjectId, OpenMode.ForWrite) as BlockTableRecord;
            modelSpace.AppendEntity(blockReference);
            transaction.AddNewlyCreatedDBObject(blockReference, true);

            BlockTableRecord record = transaction.GetObject(blockReference.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
            if (record.HasAttributeDefinitions)
            {
                record.GetAttributeDefinitions(transaction)
                      .Where(definition => definition != null && !definition.Constant)
                      .ForEach(definition => blockReference.AddAttributeReference(transaction,
                                                                                  definition,
                                                                                  (bool)attributes?.ContainsKey(definition.Tag) ? attributes[definition.Tag] : definition.TextString));
            }
            if (layer != null)
                blockReference.SetLayer(transaction, layer);
        }

        #endregion

        #region Transacted Overloads

        public static bool HasAttributeReference(this BlockReference blockReference, string tag) => blockReference.Transact(HasAttributeReference, tag);

        public static AttributeReference GetAttributeReference(this BlockReference blockReference, string tag) => blockReference.Transact(GetAttributeReference, tag);

        public static IEnumerable<AttributeReference> GetAttributeReferences(this BlockReference blockReference) => blockReference.Transact(GetAttributeReferences);

        public static AttributeReference AddAttributeReference(this BlockReference blockReference, AttributeReference attributeReference) => blockReference.Transact(AddAttributeReference, attributeReference);

        public static void RemoveAttributeReference(this BlockReference blockReference, string tag) => blockReference.Transact(RemoveAttributeReference, tag);

        public static string GetAttributeValue(this BlockReference blockReference, string tag) => blockReference.Transact(GetAttributeValue, tag);

        public static void SetAttributeValue(this BlockReference blockReference, string tag, string value) => blockReference.Transact(SetAttributeValue, tag, value);

        public static void SetAttributeValues(this BlockReference blockReference, Dictionary<string, string> values) => blockReference.Transact(SetAttributeValues, values);

        public static void MoveTo(this BlockReference blockRefrerence, Point3d position) => blockRefrerence.Transact(MoveTo, position);

        public static BlockTableRecord GetBlockTableRecord(this BlockReference blockReference) => blockReference.Transact(GetBlockTableRecord);

        public static void Insert(this BlockReference blockReference, Database database, LayerTableRecord layer = null) => blockReference.Transact(Insert, database, layer);

        public static void Insert(this BlockReference blockReference, Database database, Dictionary<string, string> attributes, LayerTableRecord layer) => blockReference.Transact(Insert, database, attributes, layer);

        #endregion
    }
}

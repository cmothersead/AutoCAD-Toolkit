using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD
{
    public static class BlockTableRecordExtensions
    {
        #region Public Extension Methods

        public static bool HasAttribute(this BlockTableRecord record, Transaction transaction, string name) => record.GetAttributeDefinitions(transaction)
                                                                                                                     .Any(definition => definition.Tag == name);

        public static IEnumerable<AttributeDefinition> GetAttributeDefinitions(this BlockTableRecord record, Transaction transaction) => ((IEnumerable<ObjectId>)record).Select(id => id.Open(transaction))
                                                                                                                                                                 .OfType<AttributeDefinition>();

        public static IEnumerable<Entity> GetEntities(this BlockTableRecord record, Transaction transaction) => ((IEnumerable<ObjectId>)record).Select(id => id.Open(transaction))
                                                                                                                                        .OfType<Entity>();

        public static IEnumerable<BlockReference> GetBlockReferences(this BlockTableRecord record, Transaction transaction) => record.GetBlockReferenceIds(true, false)
                                                                                                                              .OfType<ObjectId>()
                                                                                                                              .Select(id => id.Open(transaction) as BlockReference);

        #endregion

        #region Transacted Overloads

        public static bool HasAttribute(this BlockTableRecord record, string name) => record.Transact(HasAttribute, name);

        public static List<AttributeDefinition> GetAttributeDefinitions(this BlockTableRecord record) => record.Transact(GetAttributeDefinitions);

        public static List<Entity> GetEntities(this BlockTableRecord record) => record.Transact(GetEntities);

        public static List<BlockReference> GetBlockReferences(this BlockTableRecord record) => record.Transact(GetBlockReferences);

        #endregion
    }
}

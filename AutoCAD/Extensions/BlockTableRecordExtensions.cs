using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD
{
    public static class BlockTableRecordExtensions
    {
        #region Public Extension Methods

        public static bool HasAttribute(this BlockTableRecord record, Transaction transaction, string name) => record.AttributeDefinitions(transaction).Select(definition => definition.Tag).ToList().Contains(name);

        public static List<AttributeDefinition> AttributeDefinitions(this BlockTableRecord record, Transaction transaction)
        {
            List<AttributeDefinition> result = new List<AttributeDefinition>();
            foreach(ObjectId id in record)
            {
                DBObject obj = transaction.GetObject(id, OpenMode.ForRead);
                if (obj is AttributeDefinition attributeDefinition)
                    result.Add(attributeDefinition);
            }
            return result;
        }

        #endregion

        #region Transacted Overloads

        public static bool HasAttribute(this BlockTableRecord record, string name) => record.Transact(HasAttribute, name);

        public static List<AttributeDefinition> AttributeDefinitions(this BlockTableRecord record) => record.Transact(AttributeDefinitions);

        #endregion
    }
}

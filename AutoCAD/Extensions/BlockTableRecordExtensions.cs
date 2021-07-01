using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD
{
    public static class BlockTableRecordExtensions
    {
        public static bool HasAttribute(this BlockTableRecord record, string name)
        {
            return record.AttributeDefinitions().Select(definition => definition.Tag).ToList().Contains(name);
        }

        public static List<AttributeDefinition> AttributeDefinitions(this BlockTableRecord record)
        {
            List<AttributeDefinition> result = new List<AttributeDefinition>();
            foreach(ObjectId id in record)
            {
                DBObject obj = record.Database.Open(id);
                if (obj is AttributeDefinition attributeDefinition)
                    result.Add(attributeDefinition);
            }
            return result;
        }
    }
}

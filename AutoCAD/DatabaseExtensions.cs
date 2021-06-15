using Autodesk.AutoCAD.DatabaseServices;
using System.Collections;
using System.Collections.Generic;

namespace ICA.AutoCAD
{
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Get the <see cref="BlockTable"/> for the database.
        /// </summary>
        /// <param name="database">Database instance this method is applied to.</param>
        /// <returns>A <see cref="BlockTable"/> opened for read.</returns>
        public static BlockTable GetBlockTable (this Database database)
        {
            using(Transaction transaction = database.TransactionManager.StartTransaction())
            {
                return transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;
            }
        }

        /// <summary>
        /// Gets the <see cref="BlockTableRecord"/> with the given name from the database, if it exists.
        /// </summary>
        /// <param name="database">Instance for this method to be applied to.</param>
        /// <param name="name">String name of the block to retrive.</param>
        /// <returns><see cref="BlockTableRecord"/> with the given name in the database.</returns>
        public static BlockTableRecord GetBlockTableRecord(this Database database, string name)
        {
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                return transaction.GetObject(database.GetBlockTable()[name], OpenMode.ForRead) as BlockTableRecord;
            }
        }

        /// <summary>
        /// Get the "Model Space" <see cref="BlockTableRecord"/> for the database.
        /// </summary>
        /// <param name="database">Instance for this method to be applied to.</param>
        /// <returns><see cref="BlockTableRecord"/> representing the model space of the database.</returns>
        public static BlockTableRecord GetModelSpace(this Database database)
        {
            using(Transaction transaction = database.TransactionManager.StartTransaction())
            {
                return transaction.GetObject(database.GetBlockTable()[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
            }
        }

        /// <summary>
        /// Gets the drawing custom properties.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <returns>A strongly typed dictionary containing the entries.</returns>
        public static Dictionary<string, string> GetCustomProperties(this Database database)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            IDictionaryEnumerator dictEnum = database.SummaryInfo.CustomProperties;
            while (dictEnum.MoveNext())
            {
                DictionaryEntry entry = dictEnum.Entry;
                result.Add((string)entry.Key, (string)entry.Value);
            }
            return result;
        }

        /// <summary>
        /// Gets a drawing custom property.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <param name="key">Property key.</param>
        /// <returns>The property value or null if not found</returns>
        public static string GetCustomProperty(this Database database, string key)
        {
            DatabaseSummaryInfoBuilder sumInfo = new DatabaseSummaryInfoBuilder(database.SummaryInfo);
            IDictionary custProps = sumInfo.CustomPropertyTable;
            return (string)custProps[key];
        }

        /// <summary>
        /// Sets multiple drawing custom properties.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <param name="properties">Property key-value pairs.</param>
        public static void SetCustomProperties(this Database database, Dictionary<string, string> properties)
        {
            foreach (var entry in properties)
                database.SetCustomProperty(entry.Key, entry.Value);
        }

        /// <summary>
        /// Sets a property value
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <param name="key">Property key.</param>
        /// <param name="value">Property value.</param>
        public static void SetCustomProperty(this Database database, string key, string value)
        {
            DatabaseSummaryInfoBuilder infoBuilder = new DatabaseSummaryInfoBuilder(database.SummaryInfo);
            IDictionary custProps = infoBuilder.CustomPropertyTable;
            if (custProps.Contains(key))
                custProps[key] = value;
            else
                custProps.Add(key, value);
            database.SummaryInfo = infoBuilder.ToDatabaseSummaryInfo();
        }
    }
}

using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ICA.AutoCAD
{
    public static class DatabaseExtensions
    {
        #region Transactionles Public Extension Methods

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

        #endregion

        #region Public Extension Methods

        /// <summary>
        /// Get the "Model Space" <see cref="BlockTableRecord"/> for the database.
        /// </summary>
        /// <param name="database">Instance for this method to be applied to.</param>
        /// <returns><see cref="BlockTableRecord"/> representing the model space of the database.</returns>
        public static BlockTableRecord GetModelSpace(this Database database, Transaction transaction) => database.GetBlockTable(transaction).GetRecord(BlockTableRecord.ModelSpace);

        public static bool HasLayer(this Database database, Transaction transaction, string name) => database.GetLayerTable(transaction).Has(name);

        public static bool HasLayer(this Database database, Transaction transaction, LayerTableRecord layer) => database.HasLayer(transaction, layer.Name);

        /// <summary>
        /// Gets <see cref="LayerTableRecord"/> of the layer with the given name from the database, if it exists.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <param name="layerName">Name of the desired layer.</param>
        /// <returns><see cref="LayerTableRecord"/> of the layer if it exists. Else null</returns>
        public static LayerTableRecord GetLayer(this Database database, Transaction transaction, string name)
        {
            if (database.HasLayer(transaction, name))
                return database.GetLayerTable(transaction).GetRecord(name);

            return null;
        }

        /// <summary>
        /// Gets <see cref="LayerTableRecord"/> of the layer with the given name from the database, if it exists.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <param name="layerName">Name of the desired layer.</param>
        /// <returns><see cref="LayerTableRecord"/> of the layer if it exists. Else null</returns>
        public static LayerTableRecord GetLayer(this Database database, Transaction transaction, LayerTableRecord layer)
        {
            if (database.HasLayer(transaction, layer.Name))
                return database.GetLayer(transaction, layer.Name);

            LayerTable layerTable = transaction.GetObject(database.LayerTableId, OpenMode.ForWrite) as LayerTable;
            LayerTableRecord layerClone = layer.Clone() as LayerTableRecord;
            layerTable.Add(layerClone);
            transaction.AddNewlyCreatedDBObject(layerClone, true);
            transaction.Commit();

            return database.GetLayer(transaction, layer);
        }

        #endregion

        #region Transacted Overloads

        public static BlockTableRecord GetModelSpace(this Database database) => database.Transact(GetModelSpace);

        public static bool HasLayer(this Database database, string name) => database.Transact(HasLayer, name);

        public static bool HasLayer(this Database database, LayerTableRecord layer) => database.Transact(HasLayer, layer);

        public static LayerTableRecord GetLayer(this Database database, string name) => database.Transact(GetLayer, name);

        public static LayerTableRecord GetLayer(this Database database, LayerTableRecord layer) => database.Transact(GetLayer, layer);

        #endregion

        #region Transaction Handlers

        public static void Transact(this Database database, Action<Database, Transaction> action)
        {
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                action(database, transaction);
                transaction.Commit();
            }
        }

        public static TResult Transact<TResult>(this Database database, Func<Database, Transaction, TResult> function)
        {
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                return function(database, transaction);
            }
        }

        public static void Transact<TArgument>(this Database database, Action<Database, Transaction, TArgument> action, TArgument value)
        {
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                action(database, transaction, value);
                transaction.Commit();
            }
        }

        public static TResult Transact<TArgument, TResult>(this Database database, Func<Database, Transaction, TArgument, TResult> function, TArgument value)
        {
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                return function(database, transaction, value);
            }
        }

        public static void Transact<TArgument1, TArgument2>(this Database database, Action<Database, Transaction, TArgument1, TArgument2> action, TArgument1 value1, TArgument2 value2)
        {
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                action(database, transaction, value1, value2);
                transaction.Commit();
            }
        }

        public static TResult Transact<TArgument1, TArgument2, TResult>(this Database database, Func<Database, Transaction, TArgument1, TArgument2, TResult> function, TArgument1 value1, TArgument2 value2)
        {
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                return function(database, transaction, value1, value2);
            }
        }

        #endregion
    }
}

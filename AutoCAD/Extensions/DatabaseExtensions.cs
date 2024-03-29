﻿using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD
{
    public static class DatabaseExtensions
    {
        #region Transactionless Public Extension Methods

        /// <summary>
        /// Gets the drawing custom properties.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <returns>A strongly typed dictionary containing the entries.</returns>
        public static Dictionary<string, string> GetAllCustomProperties(this Database database)
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
        public static void SetCustomProperties(this Database database, Dictionary<string, string> properties) => properties?.ForEach(prop => database.SetCustomProperty(prop));

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

        /// <summary>
        /// Sets a property value
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <param name="pair">Property key/value pair.</param>
        public static void SetCustomProperty(this Database database, KeyValuePair<string, string> pair) => database.SetCustomProperty(pair.Key, pair.Value);

        public static void RemoveCustomProperty(this Database database, string key)
        {
            DatabaseSummaryInfoBuilder infoBuilder = new DatabaseSummaryInfoBuilder(database.SummaryInfo);
            infoBuilder.CustomPropertyTable.Remove(key);
            database.SummaryInfo = infoBuilder.ToDatabaseSummaryInfo();
        }

        public static void RemoveCustomProperties(this Database database, IEnumerable<string> keys)
        {
            DatabaseSummaryInfoBuilder infoBuilder = new DatabaseSummaryInfoBuilder(database.SummaryInfo);
            keys.ForEach(key => infoBuilder.CustomPropertyTable.Remove(key));
            database.SummaryInfo = infoBuilder.ToDatabaseSummaryInfo();
        }

        public static void RemoveAllCustomProperties(this Database database)
        {
            DatabaseSummaryInfoBuilder infoBuilder = new DatabaseSummaryInfoBuilder(database.SummaryInfo);
            IDictionaryEnumerator dictEnum = database.SummaryInfo.CustomProperties;
            while (dictEnum.MoveNext())
            {
                infoBuilder.CustomPropertyTable.Remove(dictEnum.Key);
            }
            database.SummaryInfo = infoBuilder.ToDatabaseSummaryInfo();
        }

        //This overload of the function is required to save over the current file. bBakAndRename variable seems to enable creation of bak file, deletion of
        //original file and rename of .bak as .dwg. This fails with eFilerError if the original file's database is left undisposed in some other context.
        public static void SaveFile(this Database database) => database.SaveAs(database.Filename, true, DwgVersion.Current, database.SecurityParameters);

        #endregion

        #region Public Extension Methods

        /// <summary>
        /// Get the "Model Space" <see cref="BlockTableRecord"/> for the database.
        /// </summary>
        /// <param name="database">Instance for this method to be applied to.</param>
        /// <returns><see cref="BlockTableRecord"/> representing the model space of the database.</returns>
        public static BlockTableRecord GetModelSpace(this Database database, Transaction transaction) =>
            database.GetBlockTable(transaction).GetRecord(BlockTableRecord.ModelSpace);

        public static List<ObjectId> GetObjectIds(this Database database, Transaction transaction) =>
            database.GetModelSpace(transaction)
                    .Cast<ObjectId>()
                    .ToList();

        public static List<Entity> GetEntities(this Database database, Transaction transaction) =>
            database.GetObjectIds(transaction)
                    .Select(id => id.Open(transaction))
                    .Cast<Entity>()
                    .ToList();

        public static bool HasLayer(this Database database, Transaction transaction, string name) =>
            database.GetLayerTable(transaction)
                    .Has(name);

        public static bool HasLayer(this Database database, Transaction transaction, LayerTableRecord layer) =>
            database.HasLayer(transaction, layer.Name);

        /// <summary>
        /// Gets <see cref="LayerTableRecord"/> of the layer with the given name from the database, if it exists.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <param name="layerName">Name of the desired layer.</param>
        /// <returns><see cref="LayerTableRecord"/> of the layer if it exists. Else null</returns>
        public static LayerTableRecord GetLayer(this Database database, Transaction transaction, string name) =>
            database.HasLayer(transaction, name) ? database.GetLayerTable(transaction).GetRecord(name) : null;

        /// <summary>
        /// Gets <see cref="LayerTableRecord"/> of the layer with the given name from the database, if it exists.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <param name="layerName">Name of the desired layer.</param>
        /// <returns><see cref="LayerTableRecord"/> of the layer if it exists. Else null</returns>
        public static LayerTableRecord GetLayer(this Database database, Transaction transaction, LayerTableRecord layer)
        {
            if (layer is null)
                return null;

            if (database.HasLayer(transaction, layer.Name))
                return database.GetLayer(transaction, layer.Name);

            database.AddLayer(layer);
            return database.GetLayer(layer.Name);
        }

        public static List<LayerTableRecord> GetLayers(this Database database, Transaction transaction, List<LayerTableRecord> layers) =>
            layers.Select(layer => database.GetLayer(layer))
                  .ToList();

        public static void AddLayer(this Database database, Transaction transaction, LayerTableRecord layer)
        {
            LayerTableRecord layerClone = layer.Clone() as LayerTableRecord;
            LayerTable table = database.GetLayerTable(transaction).GetForWrite(transaction);
            table.Add(layerClone);
            transaction.AddNewlyCreatedDBObject(layerClone, true);
        }

        public static DBObject OpenHandleString(this Database database, Transaction transaction, string handleString) => database.GetObjectId(false, new Handle(Convert.ToInt64(handleString, 16)), 0)
                                                                                                                                 .Open(transaction);

        public static DBDictionary GetGroupDictionary(this Database database, Transaction transaction) => database.GroupDictionaryId.Open(transaction) as DBDictionary;

        public static void AddGroup(this Database database, Transaction transaction, string name, Group group)
        {
            database.GetGroupDictionary(transaction).GetForWrite(transaction).SetAt(name, group);
            transaction.AddNewlyCreatedDBObject(group, true);
        }

        public static DBDictionary GetNamedObjectDictionary(this Database database, Transaction transaction) => database.NamedObjectsDictionaryId.Open(transaction) as DBDictionary;

        #endregion

        #region Transacted Overloads

        public static BlockTableRecord GetModelSpace(this Database database) => database.Transact(GetModelSpace);

        public static List<ObjectId> GetObjectIds(this Database database) => database.Transact(GetObjectIds);

        public static List<Entity> GetEntities(this Database database) => database.Transact(GetEntities);

        public static bool HasLayer(this Database database, string name) => database.Transact(HasLayer, name);

        public static bool HasLayer(this Database database, LayerTableRecord layer) => database.Transact(HasLayer, layer);

        public static LayerTableRecord GetLayer(this Database database, string name) => database.Transact(GetLayer, name);

        public static LayerTableRecord GetLayer(this Database database, LayerTableRecord layer) => database.Transact(GetLayer, layer);

        public static List<LayerTableRecord> GetLayers(this Database database, List<LayerTableRecord> layers) => database.Transact(GetLayers, layers);

        public static void AddLayer(this Database database, LayerTableRecord layer) => database.Transact(AddLayer, layer);

        public static DBObject OpenHandleString(this Database database, string handleString) => database.Transact(OpenHandleString, handleString);

        public static DBDictionary GetGroupDictionary(this Database database) => database.Transact(GetGroupDictionary);

        public static void AddGroup(this Database database, string name, Group group) => database.Transact(AddGroup, name, group);

        public static DBDictionary GetNamedObjectDictionary(this Database database) => database.Transact(GetNamedObjectDictionary);

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

using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD
{
    public static class SymbolTableExtensions
    {
        #region Public Enums

        public enum SymbolTableType
        {
            BlockTable,
            DimensionStyleTable,
            LayerTable,
            LinetypeTable,
            RegisteredApplicationTable,
            TextStyleTable,
            UCSTable
        }

        #endregion

        #region Generic Methods

        /// <summary>
        /// Gets one of the <see cref="SymbolTable"/>s for the database. 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="symbolTableId"></param>
        /// <returns></returns>
        public static SymbolTable GetSymbolTable(this Database database, Transaction transaction, SymbolTableType type)
        {
            ObjectId symbolTableId;
            switch (type)
            {
                case SymbolTableType.BlockTable:
                    symbolTableId = database.BlockTableId;
                    break;
                case SymbolTableType.DimensionStyleTable:
                    symbolTableId = database.DimStyleTableId;
                    break;
                case SymbolTableType.LayerTable:
                    symbolTableId = database.LayerTableId;
                    break;
                case SymbolTableType.LinetypeTable:
                    symbolTableId = database.LinetypeTableId;
                    break;
                case SymbolTableType.RegisteredApplicationTable:
                    symbolTableId = database.RegAppTableId;
                    break;
                case SymbolTableType.TextStyleTable:
                    symbolTableId = database.TextStyleTableId;
                    break;
                case SymbolTableType.UCSTable:
                    symbolTableId = database.UcsTableId;
                    break;
                default:
                    return null;
            }
            
            return transaction.GetObject(symbolTableId, OpenMode.ForRead) as SymbolTable;
        }

        public static SymbolTableRecord GetRecord(this SymbolTable symbolTable, Transaction transaction, string name) => transaction.GetObject(symbolTable[name], OpenMode.ForRead) as SymbolTableRecord;

        public static SymbolTableRecord GetRecord(this SymbolTable symbolTable, Transaction transaction, ObjectId id) => transaction.GetObject(id, OpenMode.ForRead) as SymbolTableRecord;

        public static IEnumerable<SymbolTableRecord> GetRecords(this SymbolTable symbolTable, Transaction transaction) =>((IEnumerable<ObjectId>)symbolTable).Select(id => symbolTable.GetRecord(transaction, id));

        public static bool Has(this SymbolTable symbolTable, SymbolTableRecord record) => symbolTable.Has(record.Name);

        #endregion

        #region Specific Implementations

        #region GetTable

        /// <summary>
        /// Get the <see cref="BlockTable"/> for the database.
        /// </summary>
        /// <param name="database">Database instance this method is applied to.</param>
        /// <returns>A <see cref="BlockTable"/> opened for read.</returns>
        public static BlockTable GetBlockTable(this Database database, Transaction transaction) => database.GetSymbolTable(transaction, SymbolTableType.BlockTable) as BlockTable;

        /// <summary>
        /// Get the <see cref="DimStyleTable"/> for the database.
        /// </summary>
        /// <param name="database">Database instance this method is applied to.</param>
        /// <returns>A <see cref="DimStyleTable"/> opened for read.</returns>
        public static DimStyleTable GetDimensionStyleTable(this Database database, Transaction transaction) => database.GetSymbolTable(transaction, SymbolTableType.DimensionStyleTable) as DimStyleTable;

        /// <summary>
        /// Get the <see cref="LayerTable"/> for the database.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <returns>A <see cref="LayerTable"/> opened for read.</returns>
        public static LayerTable GetLayerTable(this Database database, Transaction transaction) => database.GetSymbolTable(transaction, SymbolTableType.LayerTable) as LayerTable;

        /// <summary>
        /// Get the <see cref="LinetypeTable"/> for the database.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <returns>A <see cref="LinetypeTable"/> opened for read.</returns>
        public static LinetypeTable GetLinetypeTable(this Database database, Transaction transaction) => database.GetSymbolTable(transaction, SymbolTableType.LinetypeTable) as LinetypeTable;

        /// <summary>
        /// Get the <see cref="RegAppTable"/> for the database.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <returns>A <see cref="RegAppTable"/> opened for read.</returns>
        public static RegAppTable GetRegisteredApplicationTable(this Database database, Transaction transaction) => database.GetSymbolTable(transaction, SymbolTableType.RegisteredApplicationTable) as RegAppTable;

        /// <summary>
        /// Get the <see cref="TextStyleTable"/> for the database.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <returns>A <see cref="TextStyleTable"/> opened for read.</returns>
        public static TextStyleTable GetTextStyleTable(this Database database, Transaction transaction) => database.GetSymbolTable(transaction, SymbolTableType.TextStyleTable) as TextStyleTable; 

        /// <summary>
        /// Get the <see cref="UcsTable"/> for the database.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <returns>A <see cref="UcsTable"/> opened for read.</returns>
        public static UcsTable GetUCSTable(this Database database, Transaction transaction) => 
            database.GetSymbolTable(transaction, SymbolTableType.UCSTable) as UcsTable;

        #endregion

        #region GetRecord

        /// <summary>
        /// Gets a <see cref="BlockTableRecord"/> with the given name from the table.
        /// </summary>
        /// <param name="blockTable">Instance this method applies to.</param>
        /// <param name="name">Name of the record to find.</param>
        /// <returns><see cref="BlockTableRecord"/> if it exists.</returns>
        public static BlockTableRecord GetRecord(this BlockTable blockTable, string name) => 
            GetRecord((SymbolTable)blockTable, name) as BlockTableRecord;

        /// <summary>
        /// Gets a <see cref="DimStyleTableRecord"/> with the given name from the table.
        /// </summary>
        /// <param name="dimStyleTable">Instance this method applies to.</param>
        /// <param name="name">Name of the record to find.</param>
        /// <returns><see cref="DimStyleTableRecord"/> if it exists.</returns>
        public static DimStyleTableRecord GetRecord(this DimStyleTable dimStyleTable, string name) => 
            GetRecord((SymbolTable)dimStyleTable, name) as DimStyleTableRecord;

        /// <summary>
        /// Gets a <see cref="LayerTableRecord"/> with the given name from the table.
        /// </summary>
        /// <param name="layerTable">Instance this method applies to.</param>
        /// <param name="name">Name of the record to find.</param>
        /// <returns><see cref="LayerTableRecord"/> if it exists.</returns>
        public static LayerTableRecord GetRecord(this LayerTable layerTable, string name) =>
            GetRecord((SymbolTable)layerTable, name) as LayerTableRecord;

        /// <summary>
        /// Gets a <see cref="LinetypeTableRecord"/> with the given name from the table.
        /// </summary>
        /// <param name="linetypeTable">Instance this method applies to.</param>
        /// <param name="name">Name of the record to find.</param>
        /// <returns><see cref="LinetypeTableRecord"/> if it exists.</returns>
        public static LinetypeTableRecord GetRecord(this LinetypeTable linetypeTable, string name) =>
            GetRecord((SymbolTable)linetypeTable, name) as LinetypeTableRecord;

        /// <summary>
        /// Gets a <see cref="RegAppTableRecord"/> with the given name from the table.
        /// </summary>
        /// <param name="regAppTable">Instance this method applies to.</param>
        /// <param name="name">Name of the record to find.</param>
        /// <returns><see cref="RegAppTableRecord"/> if it exists.</returns>
        public static RegAppTableRecord GetRecord(this RegAppTable regAppTable, string name) =>
            GetRecord((SymbolTable)regAppTable, name) as RegAppTableRecord;

        /// <summary>
        /// Gets a <see cref="TextStyleTableRecord"/> with the given name from the table.
        /// </summary>
        /// <param name="textStyleTable">Instance this method applies to.</param>
        /// <param name="name">Name of the record to find.</param>
        /// <returns><see cref="TextStyleTableRecord"/> if it exists.</returns>
        public static TextStyleTableRecord GetRecord(this TextStyleTable textStyleTable, string name) =>
            GetRecord((SymbolTable)textStyleTable, name) as TextStyleTableRecord;

        /// <summary>
        /// Gets a <see cref="UcsTableRecord"/> with the given name from the table.
        /// </summary>
        /// <param name="ucsTable">Instance this method applies to.</param>
        /// <param name="name">Name of the record to find.</param>
        /// <returns><see cref="UcsTableRecord"/> if it exists.</returns>
        public static UcsTableRecord GetRecord(this UcsTable ucsTable, string name) => 
            GetRecord((SymbolTable)ucsTable, name) as UcsTableRecord;

        #endregion

        #region GetRecords

        public static IEnumerable<BlockTableRecord> GetRecords(this BlockTable blockTable) => 
            GetRecords((SymbolTable)blockTable).Cast<BlockTableRecord>();

        public static IEnumerable<DimStyleTableRecord> GetRecords(this DimStyleTable dimStyleTable) => 
            GetRecords((SymbolTable)dimStyleTable).Cast<DimStyleTableRecord>();

        public static IEnumerable<LayerTableRecord> GetRecords(this LayerTable layerTable) => 
            GetRecords((SymbolTable)layerTable).Cast<LayerTableRecord>();

        public static IEnumerable<LinetypeTableRecord> GetRecords(this LinetypeTable linetypeTable) => 
            GetRecords((SymbolTable)linetypeTable).Cast<LinetypeTableRecord>();

        public static IEnumerable<RegAppTableRecord> GetRecords(this RegAppTable regAppTable) => 
            GetRecords((SymbolTable)regAppTable).Cast<RegAppTableRecord>();

        public static IEnumerable<TextStyleTableRecord> GetRecords(this TextStyleTable textStyleTable) => 
            GetRecords((SymbolTable)textStyleTable).Cast<TextStyleTableRecord>();

        public static IEnumerable<UcsTableRecord> GetRecords(this UcsTable ucsTable) => 
            GetRecords((SymbolTable)ucsTable).Cast<UcsTableRecord>();

        #endregion

        #endregion

        #region Transacted Overloads

        public static SymbolTableRecord GetRecord(this SymbolTable symbolTable, string name) => symbolTable.Transact(GetRecord, name);

        public static SymbolTableRecord GetRecord(this SymbolTable symbolTable, ObjectId id) => symbolTable.Transact(GetRecord, id);

        public static IEnumerable<SymbolTableRecord> GetRecords(this SymbolTable symbolTable) => symbolTable.Transact(GetRecords);

        /// <summary>
        /// Get the <see cref="BlockTable"/> for the database.
        /// </summary>
        /// <param name="database">Database instance this method is applied to.</param>
        /// <returns>A <see cref="BlockTable"/> opened for read.</returns>
        public static BlockTable GetBlockTable(this Database database) => database.Transact(GetBlockTable);

        /// <summary>
        /// Get the <see cref="DimStyleTable"/> for the database.
        /// </summary>
        /// <param name="database">Database instance this method is applied to.</param>
        /// <returns>A <see cref="DimStyleTable"/> opened for read.</returns>
        public static DimStyleTable GetDimensionStyleTable(this Database database) => database.Transact(GetDimensionStyleTable);

        /// <summary>
        /// Get the <see cref="LayerTable"/> for the database.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <returns>A <see cref="LayerTable"/> opened for read.</returns>
        public static LayerTable GetLayerTable(this Database database) => database.Transact(GetLayerTable);

        /// <summary>
        /// Get the <see cref="LinetypeTable"/> for the database.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <returns>A <see cref="LinetypeTable"/> opened for read.</returns>
        public static LinetypeTable GetLinetypeTable(this Database database) => database.Transact(GetLinetypeTable);

        /// <summary>
        /// Get the <see cref="RegAppTable"/> for the database.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <returns>A <see cref="RegAppTable"/> opened for read.</returns>
        public static RegAppTable GetRegisteredApplicationTable(this Database database) => database.Transact(GetRegisteredApplicationTable);

        /// <summary>
        /// Get the <see cref="TextStyleTable"/> for the database.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <returns>A <see cref="TextStyleTable"/> opened for read.</returns>
        public static TextStyleTable GetTextStyleTable(this Database database) => database.Transact(GetTextStyleTable);

        /// <summary>
        /// Get the <see cref="UcsTable"/> for the database.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <returns>A <see cref="UcsTable"/> opened for read.</returns>
        public static UcsTable GetUCSTable(this Database database) => database.Transact(GetUCSTable);

        #endregion
    }
}

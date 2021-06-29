using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace ICA.AutoCAD
{
    public static class SymbolTables
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
        public static SymbolTable GetSymbolTable(this Database database, SymbolTableType type)
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

            using (Transaction transaction = database.TransactionManager.StartTransaction())
                return transaction.GetObject(symbolTableId, OpenMode.ForRead) as SymbolTable;
        }

        public static bool Contains(this SymbolTable symbolTable, string name)
        {
            return symbolTable[name] != null;
        }

        public static SymbolTableRecord GetRecord(this SymbolTable symbolTable, string name)
        {
            using (Transaction transaction = symbolTable.Database.TransactionManager.StartTransaction())
                return transaction.GetObject(symbolTable[name], OpenMode.ForRead) as SymbolTableRecord;
        }

        public static SymbolTableRecord GetRecord(this SymbolTable symbolTable, ObjectId id)
        {
            using (Transaction transaction = symbolTable.Database.TransactionManager.StartTransaction())
                return transaction.GetObject(id, OpenMode.ForRead) as SymbolTableRecord;
        }

        public static List<string> GetRecordNames(this SymbolTable symbolTable)
        {
            List<string> result = new List<string>();
            foreach (ObjectId id in symbolTable)
                result.Add(symbolTable.GetRecord(id).Name);
            return result;
        }

        #endregion

        #region Specific Implementations

        /// <summary>
        /// Get the <see cref="BlockTable"/> for the database.
        /// </summary>
        /// <param name="database">Database instance this method is applied to.</param>
        /// <returns>A <see cref="BlockTable"/> opened for read.</returns>
        public static BlockTable GetBlockTable(this Database database)
        {
            return database.GetSymbolTable(SymbolTableType.BlockTable) as BlockTable;
        }

        /// <summary>
        /// Get the <see cref="DimStyleTable"/> for the database.
        /// </summary>
        /// <param name="database">Database instance this method is applied to.</param>
        /// <returns>A <see cref="DimStyleTable"/> opened for read.</returns>
        public static DimStyleTable GetDimensionStyleTable(this Database database)
        {
            return database.GetSymbolTable(SymbolTableType.DimensionStyleTable) as DimStyleTable;
        }

        /// <summary>
        /// Get the <see cref="LayerTable"/> for the database.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <returns>A <see cref="LayerTable"/> opened for read.</returns>
        public static LayerTable GetLayerTable(this Database database)
        {
            return database.GetSymbolTable(SymbolTableType.LayerTable) as LayerTable;
        }

        /// <summary>
        /// Get the <see cref="LinetypeTable"/> for the database.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <returns>A <see cref="LinetypeTable"/> opened for read.</returns>
        public static LinetypeTable GetLinetypeTable(this Database database)
        {
            return database.GetSymbolTable(SymbolTableType.LinetypeTable) as LinetypeTable;
        }

        /// <summary>
        /// Get the <see cref="RegAppTable"/> for the database.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <returns>A <see cref="RegAppTable"/> opened for read.</returns>
        public static RegAppTable GetRegisteredApplicationTable(this Database database)
        {
            return database.GetSymbolTable(SymbolTableType.RegisteredApplicationTable) as RegAppTable;
        }

        /// <summary>
        /// Get the <see cref="TextStyleTable"/> for the database.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <returns>A <see cref="TextStyleTable"/> opened for read.</returns>
        public static TextStyleTable GetTextStyleTable(this Database database)
        {
            return database.GetSymbolTable(SymbolTableType.TextStyleTable) as TextStyleTable;
        }

        /// <summary>
        /// Get the <see cref="UcsTable"/> for the database.
        /// </summary>
        /// <param name="database">Instance this method applies to.</param>
        /// <returns>A <see cref="UcsTable"/> opened for read.</returns>
        public static UcsTable GetUCSTable(this Database database)
        {
            return database.GetSymbolTable(SymbolTableType.UCSTable) as UcsTable;
        }

        /// <summary>
        /// Gets a <see cref="BlockTableRecord"/> with the given name from the table.
        /// </summary>
        /// <param name="blockTable">Instance this method applies to.</param>
        /// <param name="name">Name of the record to find.</param>
        /// <returns><see cref="BlockTableRecord"/> if it exists.</returns>
        public static BlockTableRecord GetRecord (this BlockTable blockTable, string name)
        {
            return GetRecord((SymbolTable)blockTable, name) as BlockTableRecord;
        }

        /// <summary>
        /// Gets a <see cref="DimStyleTableRecord"/> with the given name from the table.
        /// </summary>
        /// <param name="dimStyleTable">Instance this method applies to.</param>
        /// <param name="name">Name of the record to find.</param>
        /// <returns><see cref="DimStyleTableRecord"/> if it exists.</returns>
        public static DimStyleTableRecord GetRecord(this DimStyleTable dimStyleTable, string name)
        {
            return GetRecord((SymbolTable)dimStyleTable, name) as DimStyleTableRecord;
        }

        /// <summary>
        /// Gets a <see cref="LayerTableRecord"/> with the given name from the table.
        /// </summary>
        /// <param name="layerTable">Instance this method applies to.</param>
        /// <param name="name">Name of the record to find.</param>
        /// <returns><see cref="LayerTableRecord"/> if it exists.</returns>
        public static LayerTableRecord GetRecord(this LayerTable layerTable, string name)
        {
            return GetRecord((SymbolTable)layerTable, name) as LayerTableRecord;
        }

        /// <summary>
        /// Gets a <see cref="LinetypeTableRecord"/> with the given name from the table.
        /// </summary>
        /// <param name="linetypeTable">Instance this method applies to.</param>
        /// <param name="name">Name of the record to find.</param>
        /// <returns><see cref="LinetypeTableRecord"/> if it exists.</returns>
        public static LinetypeTableRecord GetRecord(this LinetypeTable linetypeTable, string name)
        {
            return GetRecord((SymbolTable)linetypeTable, name) as LinetypeTableRecord;
        }

        /// <summary>
        /// Gets a <see cref="RegAppTableRecord"/> with the given name from the table.
        /// </summary>
        /// <param name="regAppTable">Instance this method applies to.</param>
        /// <param name="name">Name of the record to find.</param>
        /// <returns><see cref="RegAppTableRecord"/> if it exists.</returns>
        public static RegAppTableRecord GetRecord(this RegAppTable regAppTable, string name)
        {
            return GetRecord((SymbolTable)regAppTable, name) as RegAppTableRecord;
        }

        /// <summary>
        /// Gets a <see cref="TextStyleTableRecord"/> with the given name from the table.
        /// </summary>
        /// <param name="textStyleTable">Instance this method applies to.</param>
        /// <param name="name">Name of the record to find.</param>
        /// <returns><see cref="TextStyleTableRecord"/> if it exists.</returns>
        public static TextStyleTableRecord GetRecord(this TextStyleTable textStyleTable, string name)
        {
            return GetRecord((SymbolTable)textStyleTable, name) as TextStyleTableRecord;
        }

        /// <summary>
        /// Gets a <see cref="UcsTableRecord"/> with the given name from the table.
        /// </summary>
        /// <param name="ucsTable">Instance this method applies to.</param>
        /// <param name="name">Name of the record to find.</param>
        /// <returns><see cref="UcsTableRecord"/> if it exists.</returns>
        public static UcsTableRecord GetRecord(this UcsTable ucsTable, string name)
        {
            return GetRecord((SymbolTable)ucsTable, name) as UcsTableRecord;
        }

        #endregion
    }
}

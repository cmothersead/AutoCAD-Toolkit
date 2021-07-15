using Autodesk.AutoCAD.DatabaseServices;
using System.IO;

namespace ICA.AutoCAD
{
    public static class BlockTableExtensions
    {
        #region Public Extension Methods

        public static BlockTableRecord LoadExternalBlockTableRecord (this BlockTable blockTable, Transaction transaction, string filePath)
        {
            if (filePath is null)
                return null;

            string fileName = Path.GetFileNameWithoutExtension(filePath);
            Database tempDatabase = new Database(false, true);
            tempDatabase.ReadDwgFile(filePath, FileShare.Read, true, null);
            blockTable.Database.Insert(fileName, tempDatabase, true);
            return transaction.GetObject(blockTable[fileName], OpenMode.ForRead) as BlockTableRecord;
        }

        #endregion

        #region Transacted Overloads

        public static BlockTableRecord LoadExternalBlockTableRecord(this BlockTable blockTable, string filePath) => blockTable.Transact(LoadExternalBlockTableRecord, filePath);

        #endregion
    }
}

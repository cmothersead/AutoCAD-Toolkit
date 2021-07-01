using Autodesk.AutoCAD.DatabaseServices;
using System.IO;

namespace ICA.AutoCAD
{
    public static class BlockTableExtensions
    {
        public static BlockTableRecord LoadExternalBlockTableRecord (this BlockTable blockTable, string filePath)
        {
            if (filePath is null)
                return null;

            string fileName = Path.GetFileNameWithoutExtension(filePath);
            BlockTableRecord record;
            using (Transaction transaction = blockTable.Database.TransactionManager.StartTransaction())
            {
                Database tempDatabase = new Database(false, true);
                tempDatabase.ReadDwgFile(filePath, FileShare.Read, true, null);
                blockTable.Database.Insert(fileName, tempDatabase, true);
                record = transaction.GetObject(blockTable[fileName], OpenMode.ForRead) as BlockTableRecord;
                transaction.Commit();
            }
            return record;
        }
    }
}

using Autodesk.AutoCAD.DatabaseServices;
using System;

namespace ICA.AutoCAD
{
    public static class DBObjectExtensions
    {
        public static void Erase(this DBObject obj, Transaction transaction)
        {
            transaction.GetObject(obj.ObjectId, OpenMode.ForWrite).Erase();
        }

        public static void Transact(this DBObject obj, Action<DBObject, Transaction> action)
        {
            using (Transaction transaction = obj.Database.TransactionManager.StartTransaction())
            {
                action(obj, transaction);
                transaction.Commit();
            }
        }
    }
}

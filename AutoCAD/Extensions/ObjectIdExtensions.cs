using Autodesk.AutoCAD.DatabaseServices;
using System;

namespace ICA.AutoCAD
{
    public static class ObjectIdExtensions
    {
        #region Public Extension Methods

        public static DBObject Open(this ObjectId id, Transaction transaction) => transaction.GetObject(id, OpenMode.ForRead);

        public static void Erase(this ObjectId id, Transaction transaction) => transaction.GetObject(id, OpenMode.ForWrite).Erase();

        #endregion

        #region Transacted Overloads

        public static DBObject Open(this ObjectId id) => id.Transact(Open);

        public static void Erase(this ObjectId id) => id.Transact(Erase);

        #endregion

        #region Transaction Handlers

        public static void Transact(this ObjectId id, Action<ObjectId> action)
        {
            using (Transaction transaction = id.Database.TransactionManager.StartTransaction())
            {
                action(id);
                transaction.Commit();
            }
        }

        public static TResult Transact<TResult>(this ObjectId id, Func<ObjectId, TResult> function)
        {
            using (Transaction transaction = id.Database.TransactionManager.StartTransaction())
            {
                return function(id);
            }
        }

        #endregion
    }
}

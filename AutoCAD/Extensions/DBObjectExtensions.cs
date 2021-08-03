using Autodesk.AutoCAD.DatabaseServices;
using System;

namespace ICA.AutoCAD
{
    public static class DBObjectExtensions
    {
        #region Public Extension Methods

        public static TDBObject GetForWrite<TDBObject>(this TDBObject obj, Transaction transaction) where TDBObject : DBObject => transaction.GetObject(obj.ObjectId, OpenMode.ForWrite) as TDBObject;

        public static void Erase(this DBObject obj, Transaction transaction) => obj.GetForWrite(transaction).Erase();

        #endregion

        #region Transaction Handlers

        public static void Transact<TDBObject>(this TDBObject obj, Action<TDBObject, Transaction> action) where TDBObject: DBObject
        {
            using (Transaction transaction = obj.Database.TransactionManager.StartTransaction())
            {
                action(obj, transaction);
                transaction.Commit();
            }
        }

        public static TResult Transact<TDBObject, TResult>(this TDBObject obj, Func<TDBObject, Transaction, TResult> function) where TDBObject : DBObject
        {
            using (Transaction transaction = obj.Database.TransactionManager.StartTransaction())
            {
                TResult result = function(obj, transaction);
                transaction.Commit();
                return result;
            }
        }

        public static void Transact<TDBObject, TArgument>(this TDBObject obj, Action<TDBObject, Transaction, TArgument> action, TArgument value) where TDBObject: DBObject
        {
            if(obj.Database != null)
            {
                using (Transaction transaction = obj.Database.TransactionManager.StartTransaction())
                {
                    action(obj, transaction, value);
                    transaction.Commit();
                }
            }
            else if(value is Database database)
            {
                using (Transaction transaction = database.TransactionManager.StartTransaction())
                {
                    action(obj, transaction, value);
                    transaction.Commit();
                }
            }
            else
            {
                throw new ArgumentException("DBObject is not yet a database resident, and no database was provided");
            }
        }

        public static TResult Transact<TDBObject, TArgument, TResult>(this TDBObject obj, Func<TDBObject, Transaction, TArgument, TResult> function, TArgument value) where TDBObject : DBObject
        {
            using (Transaction transaction = obj.Database.TransactionManager.StartTransaction())
            {
                TResult result = function(obj, transaction, value);
                transaction.Commit();
                return result;
            }
        }

        public static void Transact<TDBObject, TArgument1, TArgument2>(this TDBObject obj, Action<TDBObject, Transaction, TArgument1, TArgument2> action, TArgument1 value1, TArgument2 value2) where TDBObject : DBObject
        {
            using (Transaction transaction = obj.Database.TransactionManager.StartTransaction())
            {
                action(obj, transaction, value1, value2);
                transaction.Commit();
            }
        }

        public static TResult Transact<TDBObject, TArgument1, TArgument2, TResult>(this TDBObject obj, Func<TDBObject, Transaction, TArgument1, TArgument2, TResult> function, TArgument1 value1, TArgument2 value2) where TDBObject : DBObject
        {
            using (Transaction transaction = obj.Database.TransactionManager.StartTransaction())
            {
                TResult result = function(obj, transaction, value1, value2);
                transaction.Commit();
                return result;
            }
        }

        #endregion
    }
}

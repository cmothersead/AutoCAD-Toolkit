using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD
{
    public static class DBObjectExtensions
    {
        #region Public Extension Methods

        public static TDBObject GetForWrite<TDBObject>(this TDBObject obj, Transaction transaction) where TDBObject : DBObject => transaction.GetObject(obj.ObjectId, OpenMode.ForWrite) as TDBObject;

        public static void EraseObject(this DBObject obj, Transaction transaction) => obj.GetForWrite(transaction).Erase();

        public static bool HasGroup(this DBObject obj, Transaction transaction) => obj.GetPersistentReactorIds().Cast<ObjectId>().Any(id => id.Open(transaction) is Group);

        public static List<Group> GetGroups(this DBObject obj, Transaction transaction) => obj.GetPersistentReactorIds()
                                                                                              .Cast<ObjectId>()
                                                                                              .Select(id => id.Open(transaction))
                                                                                              .OfType<Group>()
                                                                                              .ToList();

        #endregion

        #region Transacted Overloads

        public static void EraseObject(this DBObject obj) => obj.Transact(EraseObject);

        public static bool HasGroup(this DBObject obj) => obj.Transact(HasGroup);

        public static List<Group> GetGroups(this DBObject obj) => obj.Transact(GetGroups);

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
                    return;
                }
            }
            
            if(value is Database database)
            {
                using (Transaction transaction = database.TransactionManager.StartTransaction())
                {
                    action(obj, transaction, value);
                    transaction.Commit();
                    return;
                }
            }

            throw new ArgumentException("DBObject is not yet a database resident, and no database argument was provided");
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
            if(obj.Database != null)
            {
                using (Transaction transaction = obj.Database.TransactionManager.StartTransaction())
                {
                    action(obj, transaction, value1, value2);
                    transaction.Commit();
                    return;
                }
            }
            
            if(value1 is Database database1)
            {
                using (Transaction transaction = database1.TransactionManager.StartTransaction())
                {
                    action(obj, transaction, value1, value2);
                    transaction.Commit();
                    return;
                }
            }
            
            if(value2 is Database database2){
                using (Transaction transaction = database2.TransactionManager.StartTransaction())
                {
                    action(obj, transaction, value1, value2);
                    transaction.Commit();
                    return;
                }
            }

            throw new ArgumentException("DBObject is not yet a database resident, and no database argument was provided");
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

        public static void Transact<TDBObject, TArgument1, TArgument2, TArgument3>(this TDBObject obj, Action<TDBObject, Transaction, TArgument1, TArgument2, TArgument3> action, TArgument1 value1, TArgument2 value2, TArgument3 value3) where TDBObject : DBObject
        {
            using (Transaction transaction = obj.Database.TransactionManager.StartTransaction())
            {
                action(obj, transaction, value1, value2, value3);
                transaction.Commit();
            }
        }

        public static TResult Transact<TDBObject, TArgument1, TArgument2, TArgument3, TResult>(this TDBObject obj, Func<TDBObject, Transaction, TArgument1, TArgument2, TArgument3, TResult> function, TArgument1 value1, TArgument2 value2, TArgument3 value3) where TDBObject : DBObject
        {
            using (Transaction transaction = obj.Database.TransactionManager.StartTransaction())
            {
                TResult result = function(obj, transaction, value1, value2, value3);
                transaction.Commit();
                return result;
            }
        }

        #endregion
    }
}

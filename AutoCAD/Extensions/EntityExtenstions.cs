using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;

namespace ICA.AutoCAD
{
    public static class EntityExtenstions
    {
        /// <summary>
        /// Adds entity to the current document's database within self-contained transaction.
        /// </summary>
        /// <param name="entity"></param>
        public static void Insert(this Entity entity) => entity.Insert(Application.DocumentManager.MdiActiveDocument.Database);

        /// <summary>
        /// Adds entity to the passed <see cref="Database"/> within self-contained transaction.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="database">Database to append the entity to</param>
        public static void Insert<TEntity>(this TEntity entity, Database database) where TEntity : Entity
        {
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                entity.Insert(transaction, database);
                transaction.Commit();
            }
        }

        /// <summary>
        /// Adds entity to the passed <see cref="Database"/> within the passed <see cref="Transaction"/>
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="database"></param>
        /// <param name="transaction"></param>
        public static void Insert(this Entity entity, Transaction transaction, Database database)
        {
            BlockTableRecord modelSpace = transaction.GetObject(database.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
            modelSpace.AppendEntity(entity);
            transaction.AddNewlyCreatedDBObject(entity, true);
        }

        public static void SetLayer(this Entity entity, Transaction transaction, LayerTableRecord layer)
        {
            if (!entity.Database.HasLayer(layer))
                entity.Database.AddLayer(layer);

            entity.GetForWrite(transaction).Layer = layer.Name;
        }

        public static void SetLayer(this Entity entity, LayerTableRecord layer) => entity.Transact(SetLayer, layer);
    }
}

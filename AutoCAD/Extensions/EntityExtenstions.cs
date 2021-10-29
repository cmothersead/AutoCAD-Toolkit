using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace ICA.AutoCAD
{
    public static class EntityExtenstions
    {
        #region Public Extension Methods

        /// <summary>
        /// Adds entity to the current document's database within self-contained transaction.
        /// </summary>
        /// <param name="entity"></param>
        public static void Insert(this Entity entity, LayerTableRecord layer = null) => entity.Insert(Application.DocumentManager.MdiActiveDocument.Database, layer);

        /// <summary>
        /// Adds entity to the passed <see cref="Database"/> within self-contained transaction.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="database">Database to append the entity to</param>
        public static void Insert(this Entity entity, Database database, LayerTableRecord layer = null)
        {
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                switch(entity)
                {
                    case BlockReference reference:
                        reference.Insert(transaction, database, layer);
                        break;
                    default:
                        entity.Insert(transaction, database, layer);
                        break;
                }
                transaction.Commit();
            }
        }

        /// <summary>
        /// Adds entity to the passed <see cref="Database"/> within the passed <see cref="Transaction"/>
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="database"></param>
        /// <param name="transaction"></param>
        public static void Insert(this Entity entity, Transaction transaction, Database database, LayerTableRecord layer = null)
        {
            BlockTableRecord modelSpace = transaction.GetObject(database.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
            modelSpace.AppendEntity(entity);
            transaction.AddNewlyCreatedDBObject(entity, true);
            entity.SetLayer(layer);
        }

        public static void SetLayer(this Entity entity, Transaction transaction, LayerTableRecord layer)
        {
            if (layer is null)
                return;

            if (!entity.Database.HasLayer(layer))
                entity.Database.AddLayer(layer);

            entity.GetForWrite(transaction).Layer = layer.Name;
        }

        public static bool SetLayer(this Entity entity, Transaction transaction, string name)
        {
            if (!entity.Database.HasLayer(name))
                return false;

            entity.GetForWrite(transaction).Layer = name;
            return true;
        }

        public static bool IntersectsWith(this Entity entity, Entity otherEntity)
        {
            Point3dCollection collection = new Point3dCollection();
            entity.IntersectWith(otherEntity, Intersect.OnBothOperands, collection, IntPtr.Zero, IntPtr.Zero);
            return collection.Count != 0;
        }

        #endregion

        #region Transacted Overloads

        public static void SetLayer(this Entity entity, LayerTableRecord layer) => entity.Transact(SetLayer, layer);

        public static void SetLayer(this Entity entity, string name) => entity.Transact(SetLayer, name);

        public static void SetColor(this Entity entity, Transaction transaction, Color color) => entity.GetForWrite(transaction).Color = color;

        public static void SetColor(this Entity entity, Color color) => entity.Transact(SetColor, color);

        public static bool IntersectsWith(this Entity entity, Point2d point) => entity.IntersectsWith(new DBPoint(point.ToPoint3d()));

        #endregion
    }
}

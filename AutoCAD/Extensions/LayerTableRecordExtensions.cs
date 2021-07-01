using Autodesk.AutoCAD.DatabaseServices;

namespace ICA.AutoCAD
{
    public static class LayerTableRecordExtensions
    {
        public static bool Lock(this LayerTableRecord layer)
        {
            try
            {
                using(Transaction transaction = layer.Database.TransactionManager.StartTransaction())
                {
                    LayerTableRecord layerForWrite = transaction.GetObject(layer.ObjectId, OpenMode.ForWrite) as LayerTableRecord;
                    layerForWrite.IsLocked = true;
                    transaction.Commit();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool Unlock(this LayerTableRecord layer)
        {
            try
            {
                using (Transaction transaction = layer.Database.TransactionManager.StartTransaction())
                {
                    LayerTableRecord layerForWrite = transaction.GetObject(layer.ObjectId, OpenMode.ForWrite) as LayerTableRecord;
                    layerForWrite.IsLocked = false;
                    transaction.Commit();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates transaction to freeze this layer. Returns true if successful
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static bool Freeze(this LayerTableRecord layer)
        {
            try
            {
                using (Transaction transaction = layer.Database.TransactionManager.StartTransaction())
                {
                    LayerTableRecord layerForWrite = transaction.GetObject(layer.ObjectId, OpenMode.ForWrite) as LayerTableRecord;
                    layerForWrite.IsFrozen = true;
                    transaction.Commit();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates transaction to thaw this layer. Returns true if successful
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static bool Thaw(this LayerTableRecord layer)
        {
            try
            {
                using (Transaction transaction = layer.Database.TransactionManager.StartTransaction())
                {
                    LayerTableRecord layerForWrite = transaction.GetObject(layer.ObjectId, OpenMode.ForWrite) as LayerTableRecord;
                    layerForWrite.IsFrozen = false;
                    transaction.Commit();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static ObjectIdCollection GetEntities(this LayerTableRecord layer)
        {
            ObjectIdCollection collection = new ObjectIdCollection();

            using(Transaction transaction = layer.Database.TransactionManager.StartTransaction())
            {
                foreach(ObjectId id in layer.Database.GetModelSpace())
                {
                    if(((Entity)layer.Database.Open(id)).Layer == layer.Name)
                    {
                        collection.Add(id);
                    }
                }
            }

            return collection;
        }
    }
}

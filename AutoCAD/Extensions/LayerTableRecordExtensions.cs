using Autodesk.AutoCAD.DatabaseServices;

namespace ICA.AutoCAD
{
    public static class LayerTableRecordExtensions
    {
        #region Setters

        #region Methods

        public static void Lock(this LayerTableRecord layer, Transaction transaction) => layer.GetForWrite(transaction).IsLocked = true;

        public static void Unlock(this LayerTableRecord layer, Transaction transaction) => layer.GetForWrite(transaction).IsLocked = false;

        public static void Freeze(this LayerTableRecord layer, Transaction transaction) => layer.GetForWrite(transaction).IsFrozen = true;

        public static void Thaw(this LayerTableRecord layer, Transaction transaction) => layer.GetForWrite(transaction).IsFrozen = false;

        #endregion

        #region Transacted Overloads

        public static void Lock(this LayerTableRecord layer) => layer.Transact(Lock);

        public static void Unlock(this LayerTableRecord layer) => layer.Transact(Unlock);

        public static void Freeze(this LayerTableRecord layer) => layer.Transact(Freeze);

        public static void Thaw(this LayerTableRecord layer) => layer.Transact(Thaw);

        #endregion

        #endregion

        public static ObjectIdCollection GetEntities(this LayerTableRecord layer)
        {
            ObjectIdCollection collection = new ObjectIdCollection();

            using (Transaction transaction = layer.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in layer.Database.GetModelSpace())
                {
                    if (((Entity)id.Open()).Layer == layer.Name)
                    {
                        collection.Add(id);
                    }
                }
            }

            return collection;
        }
    }
}

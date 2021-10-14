using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD
{
    public static class LayerTableRecordExtensions
    {
        #region Public Extension Methods

        public static void Lock(this LayerTableRecord layer, Transaction transaction) => layer.GetForWrite(transaction).IsLocked = true;

        public static void Unlock(this LayerTableRecord layer, Transaction transaction) => layer.GetForWrite(transaction).IsLocked = false;

        public static void Freeze(this LayerTableRecord layer, Transaction transaction) => layer.GetForWrite(transaction).IsFrozen = true;

        public static void Thaw(this LayerTableRecord layer, Transaction transaction) => layer.GetForWrite(transaction).IsFrozen = false;

        public static ICollection<Entity> GetEntities(this LayerTableRecord layer, Transaction transaction) => layer.Database.GetEntities()
                                                                                                                             .Where(entity => entity.Layer == layer.Name)
                                                                                                                             .ToList();

        #endregion

        #region Transacted Overloads

        public static void Lock(this LayerTableRecord layer) => layer.Transact(Lock);

        public static void Unlock(this LayerTableRecord layer) => layer.Transact(Unlock);

        public static void Freeze(this LayerTableRecord layer) => layer.Transact(Freeze);

        public static void Thaw(this LayerTableRecord layer) => layer.Transact(Thaw);

        public static ICollection<Entity> GetEntities(this LayerTableRecord layer) => layer.Transact(GetEntities);

        #endregion
    }
}

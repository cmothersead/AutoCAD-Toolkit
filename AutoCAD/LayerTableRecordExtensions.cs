using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD
{
    public static class LayerTableRecordExtensions
    {
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
        /// Creates transaction to freeze this layer. Returns true if successful
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
    }
}

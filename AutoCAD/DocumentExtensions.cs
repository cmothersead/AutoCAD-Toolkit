using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace ICA.AutoCAD
{
    public static class DocumentExtensions
    {
        public static LayerTableRecord GetLayer(this Document document, string layerName)
        {
            try
            {
                using (Transaction transaction = document.TransactionManager.StartTransaction())
                {
                    LayerTable layerTable = transaction.GetObject(document.Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                    return transaction.GetObject(layerTable[layerName], OpenMode.ForWrite) as LayerTableRecord;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}

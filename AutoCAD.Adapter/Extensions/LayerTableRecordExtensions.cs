using Autodesk.AutoCAD.DatabaseServices;

namespace ICA.AutoCAD.Adapter
{
    public static class LayerTableRecordExtensions
    {
        public static void UnlockWithoutWarning(this LayerTableRecord layer) => layer.Transact(UnlockWithoutWarning);

        public static void UnlockWithoutWarning(this LayerTableRecord layer, Transaction transaction)
        {
            if (layer.IsErased)
                return;

            LayerTableRecord layerForWrite = layer.GetForWrite(transaction);
            layerForWrite.Modified -= ElectricalLayers.UnlockWarning;
            layerForWrite.IsLocked = false;
        }

        public static void LockWithWarning(this LayerTableRecord layer) => layer.Transact(LockWithWarning);

        public static void LockWithWarning(this LayerTableRecord layer, Transaction transaction)
        {
            if (layer.IsErased)
                return;

            LayerTableRecord layerForWrite = layer.GetForWrite(transaction);
            layerForWrite.IsLocked = true;
            layerForWrite.Modified += ElectricalLayers.UnlockWarning;
        }
    }
}

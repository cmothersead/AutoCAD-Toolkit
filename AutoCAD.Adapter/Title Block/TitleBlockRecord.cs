using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace ICA.AutoCAD.Adapter
{
    public class TitleBlockRecord
    {
        private BlockTableRecord _blockTableRecord;

        public ObjectId ObjectId => _blockTableRecord.ObjectId;
        public string Name => _blockTableRecord.Name;
        public BlockReference Reference => _blockTableRecord.GetBlockReferenceIds(true, false)[0].Open() as BlockReference;

        public TitleBlockRecord(BlockTableRecord record)
        {
            if (!record.Name.Contains("Title Block"))
                throw new ArgumentException($"\"{record.Name}\" is not a valid title block. Name must contain \"Title Block\".");

            if (!record.HasAttribute("TB"))
                throw new ArgumentException($"\"{record.Name}\" is not a valid title block. Block must contain an attribute called \"TB\".");

            _blockTableRecord = record;
        }

        public void Insert()
        {
            if (_blockTableRecord.GetBlockReferenceIds(true, false).Count > 0)
                return;

            new BlockReference(Point3d.Origin, _blockTableRecord.ObjectId) { Layer = _blockTableRecord.Database.GetLayer(ElectricalLayers.TitleBlockLayer).Name }.Insert();
            _blockTableRecord.Database.Limmax = Reference.GeometricExtents.MaxPoint.ToPoint2D();
            _blockTableRecord.Database.Limmin = Reference.GeometricExtents.MinPoint.ToPoint2D();
            GridDisplay.Limits = true;
        }

        public void Remove()
        {
            if (_blockTableRecord.GetBlockReferenceIds(true, false).Count == 0)
                return;

            using (Transaction transaction = _blockTableRecord.Database.TransactionManager.StartTransaction())
            {
                _blockTableRecord.Database.GetLayer(ElectricalLayers.TitleBlockLayer).Unlock();
                foreach (ObjectId id in _blockTableRecord.GetBlockReferenceIds(true, false))
                    id.Erase(transaction);
                _blockTableRecord.Database.GetLayer(ElectricalLayers.TitleBlockLayer).Lock();
                GridDisplay.Limits = false;
                transaction.Commit();
            }
        }

        public void Purge()
        {
            if (_blockTableRecord.GetBlockReferenceIds(true, false).Count != 0)
                Remove();

            using (Transaction transaction = _blockTableRecord.Database.TransactionManager.StartTransaction())
            {
                _blockTableRecord.Erase(transaction);
                transaction.Commit();
            }
        }
    }
}

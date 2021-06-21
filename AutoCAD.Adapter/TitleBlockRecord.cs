using Autodesk.AutoCAD.DatabaseServices;
using System;

namespace ICA.AutoCAD.Adapter
{
    public class TitleBlockRecord
    {
        private BlockTableRecord _blockTableRecord;

        public ObjectId ObjectId => _blockTableRecord.ObjectId;

        public TitleBlockRecord(BlockTableRecord record)
        {
            if (!record.Name.Contains("Title Block"))
                throw new ArgumentException($"\"{record.Name}\" is not a valid title block. Name must contain \"Title Block\".");

            if(!record.HasAttribute("TB"))
                throw new ArgumentException($"\"{record.Name}\" is not a valid title block. Block must contain an attribute called \"TB\".");

            _blockTableRecord = record;
        }

        //private BlockReference Reference
        //{
        //    get
        //    {
        //        _blockTableRecord.GetBlockReferenceIds(true, false);
        //    }
        //}

        //private TitleBlockProperties _ladderProperties;
        //public TitleBlockProperties LadderProperties
        //{
        //    get
        //    {
        //        if (_ladderProperties != null)
        //            return _ladderProperties;


        //    }
        //}
    }
}

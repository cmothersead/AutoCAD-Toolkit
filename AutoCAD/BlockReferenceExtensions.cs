using Autodesk.AutoCAD.DatabaseServices;

namespace ICA.AutoCAD
{
    public static class BlockReferenceExtensions
    {
        public static AttributeReference GetAttributeReference(this BlockReference blockReference, string tag)
        {
            using (Transaction transaction = blockReference.Database.TransactionManager.StartTransaction())
            {
                foreach (ObjectId attRefID in blockReference.AttributeCollection)
                {
                    AttributeReference attRef = transaction.GetObject(attRefID, OpenMode.ForRead) as AttributeReference;
                    if (attRef.Tag == tag)
                    {
                        return attRef;
                    }
                }
            }
            return null;
        }
    }
}

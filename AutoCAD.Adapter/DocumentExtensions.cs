using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter
{
    public static class DocumentExtensions
    {
        public static ElectricalDocumentProperties GetElectricalProperties(this Document document)
        {
            using(Transaction transaction = document.Database.TransactionManager.StartTransaction())
            {
                ObjectIdCollection references = document.Database.GetBlockTableRecord("WD_M").GetBlockReferenceIds(true, false);
                if (references.Count == 0)
                    return null;

                BlockReference wd_m = transaction.GetObject(references[0], OpenMode.ForRead) as BlockReference;
                return new ElectricalDocumentProperties
                {
                    Sheet = wd_m.GetAttributeReference("SHEET").TextString
                };
            }
        }

        public static string GetPageNumber(this Document document)
        {
            return GetElectricalProperties(document).Sheet;
        }
    }
}

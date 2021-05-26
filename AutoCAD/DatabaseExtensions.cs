using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD
{
    public static class DatabaseExtensions
    {
        public static BlockTable GetBlockTable (this Database database)
        {
            using(Transaction transaction = database.TransactionManager.StartTransaction())
            {
                return transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;
            }
        }
    }
}

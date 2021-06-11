using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter
{
    public abstract class Insertable
    {
        private Entity _entity;

        public Insertable(Entity entity)
        {
            _entity = entity;
        }

        public void Insert()
        {
            Insert(Application.DocumentManager.MdiActiveDocument.Database);
        }

        public void Insert(Database database)
        {
            using(Transaction transaction = database.TransactionManager.StartTransaction())
            {
                Insert(database, transaction);
                transaction.Commit();
            }
        }

        public virtual void Insert(Database database, Transaction transaction)
        {
            BlockTableRecord modelSpace = transaction.GetObject(database.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;
            modelSpace.AppendEntity(_entity);
            transaction.AddNewlyCreatedDBObject(_entity, true);
        }
    }
}

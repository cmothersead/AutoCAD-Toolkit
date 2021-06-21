using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class TitleBlockInsertion
    {
        private static List<ObjectId> _forDelete = new List<ObjectId>();

        public static void Handler(object sender, ObjectEventArgs args)
        {
            if (args.DBObject is BlockReference reference)
                if (reference.Name.Contains("Title Block"))
                {
                    if (reference.GetBlockTableRecord().GetBlockReferenceIds(true, false).Count > 1)
                    {
                        _forDelete.Add(reference.ObjectId);
                        Application.DocumentManager.MdiActiveDocument.CommandEnded += new CommandEventHandler(Delete);
                        Application.DocumentManager.MdiActiveDocument.CommandCancelled += new CommandEventHandler(Delete);
                    } 
                }
                     
        }

        public static void Delete(object sender, CommandEventArgs args)
        {
            using(Transaction transaction = Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in _forDelete)
                {
                    DBObject obj = transaction.GetObject(id, OpenMode.ForWrite);
                    obj.Erase();
                }
                transaction.Commit();
            }
            _forDelete = new List<ObjectId>();
            Application.DocumentManager.MdiActiveDocument.CommandEnded -= new CommandEventHandler(Delete);
            Application.DocumentManager.MdiActiveDocument.CommandCancelled -= new CommandEventHandler(Delete);
        }
    }
}

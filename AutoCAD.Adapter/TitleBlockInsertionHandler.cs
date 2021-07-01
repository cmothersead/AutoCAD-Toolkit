using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class TitleBlockInsertion
    {
        private static List<ObjectId> _forDelete = new List<ObjectId>();
        private static Document CurrentDocument => Application.DocumentManager.MdiActiveDocument;

        public static void Handler(object sender, ObjectEventArgs args)
        {
            if (args.DBObject is BlockReference reference)
                if (reference.Name.Contains("Title Block"))
                {
                    if (reference.GetBlockTableRecord().GetBlockReferenceIds(true, false).Count > 1)
                    {
                        _forDelete.Add(reference.ObjectId);
                        CurrentDocument.CommandEnded += new CommandEventHandler(Delete);
                        CurrentDocument.CommandCancelled += new CommandEventHandler(Delete);
                    } 
                }
                     
        }

        public static void Delete(object sender, CommandEventArgs args)
        {
            using(Transaction transaction = CurrentDocument.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in _forDelete)
                {
                    DBObject obj = transaction.GetObject(id, OpenMode.ForWrite);
                    obj.Erase();
                }
                transaction.Commit();
            }
            _forDelete = new List<ObjectId>();
            CurrentDocument.CommandEnded -= new CommandEventHandler(Delete);
            CurrentDocument.CommandCancelled -= new CommandEventHandler(Delete);
            CurrentDocument.Editor.WriteMessage("\nTitle Block already present on drawing.");
        }
    }
}

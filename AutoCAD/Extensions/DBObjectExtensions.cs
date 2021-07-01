using Autodesk.AutoCAD.DatabaseServices;

namespace ICA.AutoCAD
{
    public static class DBObjectExtensions
    {
        public static void Erase(this DBObject obj, Transaction transaction)
        {
            transaction.GetObject(obj.ObjectId, OpenMode.ForWrite).Erase();
        }
    }
}

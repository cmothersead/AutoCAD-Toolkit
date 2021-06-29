using Autodesk.AutoCAD.DatabaseServices;

namespace ICA.AutoCAD
{
    public static class ObjectIdExtensions
    {
        public static DBObject Open(this ObjectId id)
        {
            return id.Database.Open(id);
        }
    }
}

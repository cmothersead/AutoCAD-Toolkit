using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD
{
    public static class GroupExtensions
    {
        #region Public Extension Methods

        public static List<Entity> GetEntities(this Group group, Transaction transaction) => group.GetAllEntityIds()
                                                                                                  .Select(id => id.Open(transaction) as Entity)
                                                                                                  .ToList();

        #endregion

        #region Transacted Overloads

        public static List<Entity> GetEntities(this Group group) => group.Transact(GetEntities);

        #endregion
    }
}

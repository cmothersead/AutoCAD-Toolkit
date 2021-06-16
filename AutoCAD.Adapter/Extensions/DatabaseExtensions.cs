using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public static class DatabaseExtensions
    {
        #region Public Extension Methods

        public static ElectricalDocumentProperties ElectricalProperties(this Database database)
        {
            Dictionary<string, string> properties = database.GetCustomProperties();
            if (properties.Count != 0)
                return new ElectricalDocumentProperties(properties);

            database.SetCustomProperties(WDM.Read(database));
            return new ElectricalDocumentProperties(database.GetCustomProperties());
        }

        public static string GetPageNumber(this Database database)
        {
            return database.ElectricalProperties().Sheet.SheetNumber;
        }

        #endregion
    }
}

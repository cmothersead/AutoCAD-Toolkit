using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public static class DatabaseExtensions
    {
        #region Public Extension Methods

        #region Document Properties

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

        #region Title Block

        public static TitleBlockRecord GetTitleBlock(this Database database)
        {
            BlockTable blockTable = database.GetBlockTable();
            BlockTableRecord titleBlock;
            foreach (string name in blockTable.GetRecordNames())
            {
                if (!name.Contains("Title Block"))
                    continue;

                titleBlock = blockTable.GetRecord(name);
                if (!titleBlock.HasAttribute("TB"))
                    continue;

                return new TitleBlockRecord(titleBlock);
            }
            return null;
        }

        #endregion

        #region Ladder

        public static bool HasLadder(this Database database)
        {
            if (database.GetLadder().Count > 0)
                return true;

            return false;
        }

        public static ObjectIdCollection GetLadder(this Database database)
        {
            if (!database.HasLayer(ElectricalLayers.LadderLayer))
                return new ObjectIdCollection();

            return database.GetLayer(ElectricalLayers.LadderLayer).GetEntities();
        }

        #endregion

        #endregion
    }
}

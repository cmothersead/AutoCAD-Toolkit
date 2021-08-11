using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public static string GetSheetNumber(this Database database) => database.ElectricalProperties().Sheet.SheetNumber;

        public static void SetPageNumber(this Database database, string value)
        {
            ElectricalDocumentProperties properties = database.ElectricalProperties();
            properties.Sheet.SheetNumber = value;
            database.SetCustomProperties(properties.ToDictionary());
        }

        #endregion

        #region Title Block

        public static TitleBlock GetTitleBlock(this Database database)
        {
            BlockTable blockTable = database.GetBlockTable();
            BlockTableRecord titleBlock;
            foreach (string name in blockTable.GetRecords().Select(r => r.Name))
            {
                if (!name.Contains("Title Block"))
                    continue;

                titleBlock = blockTable.GetRecord(name);
                if (!titleBlock.HasAttribute("TB"))
                    continue;

                return new TitleBlock(titleBlock);
            }
            return null;
        }

        public static bool ContainsTBAtrribute(this Database database)
        {
            foreach (ObjectId id in database.GetModelSpace())
                if (id.Open() is AttributeDefinition definition)
                    if (definition.Tag == "TB")
                        return true;

            return false;
        }

        #endregion

        #region Ladder

        public static bool HasLadder(this Database database)
        {
            if (database.GetLadder() != null) // Refactor this to reduce the workload/redundancy
                return true;

            return false;
        }

        public static Ladder GetLadder(this Database database)
        {
            if (!database.HasLayer(ElectricalLayers.LadderLayer))
                return null;

            return new Ladder(database.GetLayer(ElectricalLayers.LadderLayer).GetEntities());
        }

        #endregion

        #region Project

        public static Project GetProject(this Database database) => new Project(Directory.GetFiles(Path.GetDirectoryName(database.OriginalFileName), "*.wdp")[0]);

        #endregion

        #endregion
    }
}

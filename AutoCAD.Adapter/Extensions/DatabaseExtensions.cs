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

        public static DrawingSettings ElectricalSetting(this Database database)
        {
            if (database.GetCustomProperty("WD_MRead") == "1")
                return new DrawingSettings(database.GetAllCustomProperties());

            database.SetCustomProperties(WDM.Read(database));
            return new DrawingSettings(database.GetAllCustomProperties());
        }

        public static string GetSheetNumber(this Database database) => database.GetCustomProperty("Sheet");

        public static void SetSheetNumber(this Database database, string value) => database.SetCustomProperty("Sheet", value);

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

        public static Project GetProject(this Database database) => Project.Open(Path.GetDirectoryName(database.OriginalFileName));

        #endregion

        #endregion
    }
}

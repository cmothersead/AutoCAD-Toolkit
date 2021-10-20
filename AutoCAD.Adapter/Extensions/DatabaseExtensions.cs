using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ICA.AutoCAD.Adapter
{
    public static class DatabaseExtensions
    {
        #region Public Extension Methods

        #region Document Settings

        public static DrawingSettings ElectricalSetting(this Database database)
        {
            if (database.GetCustomProperty("WD_MRead") == "1")
                return new DrawingSettings(database.GetAllCustomProperties());

            database.SetCustomProperties(WDM.Read(database));
            return new DrawingSettings(database.GetAllCustomProperties());
        }

        public static string GetSheetNumber(this Database database) => database.GetCustomProperty("Sheet");

        public static void SetSheetNumber(this Database database, string value) => database.SetCustomProperty("Sheet", value);

        public static List<string> GetDescription(this Database database) => database.GetAllCustomProperties()
                                                                                     .Where(prop => prop.Key.Contains("Description"))
                                                                                     .OrderBy(prop => prop.Key)
                                                                                     .Select(prop => prop.Value)
                                                                                     .ToList();

        public static void SetDescription(this Database database, List<string> values)
        {
            Dictionary<string, string> descriptions = database.GetAllCustomProperties()
                                                              .Where(prop => prop.Key.Contains("Description"))
                                                              .ToDictionary(x => x.Key, y => y.Value);
            if (values == descriptions.Select(desc => desc.Value).ToList())
                return;

            database.RemoveCustomProperties(descriptions.Select(prop => prop.Key));
            database.SetCustomProperties(values.Select((value, index) => (value, index))
                                               .ToDictionary(item => $"Description {item.index + 1}", item => item.value));
            database.SaveAs(database.Filename, DwgVersion.Current);
        }

        public static void AddRegApp(this Database database, Transaction transaction)
        {
            RegAppTable table = database.GetRegisteredApplicationTable(transaction);
            if (table.Has("ICA"))
                return;
            RegAppTableRecord regAppTableRecord = new RegAppTableRecord() { Name = "ICA" };
            table.GetForWrite(transaction).Add(regAppTableRecord);
            transaction.AddNewlyCreatedDBObject(regAppTableRecord, true);
        }

        public static void AddRegApp(this Database database) => database.Transact(AddRegApp);

        #endregion

        #region Title Block

        public static TitleBlock GetTitleBlock(this Database database)
        {
            BlockTableRecord titleBlockRecord = database.GetBlockTable()
                                                  .GetRecords()
                                                  .FirstOrDefault(record => record.Name.Contains("Title Block") && record.HasAttribute("TB"));
            return titleBlockRecord == null ? null : new TitleBlock(titleBlockRecord);
        }

        public static bool ContainsTBAtrribute(this Database database) => database.GetModelSpace()
                                                                                  .Cast<ObjectId>()
                                                                                  .Select(id => id.Open())
                                                                                  .OfType<AttributeDefinition>()
                                                                                  .Any(definition => definition.Tag == "TB");

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

        public static Project GetProject(this Database database) => Project.Open(Path.GetDirectoryName(database.OriginalFileName)) ?? Project.Import(Path.GetDirectoryName(database.OriginalFileName));

        #endregion

        #endregion
    }
}

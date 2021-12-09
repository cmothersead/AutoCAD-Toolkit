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

        public static DBDictionary GetNamedDictionary(this Database database, Transaction transaction, string name)
        {
            DBDictionary namedObjectDictionary = database.GetNamedObjectDictionary(transaction);
            if (!namedObjectDictionary.Contains(name))
            {
                DBDictionary newDict = new DBDictionary();
                namedObjectDictionary.GetForWrite(transaction).SetAt(name, newDict);
                transaction.AddNewlyCreatedDBObject(newDict, true);
            }
            return namedObjectDictionary.GetAt(name).Open(transaction) as DBDictionary;
        }

        #endregion

        #region Symbols

        public static void LogSymbols(this Database database, Transaction transaction)
        {
            DBDictionary parents = database.GetNamedDictionary(transaction, "Parents").GetForWrite(transaction);
            ;
            database.GetEntities(transaction)
                    .OfType<BlockReference>()
                    .Where(blockReference => blockReference.Layer == ElectricalLayers.SymbolLayer.Name && blockReference.HasAttributeReference("TAG1"))
                    .ForEach(blockReference => parents.SetAt(blockReference.Handle.ToString(), blockReference.GetForWrite(transaction)));

            DBDictionary children = database.GetNamedDictionary(transaction, "Children").GetForWrite(transaction);
            database.GetEntities(transaction)
                    .OfType<BlockReference>()
                    .Where(blockReference => blockReference.Layer == ElectricalLayers.SymbolLayer.Name && blockReference.HasAttributeReference("TAG2"))
                    .ForEach(blockReference => children.SetAt(blockReference.Handle.ToString(), blockReference.GetForWrite(transaction)));
        }

        public static List<ParentSymbol> GetParentSymbols(this Database database, Transaction transaction)
        {
            List<ParentSymbol> parents = new List<ParentSymbol>();
            foreach(var entry in database.GetNamedDictionary(transaction, "Parents"))
                parents.Add(new ParentSymbol(entry.Value.Open(transaction) as BlockReference));
            return parents;
        }

        public static List<ChildSymbol> GetChildSymbols(this Database database, Transaction transaction)
        {
            List<ChildSymbol> children = new List<ChildSymbol>();
            foreach (var entry in database.GetNamedDictionary(transaction, "Children"))
                children.Add(new ChildSymbol(entry.Value.Open(transaction) as BlockReference));
            return children;
        }

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
            if (!database.HasLayer(ElectricalLayers.LadderLayer))
                return false;

            using(Transaction transaction = database.TransactionManager.StartTransaction())
            {
                LayerTableRecord ladderLayer = database.GetLayer(transaction, ElectricalLayers.LadderLayer);
                if (ladderLayer.GetEntities(transaction).Count != 0)
                    return true;
            }

            return false;
        }

        public static List<Ladder> GetLadders(this Database database)
        {
            if (!database.HasLadder())
                return null;

            List<Ladder> ladders = new List<Ladder>();

            List<Entity> entities = database.GetLayer(ElectricalLayers.LadderLayer).GetEntities().ToList();

            List<double> ladderPositions = entities.OfType<BlockReference>()
                                                   .Select(blockReference => blockReference.Position.X)
                                                   .Distinct()
                                                   .OrderByDescending(x => x)
                                                   .ToList();

            foreach(double xPosition in ladderPositions)
            {
                List<Entity> rails = entities.OfType<Line>()
                                             .Where(line => line.StartPoint.X >= xPosition)
                                             .Cast<Entity>()
                                             .ToList();
                List<Entity> lineNumbers = entities.OfType<BlockReference>()
                                                   .Where(blockReference => blockReference.Position.X >= xPosition)
                                                   .Cast<Entity>()
                                                   .ToList();
                List<Entity> ladderEntities = rails.Union(lineNumbers).ToList();
                ladders.Add(new Ladder(ladderEntities));

                entities.RemoveAll(entity => ladderEntities.Contains(entity));
            }

            return ladders;
        }

        #endregion

        #region Project

        public static Project GetProject(this Database database) => Project.Open(Path.GetDirectoryName(database.OriginalFileName)) ?? Project.Import(Path.GetDirectoryName(database.OriginalFileName));

        #endregion

        #endregion

        #region Transacted Overloads

        public static void AddRegApp(this Database database) => database.Transact(AddRegApp);

        public static DBDictionary GetNamedDictionary(this Database database, string name) => database.Transact(GetNamedDictionary, name);

        public static void LogSymbols(this Database database) => database.Transact(LogSymbols);

        public static List<ParentSymbol> GetParentSymbols(this Database database) => database.Transact(GetParentSymbols);

        public static List<ChildSymbol> GetChildSymbols(this Database database) => database.Transact(GetChildSymbols);

        #endregion
    }
}

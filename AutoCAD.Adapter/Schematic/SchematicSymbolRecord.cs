using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ICA.AutoCAD.IO;
using ICA.Schematic;
using System;

namespace ICA.AutoCAD.Adapter
{
    public class SchematicSymbolRecord
    {
        #region Fields

        #region Private Fields

        private readonly BlockTableRecord _blockTableRecord;

        #endregion

        #endregion

        #region Properties

        #region Public Properties

        public ObjectId ObjectId => _blockTableRecord.ObjectId;

        public Database Database => _blockTableRecord.Database;

        #endregion

        #endregion

        #region Constructors

        public SchematicSymbolRecord(BlockTableRecord record)
        {
            _blockTableRecord = record;
        }

        #endregion

        #region Methods

        #region Public Methods

        public ISymbol InsertSymbol(Transaction transaction, Point2d location = new Point2d(), Symbol.Type? type = null)
        {
            BlockReference blockReference = new BlockReference(location.ToPoint3d(), _blockTableRecord.ObjectId);

            if (blockReference.Position == Point3d.Origin)
            {
                Document currentDocument = Application.DocumentManager.MdiActiveDocument;
                SymbolJig jig = new SymbolJig(currentDocument.Editor.CurrentUserCoordinateSystem, transaction, blockReference);

                if (jig.Run() != PromptStatus.OK)
                    return null;
            }

            if (type is null)
            {
                if (_blockTableRecord.HasAttribute("TAG1"))
                    type = Symbol.Type.Parent;
                else if (_blockTableRecord.HasAttribute("TAG2"))
                    type = Symbol.Type.Child;
                else
                {
                    PromptKeywordOptions options = new PromptKeywordOptions("\nSymbol type: ");
                    options.Keywords.Add("Parent");
                    options.Keywords.Add("Child");
                    options.Keywords.Default = "Parent";

                    PromptResult result = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(options);
                    if (result.Status != PromptStatus.OK)
                        return null;

                    if (Enum.TryParse(result.StringResult, out Symbol.Type parsed))
                        type = parsed;
                }
            }

            blockReference.Insert(transaction, Database, ElectricalLayers.SymbolLayer);

            switch (type)
            {
                case Symbol.Type.Parent:
                    return new ParentSymbol(blockReference);
                case Symbol.Type.Child:
                    return new ChildSymbol(blockReference);
                default:
                    return null;
            }
        }

        public ISymbol InsertSymbol(Point2d location = new Point2d(), Symbol.Type? type = null)
        {
            ISymbol symbol;
            using (Transaction transaction = _blockTableRecord.Database.TransactionManager.StartTransaction())
            {
                symbol = InsertSymbol(transaction, location, type);
                transaction.Commit();
            }
            return symbol;
        }

        #endregion

        #region Public Static Methods

        public static SchematicSymbolRecord GetRecord(Database database, string name)
        {
            BlockTable blockTable = database.GetBlockTable();
            BlockTableRecord record;

            if (blockTable.Has(name))
                record = blockTable.GetRecord(name);
            else
                record = blockTable.LoadExternalBlockTableRecord(Paths.FindSchematic(name));

            if (record is null)
            {
                if (name == "")
                    return null;

                throw new ArgumentException($"Symbol with name: \"{name}\" not found");
            }

            return new SchematicSymbolRecord(record);
        }

        #endregion

        #endregion
    }
}

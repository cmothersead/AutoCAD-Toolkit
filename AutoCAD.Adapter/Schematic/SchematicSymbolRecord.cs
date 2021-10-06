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

        public ISymbol InsertSymbol(Transaction transaction, Point2d location = new Point2d())
        {
            BlockReference blockReference = new BlockReference(location.ToPoint3d(), _blockTableRecord.ObjectId);

            blockReference.Insert(transaction, Database);
            blockReference.SetLayer(transaction, ElectricalLayers.SymbolLayer);

            if (blockReference.Position == Point3d.Origin)
            {
                Document currentDocument = Application.DocumentManager.MdiActiveDocument;
                SymbolJig jig = new SymbolJig(currentDocument.Editor.CurrentUserCoordinateSystem, transaction, blockReference);

                if (jig.Run() != PromptStatus.OK)
                    return null;
            }

            if (_blockTableRecord.HasAttribute("TAG1"))
            {
                if (blockReference.Database.HasLadder())
                    blockReference.SetAttributeValue("TAG1", $"{blockReference.Database.GetLadder().ClosestLineNumber(blockReference.Position)}{blockReference.GetAttributeValue("FAMILY")}");

                return new ParentSymbol(blockReference);
            }  
            else if (_blockTableRecord.HasAttribute("TAG2"))
            {
                return new ChildSymbol(blockReference);
            }
            else
            {
                PromptKeywordOptions options = new PromptKeywordOptions("\nSymbol type: ");
                options.Keywords.Add("Parent");
                options.Keywords.Add("Child");
                options.Keywords.Default = "Parent";

                PromptResult result = Application.DocumentManager.MdiActiveDocument.Editor.GetKeywords(options);
                if (result.Status != PromptStatus.OK)
                    return null;

                switch (result.StringResult)
                {
                    case "Parent":
                        return new ParentSymbol(blockReference);
                    case "Child":
                        return new ChildSymbol(blockReference);
                }
            }

            return null;
        }

        public ISymbol InsertSymbol(Point2d location = new Point2d())
        {
            ISymbol symbol;
            using (Transaction transaction = _blockTableRecord.Database.TransactionManager.StartTransaction())
            {
                symbol = InsertSymbol(transaction, location);
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
                throw new ArgumentException($"Symbol with name: \"{name}\" not found");
            }

            return new SchematicSymbolRecord(record);
        }

        #endregion

        #endregion
    }
}

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

        public ISymbol InsertSymbol(Transaction transaction, Document document)
        {
            BlockReference blockReference = new BlockReference(Point3d.Origin, _blockTableRecord.ObjectId);

            Symbol symbol = GetSymbol(document, blockReference) as Symbol;

            symbol.Insert(transaction, Database);

            if (SymbolJig.Run(document, symbol) != PromptStatus.OK)
                return null;

            return symbol as ISymbol;
        }

        public ISymbol InsertSymbol(Document document)
        {
            if (document.Database != _blockTableRecord.Database)
                return null;

            using (Transaction transaction = _blockTableRecord.Database.TransactionManager.StartTransaction())
            {
                ISymbol symbol = InsertSymbol(transaction, document);
                transaction.Commit();
                return symbol;
            }
        }

        public ISymbol InsertSymbol(Transaction transaction, Point2d location, Symbol.Type type)
        {
            BlockReference blockReference = new BlockReference(location.ToPoint3d(), _blockTableRecord.ObjectId);

            Symbol symbol = GetSymbol(blockReference, type) as Symbol;

            symbol.Insert(transaction, Database);

            return symbol as ISymbol;
        }

        public ISymbol InsertSymbol(Point2d location, Symbol.Type type)
        {
            using (Transaction transaction = _blockTableRecord.Database.TransactionManager.StartTransaction())
            {
                ISymbol symbol = InsertSymbol(transaction, location, type);
                transaction.Commit();
                return symbol;
            }
        }

        private ISymbol GetSymbol(Document document, BlockReference blockReference)
        {
            Symbol.Type type = Symbol.Type.Parent;

            if (_blockTableRecord.HasAttribute("TAG2"))
                type = Symbol.Type.Child;
            else
            {
                PromptKeywordOptions options = new PromptKeywordOptions("\nSymbol type: ");
                options.Keywords.Add("Parent");
                options.Keywords.Add("Child");
                options.Keywords.Default = "Parent";

                PromptResult result = document.Editor.GetKeywords(options);
                if (result.Status != PromptStatus.OK)
                    return null;

                if (Enum.TryParse(result.StringResult, out Symbol.Type parsed))
                    type = parsed;
            }

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

        private ISymbol GetSymbol(BlockReference blockReference, Symbol.Type type)
        {
            if (_blockTableRecord.HasAttribute("TAG1") && type != Symbol.Type.Parent)
                throw new ArgumentException($"TAG1 attribute present in block, symbol can only be of type: \"{nameof(Symbol.Type.Parent)}\"");

            if (_blockTableRecord.HasAttribute("TAG2") && type != Symbol.Type.Child)
                throw new ArgumentException($"TAG1 attribute present in block, symbol can only be of type: \"{nameof(Symbol.Type.Parent)}\"");

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

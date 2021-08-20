using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ICA.AutoCAD.IO;
using ICA.Schematic;
using System;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class SchematicSymbolRecord
    {

        #region Private Fields

        private BlockTableRecord _blockTableRecord;

        #endregion

        #region Public Properties

        public ObjectId ObjectId => _blockTableRecord.ObjectId;

        #endregion

        #region Constructors

        public SchematicSymbolRecord(BlockTableRecord record)
        {
            _blockTableRecord = record;
        }

        #endregion

        #region Public Methods

        public ISymbol InsertSymbol(Transaction transaction, Point2d location = new Point2d())
        {
            BlockReference blockReference;

            if (location != new Point2d())
                blockReference = new BlockReference(location.ToPoint3d(), _blockTableRecord.ObjectId);
            else
                blockReference = new BlockReference(Point3d.Origin, _blockTableRecord.ObjectId);

            blockReference.Insert(transaction, _blockTableRecord.Database);

            ElectricalLayers.Assign(transaction, blockReference);

            if (blockReference.Position == Point3d.Origin)
            {
                Document currentDocument = Application.DocumentManager.MdiActiveDocument;
                SymbolJig jig = new SymbolJig(currentDocument.Editor.CurrentUserCoordinateSystem, transaction, blockReference);

                if (jig.Run() != PromptStatus.OK)
                    return null;
            }

            if (blockReference.Database.HasLadder())
                blockReference.SetAttributeValue("TAG1", $"{blockReference.Database.GetLadder().ClosestLineNumber(blockReference.Position)}{blockReference.GetAttributeValue("FAMILY")}");

            return new ParentSymbol(blockReference);
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
    }
}

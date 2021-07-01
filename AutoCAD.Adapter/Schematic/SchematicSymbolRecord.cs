﻿using Autodesk.AutoCAD.ApplicationServices;
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
        private BlockTableRecord _blockTableRecord;

        public ObjectId ObjectId => _blockTableRecord.ObjectId;

        public SchematicSymbolRecord(BlockTableRecord record)
        {
            _blockTableRecord = record;
        }

        public static SchematicSymbolRecord GetRecord(string name)
        {
            BlockTable blockTable = Application.DocumentManager.MdiActiveDocument.Database.GetBlockTable();
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

        public ISymbol InsertSymbol(Transaction transaction, Point2d location = new Point2d())
        {
            BlockReference blockReference;

            if (location != new Point2d())
                blockReference = new BlockReference(location.ToPoint3d(), _blockTableRecord.ObjectId);
            else
                blockReference = new BlockReference(Point3d.Origin, _blockTableRecord.ObjectId);

            blockReference.Insert(_blockTableRecord.Database, transaction);

            if (blockReference.Position == Point3d.Origin)
            {
                Document currentDocument = Application.DocumentManager.MdiActiveDocument;
                var jig = new SymbolJig(currentDocument.Editor.CurrentUserCoordinateSystem, transaction, blockReference);

                if (jig.Run() != PromptStatus.OK)
                    return null;
            }
                
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
    }
}
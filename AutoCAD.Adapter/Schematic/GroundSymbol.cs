using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ICA.AutoCAD.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ICA.AutoCAD.Adapter
{
    public class GroundSymbol
    {
        #region Private Properties

        private BlockReference BlockReference { get; }

        #endregion

        #region Public Properties

        public Database Database => BlockReference.Database;

        public Point2d Position => BlockReference.Position.ToPoint2D();

        public List<WireConnection> WireConnections => BlockReference.GetAttributeReferences()
                                                                     .Where(reference => Regex.IsMatch(reference.Tag, @"X[1,2,4,8]TERM\d{2}"))
                                                                     .Select(reference => new WireConnection(reference))
                                                                     .ToList();

        #endregion

        #region Constructors

        public GroundSymbol(BlockReference reference)
        {
            BlockReference = reference;
        }

        #endregion

        #region Public Methods

        public void GroundConnectedWires()
        {
            foreach(Line line in Wire.GetAllSegments(Database).Where(line => line.IntersectsWith(Position)))
            {
                new Wire(line).Layer = ElectricalLayers.GroundLayer;
            }
        }

        #endregion

        #region Public Static Methods

        public static GroundSymbol Insert(Transaction transaction, Database database, Point2d location)
        {
            string name = "VGD2";
            BlockTable blockTable = database.GetBlockTable();

            BlockTableRecord record = blockTable.Has(name) ? blockTable.GetRecord(name) : blockTable.LoadExternalBlockTableRecord(Paths.FindSchematic(name));

            if (record is null)
                throw new ArgumentException($"Ground symbol \"VGD2\" missing from library");

            BlockReference blockReference = new BlockReference(location.ToPoint3d(), record.ObjectId);
            blockReference.Insert(transaction, database);

            return new GroundSymbol(blockReference);
        }

        public static GroundSymbol Insert(Transaction transaction, Document document)
        {
            string name = "VGD2";
            BlockTable blockTable = document.Database.GetBlockTable();

            BlockTableRecord record = blockTable.Has(name) ? blockTable.GetRecord(name) : blockTable.LoadExternalBlockTableRecord(Paths.FindSchematic(name));

            if (record is null)
                throw new ArgumentException($"Ground symbol \"VGD2\" missing from library");

            BlockReference blockReference = new BlockReference(new Point3d(), record.ObjectId);

            blockReference.Insert(transaction, document.Database);

            if (blockReference.Position == new Point3d())
            {
                Document currentDocument = Application.DocumentManager.MdiActiveDocument;
                SymbolJig jig = new SymbolJig(currentDocument.Editor.CurrentUserCoordinateSystem, transaction, blockReference);

                if (jig.Run() != PromptStatus.OK)
                    return null;
            }

            return new GroundSymbol(blockReference);
        }

        public static GroundSymbol Insert(Database database, Point2d location)
        {
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                GroundSymbol symbol = Insert(transaction, database, location);
                transaction.Commit();
                return symbol;
            }
        }

        public static GroundSymbol Insert(Document document)
        {
            using (Transaction transaction = document.Database.TransactionManager.StartTransaction())
            {
                GroundSymbol symbol = Insert(transaction, document);
                transaction.Commit();
                return symbol;
            }
        }

        #endregion
    }
}

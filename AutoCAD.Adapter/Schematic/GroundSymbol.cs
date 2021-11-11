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
        #region Properties

        #region Private Properties

        private BlockReference BlockReference { get; }

        private static string SymbolName => "VGD";

        #endregion

        #region Public Properties

        public Database Database => BlockReference.Database;

        public Point2d Position => BlockReference.Position.ToPoint2D();

        public List<WireConnection> WireConnections => (BlockReference.BlockTableRecord.Open() as BlockTableRecord).GetAttributeDefinitions()
                                                                     .Where(definition => Regex.IsMatch(definition.Tag, @"X[1,2,4,8]TERM\d{2}"))
                                                                     .Select(definition => new WireConnection(BlockReference, definition))
                                                                     .ToList();

        #endregion

        #endregion

        #region Constructors

        public GroundSymbol(BlockReference reference)
        {
            BlockReference = reference;
        }

        #endregion

        #region Methods

        #region Public Methods

        public void GroundConnectedWires() => Wire.GetAllSegments(Database)
                                                  .Where(line => line.IntersectsWith(Position))
                                                  .ForEach(line => line.SetLayer(ElectricalLayers.GroundLayer));

        #endregion

        #region Public Static Methods

        public static GroundSymbol Insert(Transaction transaction, Database database, Point2d location)
        {
            BlockTable blockTable = database.GetBlockTable();
            BlockTableRecord record = blockTable.Has(SymbolName) ? blockTable.GetRecord(SymbolName) : blockTable.LoadExternalBlockTableRecord(Paths.FindSchematic(SymbolName));

            if (record is null)
                throw new ArgumentException($"Ground symbol \"{SymbolName}\" missing from library.");

            BlockReference blockReference = new BlockReference(location.ToPoint3d(), record.ObjectId);
            blockReference.Insert(transaction, database);

            return new GroundSymbol(blockReference);
        }

        public static GroundSymbol Insert(Transaction transaction, Document document)
        {
            BlockTable blockTable = document.Database.GetBlockTable();

            BlockTableRecord record = blockTable.Has(SymbolName) ? blockTable.GetRecord(SymbolName) : blockTable.LoadExternalBlockTableRecord(Paths.FindSchematic(SymbolName));

            if (record is null)
                throw new ArgumentException($"Ground symbol \"{SymbolName}\" missing from library.");

            BlockReference blockReference = new BlockReference(new Point3d(), record.ObjectId);

            blockReference.Insert(transaction, document.Database);

            if (blockReference.Position == new Point3d())
            {
                SymbolJig jig = new SymbolJig(document.Editor.CurrentUserCoordinateSystem, transaction, blockReference, "Select location:");

                if (jig.Run(document) != PromptStatus.OK)
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

        #endregion
    }
}

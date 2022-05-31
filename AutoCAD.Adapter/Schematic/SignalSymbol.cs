using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ICA.AutoCAD.IO;
using ICA.Schematic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD.Adapter
{
    public class SignalSymbol : IReference
    {
        #region Properties

        #region Private Properties

        private BlockReference BlockReference { get; }

        private AttributeReference SignalCodeAttribute => BlockReference.GetAttributeReference("SIGCODE");

        private AttributeReference DescriptionAttribute => BlockReference.GetAttributeReference("DESC1");

        private AttributeReference XrefSheetAttribute => BlockReference.GetAttributeReference("SHEET");

        private AttributeReference XrefLineAttribute => BlockReference.GetAttributeReference("XREF");

        private AttributeReference WireNumberAttribute => BlockReference.GetAttributeReference("WIRENO");

        private Dictionary<string, LayerTableRecord> AttributeLayers => new Dictionary<string, LayerTableRecord>()
        {
            { "DESC", ElectricalLayers.DescriptionLayer },
            { "WIRENO", ElectricalLayers.WireNumberLayer },
            { "SHEET", ElectricalLayers.XrefLayer },
            { "XREF", ElectricalLayers.XrefLayer },
        };

        #endregion

        #region Public Properties

        public string SignalCode
        {
            get => SignalCodeAttribute?.TextString;
            set => SignalCodeAttribute?.SetValue(value);
        }

        public string Description
        {
            get => DescriptionAttribute?.TextString;
            set => DescriptionAttribute?.SetValue(value);
        }

        public string XrefSheet
        {
            get => XrefSheetAttribute?.TextString;
            set => XrefSheetAttribute?.SetValue(value);
        }

        public string XrefLine
        {
            get => XrefLineAttribute?.TextString;
            set => XrefLineAttribute?.SetValue(value);
        }

        public string WireNumber
        {
            get => WireNumberAttribute?.TextString;
            set => WireNumberAttribute?.SetValue(value);
        }

        public string Sheet => BlockReference.Database.GetSheetNumber();

        public string Line => Ladder.GetClosestLineNumber(BlockReference.Database.GetLadders(), BlockReference.Position).Replace(Sheet, "");

        #endregion

        #endregion

        #region Constructors

        public SignalSymbol(BlockReference blockReference) => BlockReference = blockReference;

        #endregion

        #region Methods

        #region Public Methods

        public void CrossReference(SignalSymbol referenceSymbol)
        {
            XrefSheet = referenceSymbol.Sheet;
            XrefLine = referenceSymbol.Line;
            referenceSymbol.XrefSheet = Sheet;
            referenceSymbol.XrefLine = Line;
            referenceSymbol.SignalCode = SignalCode;
        }

        public void AssignLayers()
        {
            using (Transaction transaction = BlockReference.Database.TransactionManager.StartTransaction())
            {
                BlockReference.GetAttributeReferences(transaction)
                              .ForEach(reference => reference.SetLayer(AttributeLayers.FirstOrDefault(pair => reference.Tag.Contains(pair.Key)).Value));
                transaction.Commit();
            }
        }

        #endregion

        #region Public Static Methods

        public static SignalSymbol Insert(Transaction transaction, Database database, Point2d location, string style, int direction)
        {
            BlockTable blockTable = database.GetBlockTable();

            string name = $"HA{style}{direction}";

            BlockTableRecord record = blockTable.Has(name) ? blockTable.GetRecord(name) : blockTable.LoadExternalBlockTableRecord(Paths.FindSchematic(name));

            if (record is null)
                throw new ArgumentException($"Symbol \"{name}\" not found in library.");

            BlockReference blockReference = new BlockReference(location.ToPoint3d(), record.ObjectId);
            blockReference.Insert(transaction, database, ElectricalLayers.SignalLayer);

            return new SignalSymbol(blockReference);
        }

        //public static SignalSymbol Insert(Transaction transaction, Document document, string style)
        //{
        //    BlockTable blockTable = document.Database.GetBlockTable();

        //    List<string> names = new List<string>
        //    {
        //        $"HA{style}1",
        //        $"HA{style}2",
        //        $"HA{style}3",
        //        $"HA{style}4",
        //    };

        //    List<BlockTableRecord> records = names.Select(name => blockTable.Has(name) ? blockTable.GetRecord(name) : blockTable.LoadExternalBlockTableRecord(Paths.FindSchematic(name)))
        //                                          .ToList();


        //    if (records.Any(record => record is null))
        //        throw new ArgumentException($"Symbol style {style} not complete in library.");

        //    BlockReference blockReference = new BlockReference(new Point3d(), records[0].ObjectId);

        //    blockReference.Insert(transaction, document.Database);

        //    if (blockReference.Position == new Point3d())
        //    {
        //        SymbolJig jig = new SymbolJig(document.Editor.CurrentUserCoordinateSystem, transaction, blockReference, "Select location:");

        //        if (jig.Run(document) != PromptStatus.OK)
        //            return null;
        //    }

        //    return new SignalSymbol(blockReference);
        //}

        public static SignalSymbol Insert(Database database, Point2d location, string style, int direction)
        {
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                SignalSymbol symbol = Insert(transaction, database, location, style, direction);
                transaction.Commit();
                return symbol;
            }
        }

        //public static SignalSymbol Insert(Document document, string name)
        //{
        //    using (Transaction transaction = document.Database.TransactionManager.StartTransaction())
        //    {
        //        SignalSymbol symbol = Insert(transaction, document, name);
        //        transaction.Commit();
        //        return symbol;
        //    }
        //}

        #endregion

        #endregion
    }
}

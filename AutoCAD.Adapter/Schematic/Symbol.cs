using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
using ICA.AutoCAD.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ICA.AutoCAD.Adapter
{
    public abstract class Symbol
    {
        #region Fields

        #endregion

        #region Properties

        #region Private Properties

        #endregion

        #region Protected Properties

        protected BlockReference BlockReference { get; }

        protected static Dictionary<string, LayerTableRecord> AttributeLayers => new Dictionary<string, LayerTableRecord>()
        {
            { "TAG", ElectricalLayers.TagLayer },
            { "MFG", ElectricalLayers.ManufacturerLayer },
            { "CAT", ElectricalLayers.PartNumberLayer },
            { "TERMDESC", ElectricalLayers.MiscellaneousLayer },
            { "DESC", ElectricalLayers.DescriptionLayer },
            { "TERM", ElectricalLayers.TerminalLayer },
            { "CON", ElectricalLayers.ConductorLayer },
            { "RATING", ElectricalLayers.RatingLayer },
            { "WIRENO", ElectricalLayers.WireNumberLayer },
            { "XREF", ElectricalLayers.XrefLayer }
        };

        protected Dictionary<string, string> Replacements => new Dictionary<string, string>()
        {
            { "%F", $"{Family}" },
            { "%S", $"" }, //Fix so that sheet is not included in line number
            { "%N", $"{LineNumber}" },
            { "%X", "1" } //suffix character for reference based tagging
        };

        private AttributeReference FamilyAttribute => BlockReference.GetAttributeReference("FAMILY");

        protected AttributeStack Stack { get; }

        #endregion

        #region Public Properties

        public string Family
        {
            get => FamilyAttribute?.TextString;
            set => FamilyAttribute?.SetValue(value);
        }

        public string LineNumber => BlockReference.Database.GetLadder()?.ClosestLineNumber(BlockReference.Position);

        public string SheetNumber => BlockReference.Database.GetSheetNumber();

        #endregion

        #endregion

        #region Constructors

        public Symbol(BlockReference blockReference)
        {
            BlockReference = blockReference;
            Stack = new AttributeStack(BlockReference, AttributeLayers)
            {
                Position = FamilyAttribute.Justify == AttachmentPoint.BaseLeft ?
                           FamilyAttribute.Position.ToPoint2D() :
                           FamilyAttribute.AlignmentPoint.ToPoint2D(),
                Justification = FamilyAttribute.Justify,
            };
        }

        #endregion

        #region Methods

        #region Public Methods

        public void CollapseAttributeStack() => Stack.Collapse();

        public void AssignLayers()
        {
            using (Transaction transaction = BlockReference.Database.TransactionManager.StartTransaction())
            {
                foreach (AttributeReference reference in BlockReference.GetAttributeReferences(transaction))
                {
                    KeyValuePair<string, LayerTableRecord> match = AttributeLayers.FirstOrDefault(pair => reference.Tag.Contains(pair.Key));
                    if (match.Key != null)
                        reference.SetLayer(transaction, match.Value);
                }
                transaction.Commit();
            }
        }

        #endregion

        #region Public Static Methods

        public static string PromptSymbolName(Editor editor)
        {
            OpenFileDialog fileDialog = new OpenFileDialog("Open Symbol", "", "dwg", "", 0);
            fileDialog.ShowDialog();
            if (fileDialog.Filename != "")
                return Paths.GetRelativePath(Paths.SchematicLibrary, fileDialog.Filename);

            PromptStringOptions options = new PromptStringOptions("Enter symbol name: ")
            {
                AllowSpaces = true
            };
            PromptResult result = editor.GetString(options);
            return result.StringResult;
        }

        #endregion

        #endregion
    }
}

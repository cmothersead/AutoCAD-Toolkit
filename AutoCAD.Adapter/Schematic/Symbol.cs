using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter
{
    public abstract class Symbol
    {
        #region Fields

        #endregion

        #region Properties

        #region Private

        

        #endregion

        #region Protected

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
            { "%S", $"SheetNumber" },
            { "%N", $"{LineNumber}" },
            { "%X", "1" } //suffix character for reference based tagging
        };

        private AttributeReference FamilyAttribute => BlockReference.GetAttributeReference("FAMILY");

        protected AttributeStack Stack { get; }

        #endregion

        #region Public

        public string Family
        {
            get => FamilyAttribute?.TextString;
            set => FamilyAttribute?.SetValue(value);
        }

        public string LineNumber => BlockReference.Database.GetLadder()?.ClosestLineNumber(BlockReference.Position);

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

        #region Public

        #endregion

        #endregion
    }
}

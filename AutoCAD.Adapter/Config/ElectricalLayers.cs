﻿using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace ICA.AutoCAD.Adapter
{
    public class ElectricalLayers
    {
        public LayerTableRecord SymbolLayer { get; set; } = new LayerTableRecord()
        {
            Name = "SYMS",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 7),
            Description = "Schematic symbol blocks"
        };
        public LayerTableRecord TagLayer { get; set; } = new LayerTableRecord()
        {
            Name = "TAGS",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 51),
            Description = "Tag attributes",
        };
        public LayerTableRecord DescriptionLayer { get; set; } = new LayerTableRecord()
        {
            Name = "DESC",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 1),
            Description = "Description attributes"
        };
        public LayerTableRecord TerminalLayer { get; set; } = new LayerTableRecord()
        {
            Name = "TERMS",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 130),
            Description = "Terminal number attributes"
        };
        public LayerTableRecord XrefLayer { get; set; } = new LayerTableRecord()
        {
            Name = "XREF",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 7),
            Description = "Cross reference attributes"
        };
        public LayerTableRecord LinkLayer { get; set; } = new LayerTableRecord()
        {
            Name = "LINK",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 9),
            Description = "Link lines"
        };
        public LayerTableRecord WireLayer { get; set; } = new LayerTableRecord()
        {
            Name = "WIRES",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 7),
            Description = "Default wires layer"
        };
        public LayerTableRecord WireNumberLayer { get; set; } = new LayerTableRecord()
        {
            Name = "WIRENO",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 3),
            Description = "Wire numbers"
        };
        public LayerTableRecord ManufacturerLayer { get; set; } = new LayerTableRecord()
        {
            Name = "MFG",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 150),
            Description = "Manufacturer name attributes"
        };
        public LayerTableRecord PartNumberLayer { get; set; } = new LayerTableRecord()
        {
            Name = "CAT",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 141),
            Description = "Part number attributes"
        };
        public LayerTableRecord LadderLayer { get; set; } = new LayerTableRecord()
        {
            Name = "LADDER",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 7),
            Description = "Ladder markings",
            IsLocked = true
        };
    }
}

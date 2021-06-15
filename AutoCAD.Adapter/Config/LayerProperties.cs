using Autodesk.AutoCAD.DatabaseServices;

namespace ICA.AutoCAD.Adapter
{
    public class LayerProperties
    {
        public LayerTableRecord TagLayer { get; set; }
        public LayerTableRecord DescriptionLayer { get; set; }
        public LayerTableRecord ChildDescriptionLayer { get; set; }
        public LayerTableRecord TerminalLayer { get; set; }
        public LayerTableRecord XrefLayer { get; set; }
        public LayerTableRecord ChildXrefLayer { get; set; }
        public LayerTableRecord ComponentLayer { get; set; }
        public LayerTableRecord LinkLayer { get; set; }
        public LayerTableRecord WireLayer { get; set; }
        public LayerTableRecord WireNumberLayer { get; set; }
    }
}

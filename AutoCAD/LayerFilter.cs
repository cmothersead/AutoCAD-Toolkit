using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD
{
    public class LayerFilter : List<string>
    {
        public SelectionFilter Filter => new SelectionFilter(this.Select(value => new TypedValue((int)DxfCode.LayerName, value)).ToArray());
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter
{
    public class ElectricalDocumentProperties
    {
        public SheetProperties SheetProperties { get; set; }
        public LadderProperties LadderProperties { get; set; }
        public ComponentProperties ComponentProperties { get; set; }
        public WireProperties WireProperties { get; set; }
    }
}

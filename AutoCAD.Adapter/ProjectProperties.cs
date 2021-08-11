using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter
{
    public class ProjectProperties
    {
        public Uri SchematicTemplate { get; set; }
        public Uri PanelTemplate { get; set; }
        public Uri ReferenceTemplate { get; set; }
        public Uri Library { get; set; }
        public LadderProperties Ladder { get; set; } = new LadderProperties();
        public ComponentProperties Component { get; set; } = new ComponentProperties();
        public WireProperties Wire { get; set; } = new WireProperties();
        public CrossReferenceProperties CrossReference { get; set; } = new CrossReferenceProperties();
    }
}

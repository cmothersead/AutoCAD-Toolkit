using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter
{
    public class SchematicTemplate
    {
        public string Name { get; set; }
        public string Voltage { get; set; }
        public int PhaseCount { get; set; }
        public int LadderCount { get; set; }
    }
}

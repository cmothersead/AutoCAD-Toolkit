using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic.Data
{
    public class Part
    {
        public int PartId { get; set; }
        public int FamilyId { get; set; }
        public int ManufacturerId { get; set; }
        public string PartNumber { get; set; }
    }
}

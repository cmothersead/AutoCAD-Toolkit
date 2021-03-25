using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic.Data
{
    public class Part : IPart
    {
        public int PartId { get; set; }
        public int FamilyId => Family.FamilyId;
        public int ManufacturerId => Manufacturer.ManufacturerId;
        public Family Family { get; }
        public Manufacturer Manufacturer { get; set; }
        public string PartNumber { get; set; }
    }
}

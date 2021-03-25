using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic.Data.Parts
{
    public class CircuitBreaker : IPart
    {
        public int PartId { get; set; }
        public Family Family { get; } = new Family { FamilyId = 3, FamilyCode = "CB" };
        public Manufacturer Manufacturer { get; set; }
        public int FamilyId => Family.FamilyId;
        public int ManufacturerId => Manufacturer.ManufacturerId;
        public string PartNumber { get; set; }
    }
}

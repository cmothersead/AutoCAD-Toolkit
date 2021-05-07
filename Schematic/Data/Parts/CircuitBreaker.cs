using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic.Data.Parts
{
    public class CircuitBreaker : IPart
    {
        public int Id { get; set; }
        public Family Family { get; } = new Family { Id = 3, Code = "CB" };
        public Manufacturer Manufacturer { get; set; }
        public int FamilyId => Family.Id;
        public int ManufacturerId => Manufacturer.Id;
        public string Number { get; set; }
    }
}

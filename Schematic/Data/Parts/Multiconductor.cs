using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic.Data.Parts
{
    public class Multiconductor : IMulticonductor
    {
        public int Id { get; set; }
        public Family Family { get; } = new Family { Id = 41, Code = "WI" };
        public int FamilyId { get; }
        public Manufacturer Manufacturer { get; set; }
        public int ManufacturerId { get; }
        public string Number { get; set; }
        public double OuterDiameter { get; set; }
        public List<IConductor> Conductors { get; set; }
        public bool Shielded { get; set; }
    }
}

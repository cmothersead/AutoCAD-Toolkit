using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic.Data
{
    public interface IPart
    {
        int PartId { get; set; }
        Family Family { get; }
        Manufacturer Manufacturer { get; set; }
        int FamilyId { get; }
        int ManufacturerId { get; }
        string PartNumber { get; set; }
    }
}

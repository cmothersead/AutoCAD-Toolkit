using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic.Data.Parts
{
    public interface IConductor
    {
        string Color { get; set; }
        int AWG { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic.Data.Parts
{
    public interface ITerminal
    {
        string TerminalNumber { get; set; }
        string TerminalDescription { get; set; }
        Potential ExpectedPotential { get; set; }
    }
}

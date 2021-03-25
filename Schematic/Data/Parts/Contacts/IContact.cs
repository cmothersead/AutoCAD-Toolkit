using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic.Data.Parts
{
    public interface IContact
    {
        ITerminal[] Terminals { get; set; }
        int MaxCurrent { get; set; }
    }
}

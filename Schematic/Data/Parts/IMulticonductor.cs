using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic.Data.Parts
{
    interface IMulticonductor : ICable
    {
        bool Shielded { get; set; }
    }
}

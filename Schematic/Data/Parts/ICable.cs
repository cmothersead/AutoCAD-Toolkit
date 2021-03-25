using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic.Data.Parts
{
    public interface ICable : IPart
    {
        double OuterDiameter { get; set; }
        List<IConductor> Conductors { get; set; }
    }
}

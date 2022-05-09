using System.Collections.Generic;

namespace ICA.Schematic.Data.Parts
{
    public interface ICable : IPart
    {
        double OuterDiameter { get; set; }
        List<IConductor> Conductors { get; set; }
    }
}

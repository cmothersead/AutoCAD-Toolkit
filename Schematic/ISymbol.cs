using System.Collections.Generic;

namespace ICA.Schematic
{
    public interface ISymbol : ILayered
    {
        string Family { get; set; }
        string Tag { get; set; }
        List<string> Description { get; set; }
    }
}

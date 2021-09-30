using System.Collections.Generic;

namespace ICA.Schematic
{
    public interface IComponent
    {
        string Tag { get; set; }
        List<string> Description { get; set; }
        string ManufacturerName { get; set; }
        string PartNumber { get; set; }
        string Family { get; }

        IParentSymbol Symbol { get; }
        List<IChildSymbol> Children { get; }
    }
}

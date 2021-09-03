using System.Collections.Generic;

namespace ICA.Schematic
{
    public interface ISymbol : ILayered
    {
        string Tag { get; set; }
        string Family { get; set; }
        List<string> Description { get; set; }
        string Enclosure { get; set; }
        string Location { get; set; }
        string ManufacturerName { get; set; }
        string PartNumber { get; set; }
    }
}

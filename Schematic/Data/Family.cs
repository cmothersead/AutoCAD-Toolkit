using System.Collections.Generic;

namespace ICA.Schematic.Data
{
    public class Family
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public IEnumerable<Manufacturer> Manufacturers { get; set; }
    }
}

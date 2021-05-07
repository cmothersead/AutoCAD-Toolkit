using System.Collections.Generic;

namespace ICA.Schematic.Data
{
    public class Manufacturer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Part> Parts { get; set; }
    }
}

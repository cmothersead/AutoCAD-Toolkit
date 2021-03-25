using System.Collections.Generic;

namespace ICA.Schematic
{
    public class Component
    {
        public IParentSymbol Parent { get; set; }
        public List<ISymbol> Children { get; set; }
    }
}

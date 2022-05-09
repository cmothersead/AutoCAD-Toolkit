using System.Collections.Generic;

namespace ICA.Schematic
{
    public interface ITitleBlock
    {
        List<ITBAttribute> Attributes { get; set; }
    }
}

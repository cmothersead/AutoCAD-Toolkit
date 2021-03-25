using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic.Data.Parts
{
    public interface IConnector
    {
        string Type { get; set; }
        int Pins { get; set; }
        Connector.Gender Gender { get; set; }
        Connector.Angle Angle { get; set; }
    }
}

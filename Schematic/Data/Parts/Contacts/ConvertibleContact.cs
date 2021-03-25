using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic.Data.Parts.Contacts
{
    public class ConvertibleContact : IContact
    {
        public ITerminal[] Terminals { get; set; } = new ITerminal[3];
        public int MaxCurrent { get; set; }
    }
}

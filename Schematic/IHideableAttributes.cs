using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.Schematic
{
    public interface IHideableAttributes
    {
        bool DescriptionHidden { get; set; }
        bool InstallationHidden { get; set; }
        bool PartInfoHidden { get; set; }
    }
}

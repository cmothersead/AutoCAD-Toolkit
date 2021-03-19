using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter.Windows.Models
{
    public class Family
    {
        public string FamilyCode { get; set; }
        public ObservableCollection<Manufacturer> Manufacturers { get; set; }
    }
}

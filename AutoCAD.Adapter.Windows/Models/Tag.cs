using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter.Windows.Models
{
    public class Tag
    {
        private string _value;
        public string Value
        {
            get => _value;
            set => _value = value.ToUpper();
        }
    }
}

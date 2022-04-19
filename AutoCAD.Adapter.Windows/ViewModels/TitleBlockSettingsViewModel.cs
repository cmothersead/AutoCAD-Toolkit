using ICA.AutoCAD.Adapter.Windows.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class TitleBlockSettingsViewModel : BaseViewModel
    {
        private TitleBlockFile _titleBlockFile;
        public List<KeyValuePair<string, string>> Attributes { get; set; }


        public TitleBlockSettingsViewModel(TitleBlockFile titleBlockFile)
        {
            _titleBlockFile = titleBlockFile;
        }
    }
}

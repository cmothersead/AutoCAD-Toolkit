using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ICA.Schematic.Components.EditWindowControls
{
    /// <summary>
    /// Interaction logic for InstallationControl.xaml
    /// </summary>
    public partial class InstallationControl : UserControl
    {
        public bool? IsChecked
        {
            get { return Installation_Checkbox.IsChecked;  }
            set { Installation_Checkbox.IsChecked = value; }
        }

        public InstallationControl()
        {
            InitializeComponent();
        }
    }
}

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

namespace ICA.AutoCAD.Adapter.Windows.Controls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class OkCancelControl : UserControl
    {
        public OkCancelControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty OkCommandProperty =
            DependencyProperty.Register(
        nameof(OkCommand),
        typeof(ICommand),
        typeof(OkCancelControl));
        public ICommand OkCommand
        {
            get => (ICommand)GetValue(OkCommandProperty);
            set => SetValue(OkCommandProperty, value);
        }
    }
}

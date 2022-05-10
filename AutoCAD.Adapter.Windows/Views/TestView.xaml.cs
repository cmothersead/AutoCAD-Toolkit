using ICA.AutoCAD.Adapter.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ICA.AutoCAD.Adapter.Windows.Views
{
    /// <summary>
    /// Interaction logic for TestView.xaml
    /// </summary>
    public partial class TestView : Window
    {
        public TestView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var test = FindName("Test") as TitleBlockPropertyControl;
        }
    }
}

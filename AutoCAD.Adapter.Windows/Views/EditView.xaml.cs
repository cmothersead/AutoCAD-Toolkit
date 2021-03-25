using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ICA.AutoCAD.Adapter.Windows.Models;
using ICA.AutoCAD.Adapter.Windows.ViewModels;
using ICA.Schematic;

namespace ICA.AutoCAD.Adapter.Windows.Views
{
    /// <summary>
    /// Interaction logic for EditView.xaml
    /// </summary>
    public partial class EditView : Window
    {
        public EditView()
        {
            InitializeComponent();
        }

        public EditView(EditViewModel editViewModel)
        {
            InitializeComponent();
            DataContext = editViewModel;
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

using System.Windows;
using ICA.AutoCAD.Adapter.Windows.ViewModels;

namespace ICA.AutoCAD.Adapter.Windows.Views
{
    /// <summary>
    /// Interaction logic for EditView.xaml
    /// </summary>
    public partial class EditView : Window
    {
        private readonly EditViewModel _editViewModel;

        public EditView(EditViewModel editViewModel)
        {
            InitializeComponent();
            _editViewModel = editViewModel;
            DataContext = _editViewModel;
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

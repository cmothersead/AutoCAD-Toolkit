using System.Windows;
using ICA.AutoCAD.Adapter.Windows.ViewModels;

namespace ICA.AutoCAD.Adapter.Windows.Views
{
    /// <summary>
    /// Interaction logic for EditView.xaml
    /// </summary>
    public partial class ParentSymbolEditView : Window
    {
        private readonly ParentSymbolEditViewModel _editViewModel;

        public ParentSymbolEditView(ParentSymbolEditViewModel editViewModel)
        {
            InitializeComponent();
            _editViewModel = editViewModel;
            DataContext = _editViewModel;
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            _editViewModel.UpdateSymbol();
            Close();
        }
    }
}

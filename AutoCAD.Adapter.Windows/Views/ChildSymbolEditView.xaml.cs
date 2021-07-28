using ICA.AutoCAD.Adapter.Windows.ViewModels;
using ICA.Schematic;
using System.Windows;

namespace ICA.AutoCAD.Adapter.Windows.Views
{
    /// <summary>
    /// Interaction logic for ChildSymbolEditView.xaml
    /// </summary>
    public partial class ChildSymbolEditView : Window
    {
        private readonly ChildSymbolEditViewModel _viewModel;

        public ChildSymbolEditView(IChildSymbol symbol)
        {
            InitializeComponent();
            _viewModel = new ChildSymbolEditViewModel(symbol);
            DataContext = _viewModel;
        }

        public ChildSymbolEditView(ChildSymbolEditViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.UpdateSymbol();
            Close();
        }
    }
}

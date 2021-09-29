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
        public ChildSymbolEditView(IChildSymbol symbol)
        {
            InitializeComponent();
            DataContext = new ChildSymbolEditViewModel(this, symbol);
        }
    }
}

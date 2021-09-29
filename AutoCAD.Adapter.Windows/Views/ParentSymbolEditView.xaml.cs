using System.Windows;
using ICA.AutoCAD.Adapter.Windows.ViewModels;
using ICA.Schematic;

namespace ICA.AutoCAD.Adapter.Windows.Views
{
    /// <summary>
    /// Interaction logic for EditView.xaml
    /// </summary>
    public partial class ParentSymbolEditView : Window
    {
        public ParentSymbolEditView(IParentSymbol symbol)
        {
            InitializeComponent();
            DataContext = new ParentSymbolEditViewModel(this, symbol);
        }
    }
}

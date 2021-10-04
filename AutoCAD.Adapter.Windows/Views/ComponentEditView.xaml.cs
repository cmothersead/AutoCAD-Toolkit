using System.Windows;
using ICA.AutoCAD.Adapter.Windows.ViewModels;
using ICA.Schematic;

namespace ICA.AutoCAD.Adapter.Windows.Views
{
    /// <summary>
    /// Interaction logic for EditView.xaml
    /// </summary>
    public partial class ComponentEditView : Window
    {
        public ComponentEditView(IComponent component)
        {
            InitializeComponent();
            DataContext = new ComponentEditViewModel(this, component);
        }
    }
}

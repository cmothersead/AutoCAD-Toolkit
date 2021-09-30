using ICA.AutoCAD.Adapter.Windows.ViewModels;
using ICA.Schematic;
using System.Collections.Generic;
using System.Windows;

namespace ICA.AutoCAD.Adapter.Windows.Views
{
    /// <summary>
    /// Interaction logic for ComponentsListView.xaml
    /// </summary>
    public partial class ComponentsListView : Window
    {
        public ComponentsListView(IEnumerable<IComponent> components)
        {
            InitializeComponent();
            DataContext = new ComponentsListViewModel(this, components);
        }
    }
}

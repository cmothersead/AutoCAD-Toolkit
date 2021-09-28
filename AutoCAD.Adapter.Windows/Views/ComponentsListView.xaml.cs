using ICA.AutoCAD.Adapter.Windows.ViewModels;
using ICA.Schematic;
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
using System.Windows.Shapes;

namespace ICA.AutoCAD.Adapter.Windows.Views
{
    /// <summary>
    /// Interaction logic for ComponentsListView.xaml
    /// </summary>
    public partial class ComponentsListView : Window
    {
        private readonly ComponentsListViewModel _viewModel;

        public ComponentsListView()
        {
            InitializeComponent();
            _viewModel = new ComponentsListViewModel();
            DataContext = _viewModel;
        }

        public ComponentsListView(IEnumerable<IParentSymbol> components)
        {
            InitializeComponent();
            _viewModel = new ComponentsListViewModel(components);
            DataContext = _viewModel;
        }

        public ComponentsListView(ComponentsListViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}

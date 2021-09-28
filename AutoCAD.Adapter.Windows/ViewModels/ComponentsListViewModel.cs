using ICA.Schematic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class ComponentsListViewModel : BaseViewModel
    {
        public ObservableCollection<IParentSymbol> Components { get; set; }
        public IParentSymbol SelectedComponent { get; set; }
        public ICommand SelectCommand { get; set; }

        public ComponentsListViewModel() { }

        public ComponentsListViewModel(IEnumerable<IParentSymbol> components)
        {
            Components = new ObservableCollection<IParentSymbol>(components);
        }
    }
}

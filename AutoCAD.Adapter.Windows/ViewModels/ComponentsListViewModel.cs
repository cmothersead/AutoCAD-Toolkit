using ICA.Schematic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class ComponentsListViewModel : BaseViewModel
    {
        #region Properties

        #region Public Properties

        public ObservableCollection<IComponent> Components { get; set; }
        public IComponent SelectedComponent { get; set; }
        public ICommand SelectCommand { get; set; }

        #endregion

        #endregion

        #region Constructors

        public ComponentsListViewModel(Window view, IEnumerable<IComponent> components)
        {
            _view = view;
            Components = new ObservableCollection<IComponent>(components);
            SelectCommand = new RelayCommand(SelectAndClose);
        }

        #endregion

        #region Methods

        #region Private Methods

        private void SelectAndEdit()
        {
            //((ComponentsListViewModel)_view.DataContext).SelectedComponent.
        }

        private void SelectAndClose()
        {
            _view.DialogResult = true;
            _view.Close();
        }

        #endregion

        #endregion
    }
}

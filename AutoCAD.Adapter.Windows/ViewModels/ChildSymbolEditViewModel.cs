using ICA.AutoCAD.Adapter.Windows.Models;
using ICA.Schematic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class ChildSymbolEditViewModel : SymbolEditViewModel
    {
        #region Properties

        #region Public Properties

        public IChildSymbol ChildSymbol => (IChildSymbol)_symbol;
        public bool TagHidden { get; set; }
        public DescriptionCollection Description { get; set; }
        public bool DescriptionHidden { get; set; }

        public ICommand SelectCommand { get; set; }

        #endregion

        #endregion

        #region Constructors

        public ChildSymbolEditViewModel(Window view, IChildSymbol symbol) : base(view, symbol)
        {
            Description = new DescriptionCollection(ChildSymbol.Description);
            DescriptionHidden = ChildSymbol.DescriptionHidden;
            OkCommand = new RelayCommand(UpdateSymbol);
            SelectCommand = new RelayCommand(SelectParent);
        }

        #endregion

        #region Methods

        private void UpdateSymbol()
        {
            ChildSymbol.Description = Description.Select(d => d.Value).Where(v => v != null).ToList();
            ChildSymbol.DescriptionHidden = DescriptionHidden;
            ChildSymbol.CollapseAttributeStack();
            base.UpdateAndClose();
        }

        private void SelectParent() => Tag = ChildSymbol.SelectParent().Tag;

        #endregion
    }
}

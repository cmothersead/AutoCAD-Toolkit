using ICA.Schematic;
using System.Windows;
using System.Windows.Input;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class SymbolEditViewModel : BaseViewModel
    {
        #region Fields

        #region Protected Fields

        protected ISymbol _symbol;

        #endregion

        #endregion

        #region Properties

        #region Public Properties

        public string Tag { get; set; }
        public ICommand OkCommand { get; set; }

        #endregion

        #endregion

        #region Constructors

        public SymbolEditViewModel(Window view, ISymbol symbol)
        {
            _view = view;
            _symbol = symbol;
            Tag = _symbol.Tag;

            OkCommand = new RelayCommand(UpdateAndClose);
        }

        #endregion

        #region Methods

        #region Protected Methods

        protected void UpdateAndClose()
        {
            _symbol.Tag = Tag;
            _view.DialogResult = true;
            _view.Close();
        }

        #endregion

        #endregion
    }
}

using System;
using System.Windows.Input;

namespace ICA.AutoCAD.Adapter.Windows
{
    public class RelayCommand : ICommand
    {
        #region Private Fields

        private Action _action;

        #endregion

        public event EventHandler CanExecuteChanged = (sender, e) => { };

        public RelayCommand(Action action) => _action = action;

        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _action();
    }
}

using System.ComponentModel;
using System.Windows;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        #region Fields

        #region Protected Fields

        protected Window _view;

        #endregion

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}

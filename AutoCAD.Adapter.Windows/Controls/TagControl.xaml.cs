using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ICA.AutoCAD.Adapter.Windows.Controls
{
    /// <summary>
    /// Interaction logic for TagControl.xaml
    /// </summary>
    public partial class TagControl : UserControl, INotifyPropertyChanged
    {
        public TagControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(object),
                typeof(TagControl)
                );
        public object Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set 
            {
                _isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

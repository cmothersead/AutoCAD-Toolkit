using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ICA.AutoCAD.Adapter.Controls
{
    /// <summary>
    /// Interaction logic for TextPartControl.xaml
    /// </summary>
    public partial class TextPartControl : UserControl
    {
        public static readonly DependencyProperty ManufacturerProperty =
            DependencyProperty.Register(
                nameof(Manufacturer),
                typeof(string),
                typeof(TextPartControl));

        public string Manufacturer
        {
            get => (string)GetValue(ManufacturerProperty);
            set => SetValue(ManufacturerProperty, value);
        }

        public static readonly DependencyProperty PartProperty =
            DependencyProperty.Register(
                nameof(Part),
                typeof(string),
                typeof(TextPartControl));

        public string Part
        {
            get => (string)GetValue(PartProperty);
            set => SetValue(PartProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool),
                typeof(TextPartControl));
        public bool? IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }
        public TextPartControl()
        {
            InitializeComponent();
        }
    }
}

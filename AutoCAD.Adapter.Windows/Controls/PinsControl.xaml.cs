using System.Windows;
using System.Windows.Controls;

namespace ICA.AutoCAD.Adapter.Windows.Controls
{
    /// <summary>
    /// Interaction logic for PinsControl.xaml
    /// </summary>
    public partial class PinsControl : UserControl
    {
        public static readonly DependencyProperty Pin1Property =
            DependencyProperty.Register(
                nameof(Pin1),
                typeof(string),
                typeof(PinsControl));
        public string Pin1
        {
            get => (string)GetValue(Pin1Property);
            set => SetValue(Pin1Property, value);
        }

        public static readonly DependencyProperty Pin2Property =
            DependencyProperty.Register(
                nameof(Pin2),
                typeof(string),
                typeof(PinsControl));
        public string Pin2
        {
            get => (string)GetValue(Pin2Property);
            set => SetValue(Pin2Property, value);
        }
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool),
                typeof(PinsControl));
        public bool? IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public static readonly DependencyProperty CharacterCasingProperty =
            DependencyProperty.Register(
                nameof(CharacterCasing),
                typeof(CharacterCasing),
                typeof(PinsControl));
        public CharacterCasing CharacterCasing
        {
            get => (CharacterCasing)GetValue(CharacterCasingProperty);
            set => SetValue(CharacterCasingProperty, value);
        }
        public PinsControl()
        {
            InitializeComponent();
        }
    }
}

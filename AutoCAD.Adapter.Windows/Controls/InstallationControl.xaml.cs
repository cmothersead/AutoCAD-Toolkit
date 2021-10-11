using System.Windows;
using System.Windows.Controls;

namespace ICA.AutoCAD.Adapter.Windows.Controls
{
    /// <summary>
    /// Interaction logic for InstallationControl.xaml
    /// </summary>
    public partial class InstallationControl : UserControl
    {
        public static readonly DependencyProperty InstallationProperty =
            DependencyProperty.Register(
                nameof(Installation),
                typeof(string),
                typeof(InstallationControl));
        public string Installation
        {
            get => (string)GetValue(InstallationProperty);
            set => SetValue(InstallationProperty, value);
        }

        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register(
                nameof(Location),
                typeof(string),
                typeof(InstallationControl));
        public string Location
        {
            get => (string)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool),
                typeof(InstallationControl));
        public bool? IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public static readonly DependencyProperty CharacterCasingProperty =
            DependencyProperty.Register(
                nameof(CharacterCasing),
                typeof(CharacterCasing),
                typeof(InstallationControl));
        public CharacterCasing CharacterCasing
        {
            get => (CharacterCasing)GetValue(CharacterCasingProperty);
            set => SetValue(CharacterCasingProperty, value);
        }

        public InstallationControl()
        {
            InitializeComponent();
        }
    }
}

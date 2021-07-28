using System.Windows;
using System.Windows.Controls;

namespace ICA.AutoCAD.Adapter.Controls
{
    /// <summary>
    /// Interaction logic for TagControl.xaml
    /// </summary>
    public partial class ChildTagControl : UserControl
    {
        public ChildTagControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(object),
                typeof(ChildTagControl)
                );
        public object Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Tag_TextBox.IsEnabled = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Tag_TextBox.IsEnabled = false;
        }
    }
}

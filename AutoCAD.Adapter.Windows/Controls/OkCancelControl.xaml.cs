using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ICA.AutoCAD.Adapter.Windows.Controls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class OkCancelControl : UserControl
    {
        public OkCancelControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty OkCommandProperty =
            DependencyProperty.Register(
        nameof(OkCommand),
        typeof(ICommand),
        typeof(OkCancelControl));
        public ICommand OkCommand
        {
            get => (ICommand)GetValue(OkCommandProperty);
            set => SetValue(OkCommandProperty, value);
        }
    }
}

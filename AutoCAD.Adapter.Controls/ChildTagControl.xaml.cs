using System;
using System.Windows;
using System.Windows.Controls;

namespace ICA.AutoCAD.Adapter.Controls
{
    /// <summary>
    /// Interaction logic for TagControl.xaml
    /// </summary>
    public partial class ChildTagControl : UserControl
    {
        #region Properties

        #region Public Properties

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(object),
                typeof(ChildTagControl));
        public object Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(object),
                typeof(ChildTagControl));
        public object Command
        {
            get => GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool),
                typeof(ChildTagControl));
        public bool? IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        #endregion

        #endregion

        #region Constructors

        public ChildTagControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Events

        public event EventHandler Checked;
        private void Hidden_Checkbox_Checked(object sender, RoutedEventArgs e) => Checked?.Invoke(this, e);

        public event EventHandler Unchecked;
        private void Hidden_Checkbox_Unchecked(object sender, RoutedEventArgs e) => Unchecked?.Invoke(this, e);

        #endregion
    }
}
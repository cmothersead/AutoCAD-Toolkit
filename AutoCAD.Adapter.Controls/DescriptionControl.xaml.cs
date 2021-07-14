using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace ICA.AutoCAD.Adapter.Controls
{
    /// <summary>
    /// Interaction logic for DescriptionControl.xaml
    /// </summary>
    public partial class DescriptionControl : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(DescriptionControl));
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool),
                typeof(DescriptionControl));
        public bool? IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public event EventHandler Checked;
        private void Description_Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            Checked?.Invoke(this, e);
        }

        public event EventHandler Unchecked;
        private void Description_Checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            Unchecked?.Invoke(this, e);
        }

        public DescriptionControl()
        {
            InitializeComponent();
        }

        public bool FocusItem(int index)
        {
            ContentPresenter c = Description_ItemsControl.ItemContainerGenerator.ContainerFromIndex(index) as ContentPresenter;
            c.ApplyTemplate();
            TextBox textBox = c.ContentTemplate.FindName("Value_TextBox", c) as TextBox;
            textBox.Focus();
            return true;
        }
    }
}

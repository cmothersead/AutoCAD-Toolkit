using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ICA.AutoCAD.Adapter.Controls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class DescriptionControl : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(DescriptionControl));
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public bool? IsChecked
        {
            get => Description_Checkbox.IsChecked;
            set => Description_Checkbox.IsChecked = value;
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

        public event TextChangedEventHandler TextChanged;
        /// <summary>
        /// Temporary workaround to add/remove empty description lines
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged?.Invoke(this, e);

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

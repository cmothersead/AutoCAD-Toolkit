using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ICA.AutoCAD.Adapter.Windows.Controls
{
    /// <summary>
    /// Interaction logic for TitleBlockPropertyControl.xaml
    /// </summary>
    public partial class TitleBlockPropertyControl : UserControl
    {

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(string),
                typeof(TitleBlockPropertyControl),
                new FrameworkPropertyMetadata(default(string),
                                              FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                              ValueChangedCallback));
        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public TitleBlockPropertyControl()
        {
            InitializeComponent();
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem selected = e.Source as MenuItem;
            if(selected.Name != "Custom")
            {
                Value = $"${Regex.Replace(selected.Name.Replace('_', '.'), @"(\d)\b", @"[$1]")}";
                return;
            }

            TextBox custom = FindName("CustomTextBox") as TextBox;
            Value = custom.Text;
        }

        private void CustomTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox custom = FindName("CustomTextBox") as TextBox;
            Value = custom.Text;
        }

        private static void ValueChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            TitleBlockPropertyControl sender = (TitleBlockPropertyControl)dependencyObject;
            string name = sender.Value.Replace('.', '_')
                                      .Replace("[", string.Empty)
                                      .Replace("]", string.Empty)
                                      .Replace("$", string.Empty);

            if (!(sender.FindName(name) is MenuItem selected))
                selected = sender.FindName("Custom") as MenuItem;
            
            MenuItem main = sender.FindName("MainItem") as MenuItem;
            TextBox custom = sender.FindName("CustomTextBox") as TextBox;

            main.Header = selected.Header;

            if (selected.Name == "Custom")
            {
                custom.Visibility = Visibility.Visible;
                custom.Text = sender.Value;
                return;
            }

            custom.Visibility = Visibility.Collapsed;
        }
    }
}

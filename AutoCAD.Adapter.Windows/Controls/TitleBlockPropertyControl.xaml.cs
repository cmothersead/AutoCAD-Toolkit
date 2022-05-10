using System;
using System.Collections.Generic;
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
        public TitleBlockPropertyControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(string),
                typeof(TitleBlockPropertyControl));
        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set
            {
                SetValue(ValueProperty, value);
                string name = value.Replace('.', '_')
                                   .Replace("[", string.Empty)
                                   .Replace("]", string.Empty)
                                   .Replace("$", string.Empty);
                MenuItem selected = FindName(name) as MenuItem;
                if (selected != null)
                {
                    MenuItem main = FindName("MainItem") as MenuItem;
                    main.Header = selected.Header;
                }

                TextBox custom = FindName("CustomTextBox") as TextBox;
                if (name == "Custom")
                    custom.Visibility = Visibility.Visible;
                else
                    custom.Visibility = Visibility.Collapsed;
            }
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem selected = e.Source as MenuItem;
            Value = $"${Regex.Replace(selected.Name.Replace('_', '.'), @"(\d)\b", @"[$1]")}";
        }
    }
}

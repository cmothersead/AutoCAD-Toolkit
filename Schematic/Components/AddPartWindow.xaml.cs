using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ICA.Schematic.Data;

namespace ICA.Schematic.Components
{
    /// <summary>
    /// Interaction logic for AddPartWindow.xaml
    /// </summary>
    public partial class AddPartWindow : Window
    {
        public Family Family
        {
            get { return Family_ComboBox.SelectedItem as Family; }
            set { Family_ComboBox.SelectedItem = value; }
        }
        public Manufacturer Manufacturer
        {
            get { return Manufacturer_ComboBox.SelectedItem as Manufacturer; }
            set { Manufacturer_ComboBox.SelectedItem = value; }
        }

        public AddPartWindow(Family family, ObservableCollection<Manufacturer> manufacturerSource, Manufacturer manufacturer, string partNumber)
        {
            InitializeComponent();
            Family_ComboBox.ItemsSource = new ObservableCollection<Family> { family };
            Family = family;
            Family_ComboBox.IsEnabled = false;
            Manufacturer_ComboBox.ItemsSource = manufacturerSource;
            Manufacturer = manufacturer;
            Part_TextBox.Text = partNumber;
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}

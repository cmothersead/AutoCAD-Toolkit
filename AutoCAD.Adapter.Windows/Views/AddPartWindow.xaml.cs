using System.Collections.ObjectModel;
using System.Windows;
using ICA.Schematic.Data;

namespace ICA.AutoCAD.Adapter.Windows.Views
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

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ICA.Schematic.Data;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ICA.Schematic.Components.EditWindowControls
{
    /// <summary>
    /// Interaction logic for PartControl.xaml
    /// </summary>
    public partial class PartControl : UserControl
    {
        public Family Family { get; set; }
        public Manufacturer Manufacturer
        {
            get { return Manufacturer_ComboBox.SelectedItem as Manufacturer; }
            set { Manufacturer_ComboBox.SelectedItem = value; }
        }
        public ObservableCollection<Manufacturer> Manufacturers
        {
            get { return (ObservableCollection<Manufacturer>)Manufacturer_ComboBox.ItemsSource; }
            set { Manufacturer_ComboBox.ItemsSource = value; }
        }
        public Part Part
        {
            get { return Part_ComboBox.SelectedItem as Part; }
            set { Part_ComboBox.SelectedItem = value; }
        }
        public bool? IsChecked
        {
            get { return PartNumber_Checkbox.IsChecked; }
            set { PartNumber_Checkbox.IsChecked = value; }
        }

        public PartControl()
        {
            InitializeComponent();
        }

        public async void Initialize(string family, string manufacturer, string part)
        {
            try
            {
                Family = await FamilyProcessor.GetFamilyAsync(family);

                var manufacturersList = await ManufacturerProcessor.GetManufacturersAsync(Family.FamilyId);
                foreach (var item in manufacturersList)
                {
                    item.ManufacturerName = item.ManufacturerName.ToUpper();
                }
                Manufacturer_ComboBox.SelectionChanged -= Manufacturer_ComboBox_SelectionChanged;
                Manufacturer_ComboBox.ItemsSource = manufacturersList;
                Manufacturer = manufacturersList.Single(m => m.ManufacturerName == manufacturer);
                Manufacturer_ComboBox.SelectionChanged += Manufacturer_ComboBox_SelectionChanged;

                var partsList = await PartProcessor.GetPartNumbersAsync(Family.FamilyId, Manufacturer.ManufacturerId);
                Part_ComboBox.ItemsSource = partsList;
                Part = partsList.Single(p => p.PartNumber == part);
                Part_ComboBox.IsEnabled = true;
            }
            catch (System.Net.Http.HttpRequestException)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Unable to connect to parts list");
            }
            catch (InvalidOperationException)
            {
                if (MessageBox.Show("Part data does not match existing part. Create new?", "Invalid Part Data", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Part newPart = await AddPart(Family, Manufacturer, part);
                    if (newPart != null)
                    {
                        await LoadManufacturers(newPart.FamilyId);
                        if (Manufacturer.ManufacturerId != newPart.ManufacturerId)
                        {
                            Manufacturer_ComboBox.SelectionChanged -= Manufacturer_ComboBox_SelectionChanged;
                            Manufacturer_ComboBox.SelectedValue = newPart.ManufacturerId;
                            Manufacturer_ComboBox.SelectionChanged += Manufacturer_ComboBox_SelectionChanged;
                        }
                        await LoadParts(newPart.FamilyId, newPart.ManufacturerId);
                        Part_ComboBox.SelectedValue = newPart.PartId;
                    }
                }
            }
        }

        private async void Manufacturer_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Manufacturer != null)
            {
                await LoadParts(Family.FamilyId, Manufacturer.ManufacturerId);
                Part_ComboBox.IsEnabled = true;
            }
            else
            {
                Part_ComboBox.IsEnabled = false;
            }
            
        }

        public async Task LoadParts(int familyId, int manufacturerId)
        {
            Part_ComboBox.ItemsSource = await PartProcessor.GetPartNumbersAsync(familyId, manufacturerId);
        }

        public async Task LoadManufacturers(int familyId)
        {
            var manufacturersList = await ManufacturerProcessor.GetManufacturersAsync(familyId);
            foreach (var item in manufacturersList)
            {
                item.ManufacturerName = item.ManufacturerName.ToUpper();
            }
            Manufacturer_ComboBox.ItemsSource = manufacturersList;
        }

        private void ManufacturerDefault_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Manufacturer_ComboBox.IsEnabled = false;
            if ((int)Manufacturer_ComboBox.SelectedValue != 5)
            {
                Manufacturer_ComboBox.SelectedValue = 5;
            }
        }

        private void ManufacturerDefault_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Manufacturer_ComboBox.IsEnabled = true;
        }

        private async void AddPart_Button_Click(object sender, RoutedEventArgs e)
        {
            Part newPart = await AddPart(Family, Manufacturer);
            if ( newPart != null)
            {
                Manufacturer_ComboBox.SelectionChanged -= Manufacturer_ComboBox_SelectionChanged;
                await LoadManufacturers(newPart.FamilyId);
                Manufacturer_ComboBox.SelectedValue = newPart.ManufacturerId;
                Manufacturer_ComboBox.SelectionChanged += Manufacturer_ComboBox_SelectionChanged;
                await LoadParts(newPart.FamilyId, newPart.ManufacturerId);
                Part_ComboBox.SelectedValue = newPart.PartId;
            }
        }

        private async Task<Part> AddPart(Family family, Manufacturer manufacturer, string partNumber = "")
        {
            AddPartWindow addPartWindow = new AddPartWindow(
                family,
                Manufacturers,
                manufacturer,
                partNumber
            );
            if ((bool)Application.ShowModalWindow(addPartWindow))
            {
                return await PartProcessor.CreatePartAsync(addPartWindow.Family.FamilyId, addPartWindow.Manufacturer.ManufacturerId, addPartWindow.Part_TextBox.Text);
            }

            return null;
        }
    }
}

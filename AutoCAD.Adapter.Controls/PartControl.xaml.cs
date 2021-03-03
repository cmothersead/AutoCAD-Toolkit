using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoCAD.Adapter.Controls
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
        public IPart Part
        {
            get { return Part_ComboBox.SelectedItem as Part; }
            set { Part_ComboBox.SelectedItem = value; }
        }
        public ObservableCollection<IPart> Parts
        {
            get { return (ObservableCollection<IPart>)Part_ComboBox.ItemsSource; }
            set { Part_ComboBox.ItemsSource = value; }
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

        public void Initialize(string family, string manufacturer, string part)
        {
            Manufacturer_ComboBox.SelectionChanged -= Manufacturer_ComboBox_SelectionChanged;
            Manufacturers = new ObservableCollection<Manufacturer>
                {
                    new Manufacturer
                    {
                        ManufacturerName = manufacturer
                    }
                };
            Manufacturer = Manufacturers[0];
            Parts = new ObservableCollection<IPart>
                {
                    new Part
                    {
                        PartNumber = part
                    }
                };
            Part = Parts[0];
            Manufacturer_ComboBox.SelectionChanged += Manufacturer_ComboBox_SelectionChanged;
        }

        public async void Populate(string family, string manufacturer, string part)
        {
            Manufacturer_ComboBox.SelectionChanged -= Manufacturer_ComboBox_SelectionChanged;
            try
            {
                Family = await FamilyProcessor.GetFamilyAsync(family);

                Manufacturers = await LoadManufacturers(Family.FamilyId);
                Manufacturer = Manufacturers.Single(m => m.ManufacturerName == manufacturer);

                Parts = await LoadParts(Family.FamilyId, Manufacturer.ManufacturerId);
                Part = Parts.Single(p => p.PartNumber == part);

                Manufacturer_ComboBox.IsEnabled = true;
                Part_ComboBox.IsEnabled = true;
            }
            catch (HttpRequestException)
            {
                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Unable to connect to parts list");
            }
            catch (InvalidOperationException)
            {
                if (MessageBox.Show("Part data does not match existing part. Create new?", "Invalid Part Data", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    IPart newPart = await AddPart(Family, Manufacturer, part);
                    if (newPart != null)
                    {
                        await LoadManufacturers(newPart.FamilyId);
                        Manufacturer_ComboBox.SelectedValue = newPart.ManufacturerId;

                        await LoadParts(newPart.FamilyId, newPart.ManufacturerId);
                        Part_ComboBox.SelectedValue = newPart.PartId;
                    }
                }
            }
            Manufacturer_ComboBox.SelectionChanged += Manufacturer_ComboBox_SelectionChanged;
        }

        private async void Manufacturer_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Manufacturer != null)
            {
                await LoadParts(Family.FamilyId, Manufacturer.ManufacturerId);
                Part_ComboBox.IsEnabled = true;
            }
            else
            {
                Part_ComboBox.IsEnabled = false;
            }

        }

        public async Task<ObservableCollection<IPart>> LoadParts(int familyId, int manufacturerId)
        {
            return await PartProcessor.GetPartNumbersAsync(familyId, manufacturerId);
        }

        public async Task<ObservableCollection<Manufacturer>> LoadManufacturers(int familyId)
        {
            return await ManufacturerProcessor.GetManufacturersUppercaseAsync(familyId);
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
            IPart newPart = await AddPart(Family, Manufacturer);
            if (newPart != null)
            {
                Manufacturer_ComboBox.SelectionChanged -= Manufacturer_ComboBox_SelectionChanged;
                await LoadManufacturers(newPart.FamilyId);
                Manufacturer_ComboBox.SelectedValue = newPart.ManufacturerId;
                await LoadParts(newPart.FamilyId, newPart.ManufacturerId);
                Part_ComboBox.SelectedValue = newPart.PartId;
                Manufacturer_ComboBox.SelectionChanged += Manufacturer_ComboBox_SelectionChanged;
            }
        }

        private async Task<IPart> AddPart(Family family, Manufacturer manufacturer, string partNumber = "")
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

using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;

namespace ICA.AutoCAD.Adapter.Windows.Controls
{
    /// <summary>
    /// Interaction logic for PartControl.xaml
    /// </summary>
    public partial class PartControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty FamilyProperty =
            DependencyProperty.Register(
                nameof(Family),
                typeof(object),
                typeof(PartControl));

        public object Family
        {
            get => GetValue(FamilyProperty);
            set => SetValue(FamilyProperty, value);
        }

        public static readonly DependencyProperty SelectedManufacturerProperty =
            DependencyProperty.Register(
                nameof(SelectedManufacturer),
                typeof(object),
                typeof(PartControl));

        public object SelectedManufacturer
        {
            get => GetValue(SelectedManufacturerProperty);
            set => SetValue(SelectedManufacturerProperty, value);
        }

        public static readonly DependencyProperty SelectedPartProperty =
            DependencyProperty.Register(
                nameof(SelectedPart),
                typeof(object),
                typeof(PartControl));

        public object SelectedPart
        {
            get => GetValue(SelectedPartProperty);
            set => SetValue(SelectedPartProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool),
                typeof(PartControl));
        public bool? IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public PartControl()
        {
            InitializeComponent();
        }

        private void Manufacturer_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ManufacturerDefault_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Manufacturer_ComboBox.IsEnabled = false;
        }

        private void ManufacturerDefault_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Manufacturer_ComboBox.IsEnabled = true;
        }

        private void AddPart_Button_Click(object sender, RoutedEventArgs e)
        {
            //IPart newPart = await AddPart(Family, Manufacturer);
            //if (newPart != null)
            //{
            //    Manufacturer_ComboBox.SelectionChanged -= Manufacturer_ComboBox_SelectionChanged;
            //    await LoadManufacturers(newPart.FamilyId);
            //    Manufacturer_ComboBox.SelectedValue = newPart.ManufacturerId;
            //    await LoadParts(newPart.FamilyId, newPart.ManufacturerId);
            //    Part_ComboBox.SelectedValue = newPart.PartId;
            //    Manufacturer_ComboBox.SelectionChanged += Manufacturer_ComboBox_SelectionChanged;
            //}
        }
    }
}

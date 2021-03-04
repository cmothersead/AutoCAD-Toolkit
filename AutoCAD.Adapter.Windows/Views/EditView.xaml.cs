using System.Collections.ObjectModel;
using System.Windows;
using ICA.AutoCAD.Adapter.Windows.ViewModels;
using ICA.Schematic;

namespace ICA.AutoCAD.Adapter.Windows.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EditView : Window
    {
        public ParentSymbol Symbol;

        public EditView()
        {
            InitializeComponent();
        }

        public EditView(ParentSymbol symbol)
        {
            Symbol = symbol;
            InitializeComponent();
        }

        private void EditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //PopulateForm();
        }

        //private void PopulateForm()
        //{
        //    DescriptionControl.IsChecked = Symbol.DescriptionHidden;
        //    PartControl.IsChecked = Symbol.PartInfoHidden;
        //    InstallationControl.IsChecked = Symbol.InstallationHidden;
        //    TagControl.Text = Symbol.Tag;
        //    List<string> Description = new List<string>();
        //    foreach (string description in Symbol.Description)
        //    {
        //        Description.Add(description);
        //    }
        //    DescriptionControl.Text = Description;
        //    InstallationControl.Installation = Symbol.Enclosure;
        //    InstallationControl.Location = Symbol.Location;
        //    PartControl.Initialize(Symbol.Family, Symbol.ManufacturerName, Symbol.PartNumber);
        //}

        //private void Commit()
        //{
        //    Symbol.DescriptionHidden = (bool)DescriptionControl.IsChecked;
        //    Symbol.PartInfoHidden = (bool)PartControl.IsChecked;
        //    Symbol.InstallationHidden = (bool)InstallationControl.IsChecked;
        //    Symbol.Description = DescriptionControl.Text;
        //    Symbol.ManufacturerName = PartControl.Manufacturer.ManufacturerName;
        //    Symbol.PartNumber = PartControl.Part.PartNumber;
        //    Symbol.Enclosure = InstallationControl.Installation;
        //    Symbol.Location = InstallationControl.Location;
        //    Symbol.CollapseAttributeStack();
        //}

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            //Commit();
            this.Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

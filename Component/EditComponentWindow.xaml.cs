using System;
using System.Collections.Generic;
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
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;

namespace Component
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EditComponentWindow : Window
    {
        private catalogDataSet catalogDataSet;
        private catalogDataSetTableAdapters.ManufacturerTableAdapter ManufacturerTableAdapter = new catalogDataSetTableAdapters.ManufacturerTableAdapter();
        private catalogDataSetTableAdapters.CatalogTableAdapter CatalogTableAdapter = new catalogDataSetTableAdapters.CatalogTableAdapter();
        public ComponentInstance Component;

        public EditComponentWindow()
        {
            InitializeComponent();
        }

        private void EditComponentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            catalogDataSet = (catalogDataSet)FindResource("catalogDataSet");
            ManufacturerTableAdapter.Fill(catalogDataSet.Manufacturer, Component.Family.TextString);
            Tag_TextBox.Text = Component.Tag.TextString;
            Description1_TextBox.Text = Component.Desc[0].TextString;
            Description2_TextBox.Text = Component.Desc[1].TextString;
            Description3_TextBox.Text = Component.Desc[2].TextString;
            if(Component.Inst == null)
            {
                Installation_TextBox.IsEnabled = false;
            }
            else
            {
                Installation_TextBox.Text = Component.Inst.TextString;
            }
            if(Component.Loc == null)
            {
                Location_TextBox.IsEnabled = false;
            }
            else
            {
                Location_TextBox.Text = Component.Inst.TextString;
            }
            if (!String.IsNullOrEmpty(Component.Mfg.TextString))
            {
                Manufacturer_ComboBox.SelectedValue = Component.Mfg.TextString;
                FillCatalogTable();
                Catalog_ComboBox.IsEnabled = true;
            }
            if(!String.IsNullOrEmpty(Component.Cat.TextString))
            {
                Catalog_ComboBox.SelectedValue = Component.Cat.TextString;
            }
        }

        private void Manufacturer_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillCatalogTable();
            Catalog_ComboBox.IsEnabled = true;
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Description1_TextBox_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {

        }

        private void Manufacturer_ComboBox_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void FillCatalogTable()
        {
            catalogDataSet = (catalogDataSet)FindResource("catalogDataSet");
            CatalogTableAdapter.Fill(catalogDataSet.Catalog, Component.Family.TextString, (String)Manufacturer_ComboBox.SelectedValue);
        }
    }
}

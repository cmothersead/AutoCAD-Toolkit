using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
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

namespace PanelInsert
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class InsertDialog : Window
    {
        public InsertDialog()
        {
            InitializeComponent();
            ComponentCollection componentCollection = (ComponentCollection)this.Resources["components"];
            OleDbConnection con = Connect.ProjectDatabase();
            con.Open();

            string statement = "SELECT * FROM `COMP` WHERE `PAR1_CHLD2` = '1'";
            OleDbCommand cmd = new OleDbCommand(statement, con);
            OleDbDataReader odr = cmd.ExecuteReader();

            while (odr.Read())
            {
                componentCollection.Add(new PanelInsert.Component()
                {
                    DeviceNumber = odr["TAGNAME"].ToString(),
                    Manufacturer = odr["MFG"].ToString(),
                    CatalogNumber = odr["CAT"].ToString(),
                    Installation = odr["INST"].ToString(),
                    Location = odr["LOC"].ToString(),
                    Family = odr["FAMILY"].ToString()
                }); ;
            }

            InstList.ItemsSource = componentCollection.InstallationList;
            InstList.DisplayMemberPath = "Name";

            LocList.ItemsSource = componentCollection.LocationList;
            LocList.DisplayMemberPath = "Name";
        }

        private void InstList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComponentCollection componentCollection = (ComponentCollection)this.Resources["components"];
            componentCollection.SelectedInstallations = new System.Collections.Generic.List<string>();
            foreach (ComponentCollection.Frequency inst in InstList.SelectedItems)
            {
                componentCollection.SelectedInstallations.Add(inst.Name);
            }
            LocList.ItemsSource = componentCollection.LocationList;
            CollectionViewSource cvsComponents = (CollectionViewSource)this.Resources["cvsComponents"];
            cvsComponents.View.Refresh();
        }

        private void LocList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComponentCollection componentCollection = (ComponentCollection)this.Resources["components"];
            componentCollection.SelectedLocations = new System.Collections.Generic.List<string>();
            foreach (ComponentCollection.Frequency loc in LocList.SelectedItems)
            {
                componentCollection.SelectedLocations.Add(loc.Name);
            }
            CollectionViewSource cvsComponents = (CollectionViewSource)this.Resources["cvsComponents"];
            cvsComponents.View.Refresh();
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            ComponentCollection componentCollection = (ComponentCollection)this.Resources["components"];
            Component component = (Component)e.Item;
            if (componentCollection.SelectedInstallations.Contains(component.Installation) & componentCollection.SelectedLocations.Contains(component.Location))
            {
                e.Accepted = true;
            }
            else
            {
                e.Accepted = false;
            }
        }
    }
}

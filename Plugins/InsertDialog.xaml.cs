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
            FillCollection(componentCollection);
        }

        private void FillCollection(ComponentCollection componentCollection)
        {
            OleDbConnection con = Connect.ProjectDatabase();
            con.Open();

            string statement = "SELECT * FROM `COMP` WHERE `PAR1_CHLD2` = '1'";
            OleDbCommand cmd = new OleDbCommand(statement, con);
            OleDbDataReader odr = cmd.ExecuteReader();

            while (odr.Read())
            {
                PanelInsert.Component component = new PanelInsert.Component();
                component.deviceNumber = odr["TAGNAME"].ToString();
                component.manufacturer = odr["MFG"].ToString();
                component.catalogNumber = odr["CAT"].ToString();
                component.installation = odr["INST"].ToString();
                component.location = odr["LOC"].ToString();
                componentCollection.Add(component);
            }
        }

    }
}

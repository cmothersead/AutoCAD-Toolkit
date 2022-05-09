using System.Collections;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PanelInsert
{

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
                    DeviceTag = odr["TAGNAME"].ToString(),
                    Manufacturer = odr["MFG"].ToString(),
                    CatalogNumber = odr["CAT"].ToString(),
                    Installation = odr["INST"].ToString(),
                    Location = odr["LOC"].ToString(),
                    Family = odr["FAMILY"].ToString()
                });
            }

            InstList.ItemsSource = componentCollection.InstallationList;
            InstList.DisplayMemberPath = "Name";

            LocList.ItemsSource = componentCollection.LocationList;
            LocList.DisplayMemberPath = "Name";
        }

        void InsertDialog_Loaded(object sender, RoutedEventArgs e)
        {
            CollectionViewSource cvsComponents = (CollectionViewSource)this.Resources["cvsComponents"];
            ListCollectionView view = (ListCollectionView)cvsComponents.View;
            view.CustomSort = new AlphanumComparator();
        }

        public class AlphanumComparator : IComparer
        {
            public int Compare(object x, object y)
            {
                string s1 = x.ToString();
                if (s1 == null)
                {
                    return 0;
                }
                string s2 = y.ToString();
                if (s2 == null)
                {
                    return 0;
                }

                int len1 = s1.Length;
                int len2 = s2.Length;
                int marker1 = 0;
                int marker2 = 0;

                // Walk through two the strings with two markers.
                while (marker1 < len1 && marker2 < len2)
                {
                    char ch1 = s1[marker1];
                    char ch2 = s2[marker2];

                    // Some buffers we can build up characters in for each chunk.
                    char[] space1 = new char[len1];
                    int loc1 = 0;
                    char[] space2 = new char[len2];
                    int loc2 = 0;

                    // Walk through all following characters that are digits or
                    // characters in BOTH strings starting at the appropriate marker.
                    // Collect char arrays.
                    do
                    {
                        space1[loc1++] = ch1;
                        marker1++;

                        if (marker1 < len1)
                        {
                            ch1 = s1[marker1];
                        }
                        else
                        {
                            break;
                        }
                    } while (char.IsDigit(ch1) == char.IsDigit(space1[0]));

                    do
                    {
                        space2[loc2++] = ch2;
                        marker2++;

                        if (marker2 < len2)
                        {
                            ch2 = s2[marker2];
                        }
                        else
                        {
                            break;
                        }
                    } while (char.IsDigit(ch2) == char.IsDigit(space2[0]));

                    // If we have collected numbers, compare them numerically.
                    // Otherwise, if we have strings, compare them alphabetically.
                    string str1 = new string(space1);
                    string str2 = new string(space2);

                    int result;

                    if (char.IsDigit(space1[0]) && char.IsDigit(space2[0]))
                    {
                        int thisNumericChunk = int.Parse(str1);
                        int thatNumericChunk = int.Parse(str2);
                        result = thisNumericChunk.CompareTo(thatNumericChunk);
                    }
                    else
                    {
                        result = str1.CompareTo(str2);
                    }

                    if (result != 0)
                    {
                        return result;
                    }
                }
                return len1 - len2;
            }
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

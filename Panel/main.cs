using Autodesk.AutoCAD.Runtime;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Linq;

namespace PanelInsert
{
    public class Connect
    {
        public static OleDbConnection ProjectDatabase()
        {
            var connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=''";
            return new OleDbConnection(connectionString);
        }

        [CommandMethod("TESTDIALOG")]
        public static void DialogTest()
        {
            var dialog = new InsertDialog();
            var result = Autodesk.AutoCAD.ApplicationServices.Application.ShowModalWindow(dialog);
        }
    }

    public class Component
    {
        public string DeviceTag { get; set; }
        public string Manufacturer { get; set; }
        public string CatalogNumber { get; set; }
        public string Installation { get; set; }
        public string Location { get; set; }
        public string Family { get; set; }

        public override string ToString()
        {
            return DeviceTag;
        }
    }

    public class ComponentCollection : ObservableCollection<Component>
    {
        public class Frequency
        {
            public string Name { get; set; }
            public int Count { get; set; }
        }

        public List<string> SelectedInstallations { get; set; } = new List<string>();
        public List<string> SelectedLocations { get; set; } = new List<string>();

        public IEnumerable<Frequency> InstallationList
        {
            get
            {
                return from component in this
                       group component by component.Installation into installation
                       orderby installation.Count() descending
                       where !ExcludedInstallations.Contains(installation.Key)
                       select new Frequency
                       {
                           Name = installation.Key,
                           Count = installation.Count()
                       };

            }
        }

        public List<string> ExcludedInstallations = new List<string>
        {
            "",
            "CABLE",
            "MOTOR",
            "SENSOR",
            "VALVE PACK"
        };

        public IEnumerable<Frequency> LocationList
        {
            get
            {
                var currentInstallation = from component in this
                                          where this.SelectedInstallations.Contains(component.Installation)
                                          select component;

                return from component in currentInstallation
                       group component by component.Location into location
                       orderby location.Count() descending
                       where !ExcludedLocations.Contains(location.Key)
                       select new Frequency
                       {
                           Name = location.Key,
                           Count = location.Count()
                       };

            }
        }

        public List<string> ExcludedLocations = new List<string>
        {
            ""
        };
    }
}

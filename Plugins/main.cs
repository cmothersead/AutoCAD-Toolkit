using System;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Forms;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.Electrical.Project;

namespace PanelInsert
{
    public class Connect
    {
        public static OleDbConnection ProjectDatabase()
        {
            var connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source='" + ProjectManager.GetInstance().GetActiveProject().GetDbFullPath() + "'";
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
        public string deviceNumber { get; set; }
        public string manufacturer { get; set; }
        public string catalogNumber { get; set; }
        public string installation { get; set; }
        public string location { get; set; }
    }

    public class ComponentCollection : ObservableCollection<Component>
    {

    }
}

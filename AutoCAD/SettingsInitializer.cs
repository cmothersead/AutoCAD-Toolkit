using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Interop;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD
{
    public class SettingsInitializer : IExtensionApplication
    {
        public void Initialize()
        {
            List<string> test = Settings.GetSupportPaths();
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }

        
    }

    public class Settings
    {
        public static List<string> GetSupportPaths()
        {
            return ((AcadApplication)Application.AcadApplication).Preferences.Files.SupportPath.Split(';').ToList();
        }
    }
}

using Autodesk.AutoCAD.Runtime;
using System;

namespace ICA.AutoCAD
{
    public class SettingsInitializer : IExtensionApplication
    {
        public void Initialize()
        {
            //List<string> test = Settings.GetSupportPaths();
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }

        
    }

    public class Settings
    {
        //public static List<string> GetSupportPaths()
        //{
        //}
    }
}

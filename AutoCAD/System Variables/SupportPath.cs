using Autodesk.AutoCAD.ApplicationServices;
using System.Collections.Generic;

namespace ICA.AutoCAD
{
    public class SupportPath
    {
        public static List<string> GetDefault()
        {
            dynamic application = Application.AcadApplication;
            string path = application.Path;
            dynamic preferences = Application.Preferences;
            preferences.Output.AutomaticPlotLog = false;
            return new List<string>
            {
                "C:\\Program Files\\Autodesk\\AutoCAD 2022\\Acade\\",
                "C:\\Program Files\\Autodesk\\AutoCAD 2022\\Acade\\en-US\\",
                "C:\\Program Files\\Autodesk\\AutoCAD 2022\\Acade\\Help\\en-US\\Help\\",
                "C:\\Program Files\\Autodesk\\AutoCAD 2022\\Acade\\Support\\en-US\\",
                "C:\\Program Files\\Autodesk\\AutoCAD 2022\\Acade\\Support\\en-US\\Shared\\",
            };

        }


    }
}

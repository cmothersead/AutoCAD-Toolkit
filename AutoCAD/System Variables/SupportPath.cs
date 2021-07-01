using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD
{
    public class SupportPath
    {
        public static List<string> GetDefault ()
        {
            string localRootPrefix = Application.GetSystemVariable("LOCALROOTPREFIX") as string;
            string myDocumentsPrefix = Application.GetSystemVariable("MYDOCUMENTSPREFIX") as string;
            string roamableRootPrefix = Application.GetSystemVariable("ROAMABLEROOTPREFIX") as string;
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

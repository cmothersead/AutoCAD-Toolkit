using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD
{
    public static class SystemVariables
    {
        public static bool GridSnap
        {
            get
            {
                if ((short)Application.GetSystemVariable("SNAPMODE") == 1)
                    return true;
                else
                    return false;
            }
            set
            {
                if (value)
                    Application.SetSystemVariable("SNAPMODE", 1);
                else
                    Application.SetSystemVariable("SNAPMODE", 0);
            }
        }
    }
}

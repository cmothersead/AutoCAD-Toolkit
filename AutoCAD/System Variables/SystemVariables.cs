using Autodesk.AutoCAD.ApplicationServices;
using System;

namespace ICA.AutoCAD
{
    public static class SystemVariables
    {
        public static bool GridSnap
        {
            get => (short)Application.GetSystemVariable("SNAPMODE") == 1;
            set => Application.SetSystemVariable("SNAPMODE", value ? 1 : 0);
        }

        public static ObjectSnap? ObjectSnap
        {
            get => (ObjectSnap?)Convert.ToInt32(Application.GetSystemVariable("OSMODE"));
            set => Application.SetSystemVariable("OSMODE", (short)(value ?? 0));
        }
    }
}

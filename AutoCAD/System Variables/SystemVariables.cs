using Autodesk.AutoCAD.ApplicationServices;

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

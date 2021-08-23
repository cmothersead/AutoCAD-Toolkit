using Autodesk.AutoCAD.ApplicationServices;

namespace ICA.AutoCAD
{
    public static class SystemVariables
    {
        public static bool GridSnap
        {
            get => (short)Application.GetSystemVariable("SNAPMODE") == 1;
            set => Application.SetSystemVariable("SNAPMODE", value ? 1 : 0);
        }
    }
}

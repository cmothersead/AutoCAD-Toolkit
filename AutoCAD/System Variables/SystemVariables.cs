using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD
{
    public static class SystemVariables
    {
        public static bool Backup
        {
            get => (short)Application.GetSystemVariable("ISAVEBAK") == 1;
            set => Application.SetSystemVariable("ISAVEBAK", value ? 1 : 0);
        }

        public static bool FileDialog
        {
            get => (short)Application.GetSystemVariable("FILEDIA") == 1;
            set => Application.SetSystemVariable("FILEDIA", value ? 1 : 0);
        }

        public static GridDisplay GridDisplay
        {
            get => (GridDisplay)Convert.ToInt32(Application.GetSystemVariable("GRIDDISPLAY"));
            set => Application.SetSystemVariable("GRIDDISPLAY", (short)value);
        }

        public static bool GridSnap
        {
            get => (short)Application.GetSystemVariable("SNAPMODE") == 1;
            set => Application.SetSystemVariable("SNAPMODE", value ? 1 : 0);
        }

        public static short LockFade
        {
            get => (short)Application.GetSystemVariable("LAYLOCKFADECTL");
            set => Application.SetSystemVariable("LAYLOCKFADECTL", value);
        }

        public static ObjectSnap ObjectSnap
        {
            get => (ObjectSnap)Convert.ToInt32(Application.GetSystemVariable("OSMODE"));
            set => Application.SetSystemVariable("OSMODE", (short)value);
        }

        public static bool PDFComments
        {
            get => (short)Application.GetSystemVariable("PDFSHX") == 1;
            set => Application.SetSystemVariable("PDFSHX", value ? 1 : 0);
        }

        public static bool PDFFontAlt
        {
            get => (short)Application.GetSystemVariable("AEPDFFONTALT") == 1;
            set => Application.SetSystemVariable("AEPDFFONTALT", value ? 1 : 0);
        }

        public static List<string> TrustedPaths
        {
            get => ((string)Application.GetSystemVariable("TRUSTEDPATHS")).Split(';').ToList();
            set => Application.SetSystemVariable("TRUSTEDPATHS", String.Join(";", value));
        }
    }
}

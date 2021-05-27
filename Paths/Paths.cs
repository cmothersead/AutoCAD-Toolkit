using System;
using System.Collections.Generic;
using System.IO;

namespace ICA.AutoCAD.Directory
{
    public class Paths
    {
        private static string _main;
        public static string Main
        {
            get
            {
                if (_main is not null)
                    return _main;

                string[] files = System.IO.Directory.GetFiles(@"C:\", "acad.lsp", new EnumerationOptions()
                { 
                    IgnoreInaccessible = true,
                    RecurseSubdirectories = true
                });
                foreach(string file in files)
                {
                    string directory = Path.GetFullPath($"{Path.GetDirectoryName(file)}\\..\\");
                    if (IsLibrary(directory))
                    {
                        _main = directory;
                        return directory;
                    }
                }
                return null;
            }
        }

        private static bool IsLibrary (string path)
        {
            List<string> subfolders = new()
            {
                "libs",
                "plugins",
                "templates",
                "libs\\schematic",
                "libs\\panel"
            };
            foreach (string subfolder in subfolders)
                if (!System.IO.Directory.Exists($"{path}\\{subfolder}"))
                    return false;
            return true;
        }

    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ICA.AutoCAD.IO
{
    public class Paths
    {
        #region Private Properties

        private static EnumerationOptions DefaultSearch => new()
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = true,
        };

        #endregion

        #region Public Properties

        private static string _main;
        /// <summary>
        /// Main ICA AutoCAD Extension directory
        /// </summary>
        public static string Main
        {
            get
            {
                if (_main is not null)
                    return _main;

                string[] files = Directory.GetFiles(@"C:\", "acad.lsp", DefaultSearch);

                _main = Directory.GetFiles(@"C:\", "acad.lsp", DefaultSearch)
                                 .FirstOrDefault(file => IsLibrary(Path.GetFullPath($"{Path.GetDirectoryName(file)}\\..\\")));

                return _main;
            }
        }

        /// <summary>
        /// ICA Custom Libraries directory
        /// </summary>
        public static string Libraries => Path.GetFullPath($"{Main}\\libs");

        /// <summary>
        /// ICA schematic library directory. Contains schematic block drawings
        /// </summary>
        public static string SchematicLibrary => Path.GetFullPath($"{Libraries}\\schematic\\library");

        /// <summary>
        /// ICA schematic menu directory. Contains menu file and images
        /// </summary>
        public static string SchematicMenu => Path.GetFullPath($"{Libraries}\\schematic\\menu");

        /// <summary>
        /// ICA panel library directory. Contains panel representation drawings
        /// </summary>
        public static string PanelLibrary => Path.GetFullPath($"{Libraries}\\panel\\library");

        /// <summary>
        /// ICA panel menu directory. Contains menu file and images
        /// </summary>
        public static string PanelMenu => Path.GetFullPath($"{Libraries}\\panel\\menu");

        /// <summary>
        /// ICA AutoCAD plugins directory. Contains code & libraries to add functionality to AutoCAD
        /// </summary>
        public static string Plugins => Path.GetFullPath($"{Main}\\plugins");

        /// <summary>
        /// ICA template directory. Contains custom AutoCAD & Office templates.
        /// </summary>
        public static string Templates => Path.GetFullPath($"{Main}\\templates");

        #endregion

        #region Private Methods

        private static bool IsLibrary(string path)
        {
            List<string> subfolders = new()
            {
                "libs",
                "plugins",
                "templates",
                "libs\\schematic",
                "libs\\panel"
            };
            return !subfolders.Any(subfolder => !Directory.Exists($"{path}\\{subfolder}"));
        }

        #endregion
    }
}

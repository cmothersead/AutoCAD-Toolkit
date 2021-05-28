using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ICA.AutoCAD.IO
{
    public class Paths
    {
        #region Public Properties

        /// <summary>
        /// Main ICA AutoCAD Extension directory
        /// </summary>
        private static string _main;
        public static string Main
        {
            get
            {
                if (_main != null)
                    return _main;

                List<string> searchDirectories = new List<string>
                {
                    Path.GetFullPath($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}//OneDrive - icacontrol.com//Electrical Library"),
                    Path.GetFullPath($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}//OneDrive - icacontrol.com"),
                    Path.GetFullPath($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}//icacontrol.com"),
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    @"C:\",
                };

                foreach (string searchDirectory in searchDirectories) 
                {
                    _main = FindLibrary(searchDirectory);
                    if (_main != null)
                        return _main;
                }

                throw new DirectoryNotFoundException("Unable to locate ICA AutoCAD directory");
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

        /// <summary>
        /// Searches subdirectories of <paramref name="basePath"/> for the structure of ICA's AutoCAD directory
        /// </summary>
        /// <param name="basePath">Top level directory to start the search</param>
        /// <returns></returns>
        private static string FindLibrary(string basePath)
        {
            string[] files = GetFiles(basePath, "acad.lsp");
            foreach (string file in files)
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

        /// <summary>
        /// Determines if the given directory contains the elements of ICA's AutoCAD directory
        /// </summary>
        /// <param name="path">Directory path to check</param>
        /// <returns></returns>
        private static bool IsLibrary(string path)
        {
            List<string> subfolders = new List<string>()
            {
                "libs",
                "plugins",
                "templates",
                "libs\\schematic",
                "libs\\panel"
            };
            foreach (string subfolder in subfolders)
                if (!Directory.Exists($"{path}\\{subfolder}"))
                    return false;
            return true;
        }

        /// <summary>
        /// Recursive application of <see cref="Directory.GetFiles(string, string)"/>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        private static string[] GetFiles(string path, string searchPattern)
        {
            List<string> results = new List<string>();
            try
            {
                results.AddRange(Directory.EnumerateFiles(path, searchPattern).ToList());
                foreach (string subdirectory in Directory.EnumerateDirectories(path))
                {
                    if (new DirectoryInfo(subdirectory).Attributes.HasFlag(FileAttributes.System))
                        continue;
                    results.AddRange(GetFiles(subdirectory, searchPattern).ToList());
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }
            return results.ToArray();
        }

        #endregion

        #region Public Methods

        public static string FindSchematic(string name)
        {
            try
            {
                if (!name.Contains(".dwg"))
                    name = $"{name}.dwg";
                return GetFiles(Paths.SchematicLibrary, name)[0];
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }


        }

        public static string FindPanel(string name)
        {
            try
            {
                if (!name.Contains(".dwg"))
                    name = $"{name}.dwg";
                return GetFiles(Paths.PanelLibrary, name)[0];
            }
            catch(IndexOutOfRangeException)
            {
                return null;
            }
            
        }

        #endregion
    }
}

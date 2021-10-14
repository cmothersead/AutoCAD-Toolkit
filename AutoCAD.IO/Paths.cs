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

                _main = searchDirectories.Select(searchDirectory => FindLibrary(searchDirectory))
                                         .FirstOrDefault(result => result != null);

                if (_main != null)
                    return _main;

                throw new DirectoryNotFoundException("Unable to locate ICA AutoCAD directory.");
            }
        }

        /// <summary>
        /// ICA Custom Libraries directory
        /// </summary>
        public static string Libraries => Path.GetFullPath($"{Main}\\libs");

        /// <summary>
        /// ICA schematic library directory. Contains schematic block drawings
        /// </summary>
        public static string SchematicLibrary => Path.GetFullPath($"{Libraries}\\dev\\schematic\\library");

        /// <summary>
        /// ICA schematic menu directory. Contains menu file and images
        /// </summary>
        public static string SchematicMenu => Path.GetFullPath($"{Libraries}\\dev\\schematic\\menu");

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

        /// <summary>
        /// ICA title block directory. Contains title block definition files.
        /// </summary>
        public static string TitleBlocks => Path.GetFullPath($"{Templates}\\title blocks");

        #endregion

        #region Private Methods

        /// <summary>
        /// Searches subdirectories of <paramref name="basePath"/> for the structure of ICA's AutoCAD directory
        /// </summary>
        /// <param name="basePath">Top level directory to start the search</param>
        /// <returns></returns>
        private static string FindLibrary(string basePath) => GetFiles(basePath, "acad.lsp").Select(file => Path.GetFullPath($"{Path.GetDirectoryName(file)}\\..\\"))
                                                                                            .FirstOrDefault(directory => IsLibrary(directory));

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
            return !subfolders.Any(subfolder => !Directory.Exists($"{path}\\{subfolder}"));
        }

        /// <summary>
        /// Recursive application of <see cref="Directory.GetFiles(string, string)"/>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        private static IEnumerable<string> GetFiles(string path, string searchPattern)
        {
            try
            {
                return Directory.EnumerateFiles(path, searchPattern).Union(Directory.EnumerateDirectories(path)
                                                                                    .Where(subdirectory => !new DirectoryInfo(subdirectory).Attributes.HasFlag(FileAttributes.System))
                                                                                    .SelectMany(subdirectory => GetFiles(subdirectory, searchPattern)));
            }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }
            return Enumerable.Empty<string>();
        }

        #endregion

        #region Public Methods

        public static string FindSchematic(string name)
        {
            try
            {
                if (!name.Contains(".dwg"))
                    name = $"{name}.dwg";
                return GetFiles(Paths.SchematicLibrary, name).FirstOrDefault();
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
                return GetFiles(Paths.PanelLibrary, name).FirstOrDefault();
            }
            catch(IndexOutOfRangeException)
            {
                return null;
            }
        }

        public static string GetRelativePath(string relativeTo, string path)
        {
            relativeTo = Path.GetFullPath(relativeTo);
            path = Path.GetFullPath(path);
            if (path.Contains(relativeTo))
                path = path.Replace(relativeTo + "\\", "");
            return path;
        }

        #endregion
    }
}

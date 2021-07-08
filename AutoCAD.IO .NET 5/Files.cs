using System.IO;

namespace ICA.AutoCAD.IO
{
    public class Files
    {
        #region Private Properties

        private static EnumerationOptions DefaultSearch => new()
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = true,
        };

        #endregion

        #region Public Methods

        public static string FindSchematic(string name)
        {
            return Directory.GetFiles(Paths.SchematicLibrary, name, DefaultSearch)[0];
        }

        public static string FindPanel(string name)
        {
            return Directory.GetFiles(Paths.PanelLibrary, name, DefaultSearch)[0];
        }

        #endregion
    }
}

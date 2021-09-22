using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ICA.AutoCAD.Adapter
{
    public class Drawing
    {
        #region Private Properties

        private Database _database;
        private Database Database
        {
            get
            {
                if (Project is null)
                    return null;

                if (_database is null)
                {
                    if (!File.Exists(FullPath))
                        throw new FileNotFoundException();

                    _database = new Database(false, true);
                    _database.ReadDwgFile(FullPath, FileOpenMode.OpenForReadAndAllShare, true, null);
                    _database.CloseInput(true);
                }

                return _database;
            }
        }

        #endregion

        #region Public Properties

        [XmlIgnore]
        public Uri FileUri => new Uri(Project.FileUri, $"{FileName}.dwg");
        [XmlIgnore]
        public Project Project { get; set; }
        public string FullPath => new Uri(Project.FileUri, FileUri).LocalPath;

        [XmlAttribute]
        public string FileName { get; set; }
        public List<string> Description
        {
            get => Database?.GetDescription();
            set => Database?.SetDescription(value);
        }
        [XmlAttribute]
        public string PageNumber
        {
            get => Database?.GetSheetNumber();
            set => Database?.SetSheetNumber(value);
        }

        [XmlIgnore]
        public DrawingSettings Settings { get; set; }

        #endregion

        #region Constructor

        public Drawing() { }

        #endregion

        #region Methods

        public static Drawing CreateFromTemplate(Project project, string templatePath, string fileName)
        {
            CloneDatabase(templatePath, $"{project.DirectoryUri.LocalPath}\\{fileName}.dwg");
            return new Drawing()
            {
                Project = project,
                FileName = fileName
            };
        }

        public static void CloneDatabase(string templatePath, string filePath)
        {
            using (Database database = new Database(false, true))
            {
                database.ReadDwgFile(templatePath, FileOpenMode.OpenForReadAndAllShare, true, null);
                database.CloseInput(true);
                database.SaveAs(filePath, DwgVersion.Current);
            }
        }

        public override string ToString() => FileName;

        #endregion
    }
}

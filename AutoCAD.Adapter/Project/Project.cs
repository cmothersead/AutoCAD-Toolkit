using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using ICA.AutoCAD.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace ICA.AutoCAD.Adapter
{
    public class Project
    {
        public enum DrawingType
        {
            Schematic,
            Panel,
            Reference
        }

        #region Public Properties

        [XmlIgnore]
        public Uri Uri { get; set; }

        public string Name { get; set; }
        public List<Drawing> Drawings { get; set; }
        public ProjectSettings Settings { get; set; }

        #endregion

        #region Constructors

        public Project() { }

        #endregion

        public void Run<TArgument>(Action<Database, TArgument> action, TArgument value)
        {
            foreach(Drawing drawing in Drawings)
            {
                Database database = Commands.LoadDatabase(drawing.Uri);
                if(Application.DocumentManager.Contains(drawing.Uri))
                {
                    using (DocumentLock doclock = Application.DocumentManager.GetDocument(database).LockDocument())
                        action(database, value);
                }
                else
                {
                    action(database, value);
                    database.SaveAs(database.OriginalFileName, DwgVersion.Current);
                }
            }
        }

        public Document AddPage(DrawingType type, string savePath)
        {
            Document newPage;
            switch (type)
            {
                case DrawingType.Schematic:
                    newPage = Application.DocumentManager.Add(Settings.SchematicTemplatePath);
                    break;
                case DrawingType.Panel:
                    newPage = Application.DocumentManager.Add(Settings.PanelTemplatePath);
                    break;
                case DrawingType.Reference:
                    newPage = Application.DocumentManager.Add(Settings.ReferenceTemplatePath);
                    break;
                default:
                    return null;
            }
            newPage.Database.SaveAs(new Uri(Uri, savePath).LocalPath, DwgVersion.Current);
            Drawings.Add(new Drawing()
            {
                Uri = new Uri(Uri, savePath),
                Project = this,
                Settings = new DrawingSettings(Settings)
            });
            Save();
            return newPage;
        }

        public static Project Open(string directoryPath)
        {
            XmlSerializer reader = new XmlSerializer(typeof(Project));
            string filePath = Directory.GetFiles(directoryPath, "*.xml").FirstOrDefault() ?? Directory.GetFiles(directoryPath, "*.wdp").FirstOrDefault();
            FileStream file = File.OpenRead(filePath);
            Project project = reader.Deserialize(file) as Project;
            project.Uri = new Uri(filePath);
            return project;
        }

        public void Save()
        {
            XmlSerializer writer = new XmlSerializer(typeof(Project));
            FileStream file = File.OpenWrite(Uri.LocalPath);
            writer.Serialize(file, this);
        }
    }
}

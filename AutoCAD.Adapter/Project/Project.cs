using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using ICA.AutoCAD.IO;
using System;
using System.Collections.Generic;
using System.IO;

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

        public string Name { get; set; }
        public Uri Uri { get; set; }
        public List<Drawing> Drawings { get; set; }
        public ProjectSettings Settings { get; set; }

        #endregion

        #region Constructors

        public Project()
        {
        }

        #endregion

        public void Run<TArgument>(Action<Database, TArgument> action, TArgument value)
        {
            foreach(Drawing drawing in Drawings)
            {
                Database database = Commands.LoadDatabase(drawing.FileUri);
                if(Application.DocumentManager.Contains(drawing.FileUri))
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
                    newPage = Application.DocumentManager.Add(Settings.SchematicTemplate.LocalPath);
                    break;
                case DrawingType.Panel:
                    newPage = Application.DocumentManager.Add(Settings.PanelTemplate.LocalPath);
                    break;
                case DrawingType.Reference:
                    newPage = Application.DocumentManager.Add(Settings.ReferenceTemplate.LocalPath);
                    break;
                default:
                    return null;
            }
            newPage.Database.SaveAs(savePath, DwgVersion.Current);
            Drawings.Add(new Drawing()
            {
                FileUri = new Uri(Uri, savePath),
                Project = this,
                Settings = new DrawingSettings(Settings)
            });
            return newPage;
        }


    }
}

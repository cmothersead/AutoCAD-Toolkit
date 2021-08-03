using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.IO;

namespace ICA.AutoCAD.Adapter
{
    public class Project
    {
        #region Public Properties

        public string Name { get; set; }
        public List<Drawing> Drawings { get; set; }
        public ProjectSettings Settings { get; set; }

        #endregion

        public Project(string filePath)
        {
            Drawings = new List<Drawing>();
            Name = Path.GetFileNameWithoutExtension(filePath);
            Uri projectUri = new Uri(filePath);
            string[] test = File.ReadAllLines(filePath);
            Drawing drawing = new Drawing();
            foreach (string line in test)
            {
                if (line.StartsWith("+"))
                    continue;
                else if (line.StartsWith("?"))
                    continue;
                else if (line.StartsWith("==="))
                {
                    if (line != "===")
                        drawing.Description.Add(line.Substring(3));
                }
                else if (line.StartsWith("=="))
                    continue;
                else
                {
                    drawing.FileUri = new Uri(projectUri, line);
                    Drawings.Add(drawing);
                    drawing = new Drawing();
                }
            }
        }

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
    }
}

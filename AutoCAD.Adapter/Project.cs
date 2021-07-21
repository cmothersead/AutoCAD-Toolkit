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
    }
}

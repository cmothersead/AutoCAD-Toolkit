﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace ICA.AutoCAD.Adapter
{
    public class Project : IDisposable
    {
        #region Private Fields

        private bool disposedValue;

        #endregion

        #region Properties

        #region Public Properties

        [XmlIgnore]
        public Uri DirectoryUri { get; set; }
        public Job Job { get; set; }
        public List<Drawing> Drawings { get; set; } = new List<Drawing>();

        [XmlIgnore]
        public Uri FileUri => new Uri($"{DirectoryUri.LocalPath}\\{Name}.xml");
        public string Name => $"{Job}";

        [XmlIgnore]
        public ProjectSettings Settings { get; set; } = new ProjectSettings();

        #endregion

        #region Private Properties

        private string NextDrawingName => $"{Job.Customer.Id:D3}-{Job.Id:D3}PG{Drawings.Count + 1:D3}";

        #endregion

        #endregion

        #region Constructors

        public Project() { }

        #endregion

        #region Methods

        public void AddPage(DrawingType? type, string name = null)
        {
            if (type is null)
                return;

            if (name is null)
                name = NextDrawingName;

            string path;
            switch (type)
            {
                case DrawingType.Schematic:
                    path = Settings.SchematicTemplatePath;
                    break;
                case DrawingType.Panel:
                    path = Settings.PanelTemplatePath;
                    break;
                case DrawingType.Reference:
                    path = Settings.ReferenceTemplatePath;
                    break;
                default:
                    return;
            }
            Drawings.Add(Drawing.CreateFromTemplate(this, path, name));
            Save();
        }

        #region File IO

        public string GetFilePath(string fileName) => $"{DirectoryUri.LocalPath}\\{fileName}.dwg";

        public static Project Open(string directoryPath)
        {
            string filePath = Directory.GetFiles(directoryPath, "*.xml").FirstOrDefault();

            if (filePath is null)
                return null;

            XmlSerializer reader = new XmlSerializer(typeof(Project));
            FileStream file = File.OpenRead(filePath);
            Project project = reader.Deserialize(file) as Project;
            file.Close();
            project.DirectoryUri = new Uri(directoryPath);
            project.Drawings.ForEach(drawing => drawing.Project = project);
            return project;
        }

        public void Save()
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            XmlSerializer writer = new XmlSerializer(typeof(Project));
            FileStream file = File.OpenWrite(FileUri.LocalPath);
            file.SetLength(0);
            writer.Serialize(file, this, ns);
            file.Close();
            File.SetAttributes(file.Name, File.GetAttributes(file.Name) | FileAttributes.Hidden);
        }

        public static Project Import(string directoryPath) => WDP.Import(Directory.GetFiles(directoryPath, "*.wdp").FirstOrDefault());

        public void Export() => WDP.Export(this, Path.ChangeExtension(FileUri.LocalPath, ".wdp"));

        #endregion

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Drawings.ForEach(drawing => drawing.Dispose());
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public override string ToString() => Name;

        #region Public Static Methods

        public static DrawingType? PromptDrawingType(Editor editor)
        {
            PromptKeywordOptions options = new PromptKeywordOptions("\nChoose page type: ");
            options.Keywords.Add("Schematic");
            options.Keywords.Add("Panel");
            options.Keywords.Add("Reference");
            options.Keywords.Default = "Schematic";

            PromptResult result = editor.GetKeywords(options);
            if (result.Status != PromptStatus.OK)
                return null;

            switch (result.StringResult)
            {
                default:
                    return DrawingType.Schematic;
                case "Panel":
                    return DrawingType.Panel;
                case "Reference":
                    return DrawingType.Reference;
            }
        }

        #endregion

        #endregion

        #region Enums

        public enum DrawingType
        {
            Schematic,
            Panel,
            Reference
        }

        #endregion
    }
}

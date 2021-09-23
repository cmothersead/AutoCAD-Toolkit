using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ICA.AutoCAD.Adapter
{
    public class Project : IDisposable
    {
        private bool disposedValue;

        public enum DrawingType
        {
            Schematic,
            Panel,
            Reference
        }

        #region Public Properties

        [XmlIgnore]
        public Uri DirectoryUri { get; set; }
        [XmlIgnore]
        public Uri FileUri => new Uri($"{DirectoryUri.LocalPath}\\{Name}.xml");

        [XmlAttribute]
        public string Name => $"{Job}";
        public Job Job { get; set; }
        public List<Drawing> Drawings { get; set; } = new List<Drawing>();

        [XmlIgnore]
        public ProjectSettings Settings { get; set; }

        #endregion

        #region Private Properties

        private string NextDrawingName => $"{Job.Customer.Id:D3}-{Job.Id:D3}PG{Drawings.Count + 1:D3}";

        #endregion

        #region Constructors

        public Project() { }

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

        public void AddPage(DrawingType type, string name = null)
        {
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

        public string GetFilePath(string fileName)
        {
            return $"{DirectoryUri.LocalPath}\\{fileName}.dwg";
        }

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

        public static Project Import(string directoryPath)
        {
            string filePath = Directory.GetFiles(directoryPath, "*.wdp").FirstOrDefault();

            return filePath is null ? null : WDP.Import(filePath);
        }

        public void Save()
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            XmlSerializer writer = new XmlSerializer(typeof(Project));
            FileStream file = File.OpenWrite(FileUri.LocalPath);
            writer.Serialize(file, this, ns);
            file.Close();
            File.SetAttributes(file.Name, File.GetAttributes(file.Name) | FileAttributes.Hidden);
        }

        public void Export() => WDP.Export(this, Path.ChangeExtension(FileUri.LocalPath, ".wdp"));

        public override string ToString() => Name;

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

        //public XmlSchema GetSchema() => null;

        //public void ReadXml(XmlReader reader)
        //{
        //    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        //    ns.Add("", "");

        //    reader.ReadStartElement();
        //    if(reader.Name == "Job")
        //    {
        //        XmlSerializer jobSerializer = new XmlSerializer(typeof(Job));
        //        Job = jobSerializer.Deserialize(reader) as Job;
        //    }
        //    if(reader.Name == "Drawings")
        //    {
        //        reader.ReadStartElement();
        //        Drawings = new List<Drawing>();
        //        while(reader.Name == "Drawing")
        //        {
        //            bool empty = reader.IsEmptyElement;
        //            Drawings.Add(new Drawing(this, reader.GetAttribute("FileName")));
        //            reader.ReadStartElement();
        //            if (!empty)
        //            {
        //                reader.ReadStartElement();
        //                reader.ReadEndElement();
        //            }
        //        }
        //        reader.ReadEndElement();
        //    }
        //    reader.ReadEndElement();
        //}

        //public void WriteXml(XmlWriter writer)
        //{
        //    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        //    ns.Add("", "");

        //    writer.WriteAttributeString("Name", Name);

        //    XmlSerializer jobSerializer = new XmlSerializer(typeof(Job));
        //    jobSerializer.Serialize(writer, Job, ns);

        //    writer.WriteStartElement("Drawings");

        //    XmlSerializer drawingSerializer = new XmlSerializer(typeof(Drawing));
        //    Drawings.ForEach(drawing => drawingSerializer.Serialize(writer, drawing, ns));

        //    writer.WriteEndElement();
        //}
    }
}

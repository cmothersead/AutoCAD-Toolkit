using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using ICA.AutoCAD.Adapter.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace ICA.AutoCAD.Adapter
{
    public class Project : IDisposable
    {
        #region Fields

        #region Private Fields

        private bool disposedValue;

        #endregion

        #endregion

        #region Properties

        #region Public Properties

        [JsonIgnore, XmlIgnore]
        public Uri DirectoryUri { get; set; }
        public Job Job { get; set; }
        public List<Drawing> Drawings { get; set; } = new List<Drawing>();
        [JsonIgnore, XmlIgnore]
        public Uri XmlUri => new Uri($@"{DirectoryUri.LocalPath}/{Name}.xml");
        [JsonIgnore, XmlIgnore]
        public Uri JsonUri => new Uri($@"{DirectoryUri.LocalPath}/{Name}.aeproj");
        [JsonIgnore, XmlIgnore]
        public string Name => $"{Job}";
        [JsonIgnore, XmlIgnore]
        public int SheetCount
        {
            get
            {
                int max = 0;
                if (Drawings.Any(drawing => drawing.PageNumber != null))
                    max = Drawings.Where(drawing => drawing.PageNumber != null)
                                  .Max(drawing => int.Parse(drawing.PageNumber));
                if (max < Drawings.Count)
                    max = Drawings.Count;

                return max;
            }
        }

        public ProjectSettings Settings { get; set; } = new ProjectSettings();

        [JsonIgnore, XmlIgnore]
        public List<Component> Components => Drawings.Where(drawing => !drawing.Spare)
                                                     .SelectMany(drawing => drawing.Components)
                                                     .ToList();

        #endregion

        #region Private Properties

        private string NextDrawingName => $"{Job.Customer.Id:D3}-{Job.Id:D3}PG{Drawings.Count + 1:D3}";


        private Dictionary<string, string> Replacements => new Dictionary<string, string>
        {
            { "%P", $"{Job.Code}" },
            { "%S", @"(\d{2,3})" },
        };
        public Regex DrawingNameFormat => new Regex(Settings.FileNameFormat.Replace(Replacements) + @"$");

        #endregion

        #endregion

        #region Constructors

        public Project()
        {
        }

        #endregion

        #region Methods

        #region Public Methods

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
            SaveAsXML();
        }

        public void RunOnAllDrawings(Action<Drawing> action)
        {
            foreach (Drawing drawing in Drawings)
            {
                if (Application.DocumentManager.Contains(drawing.FileUri))
                {
                    Document doc = Application.DocumentManager.Get(drawing.FullPath);
                    Document activeDoc = Application.DocumentManager.MdiActiveDocument;

                    Application.DocumentManager.MdiActiveDocument = doc;

                    using (DocumentLock docLock = doc.LockDocument())
                        action(drawing);

                    Application.DocumentManager.MdiActiveDocument = activeDoc;
                }
                else
                {
                    action(drawing);
                }
            }
        }

        public Drawing NextDrawing(Drawing current, bool skipSpares) => Drawings.Where(drawing => !skipSpares || !drawing.Spare)
                                                                                .SkipWhile(drawing => int.Parse(drawing.PageNumber) <= int.Parse(current.PageNumber))
                                                                                .FirstOrDefault();
        public Drawing PreviousDrawing(Drawing current, bool skipSpares) => Drawings.Where(drawing => !skipSpares || !drawing.Spare)
                                                                                    .Reverse()
                                                                                    .SkipWhile(drawing => int.Parse(drawing.PageNumber) >= int.Parse(current.PageNumber))
                                                                                    .FirstOrDefault();

        #region File IO

        public void SaveAsXML()
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            XmlSerializer writer = new XmlSerializer(typeof(Project));
            FileStream file = File.OpenWrite(XmlUri.LocalPath);
            file.SetLength(0);
            writer.Serialize(file, this, ns);
            file.Close();
            File.SetAttributes(file.Name, File.GetAttributes(file.Name) | FileAttributes.Hidden);
        }

        public void SaveAsJSON()
        {
            Project project = this;
            project.Drawings = Drawings.Where(drawing => !drawing.Spare).ToList();
            string jsonString = JsonConvert.SerializeObject(project, Formatting.Indented, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
            });
            File.WriteAllText(JsonUri.LocalPath, jsonString);
        }

        public void Export() => WDP.Export(this, Path.ChangeExtension(XmlUri.LocalPath, ".wdp"));

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

        #endregion

        #region Public Override Methods

        public override string ToString() => Name;

        #endregion

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

        public static Project OpenXML(string directoryPath)
        {
            string filePath = Directory.GetFiles(directoryPath, "*.xml").FirstOrDefault();

            if (filePath is null)
                return null;

            XmlSerializer reader = new XmlSerializer(typeof(Project));
            FileStream file = File.OpenRead(filePath);
            Project project = reader.Deserialize(file) as Project;
            file.Close();
            project.DirectoryUri = new Uri(directoryPath);
            project.Drawings.ForEach(drawing =>
            {
                drawing.Name = drawing.Name ?? drawing.ReplaceParameters(project.Settings.FileNameFormat);
                drawing.Project = project;
                drawing.TitleBlockAttributes = project.Settings.TitleBlockAttributes;
            });
            return project;
        }

        public static Project OpenJSON(string directoryPath)
        {
            string json = Directory.GetFiles(directoryPath, "*.aeproj").FirstOrDefault();

            if (json is null)
                return null;

            string fileContents = File.ReadAllText(json);

            Project project = JsonConvert.DeserializeObject<Project>(fileContents);
            project.DirectoryUri = new Uri(directoryPath);
            project.Drawings.ForEach(drawing =>
            {
                drawing.Project = project;
                drawing.Name = drawing.Name ?? drawing.ReplaceParameters(project.Settings.FileNameFormat);
                drawing.TitleBlockAttributes = project.Settings.TitleBlockAttributes;
            });

            //If page numbers are integers, then attempt to find existing spare drawings
            if (int.TryParse(project.Drawings.First().PageNumber, out int result))
            {
                project.Drawings.AddRange(Directory.EnumerateFiles(project.DirectoryUri.LocalPath)
                                                    .Where(filePath => !project.Drawings.Any(drawing => drawing.FullPath == filePath))
                                                    .Where(filePath => Path.GetExtension(filePath) == ".dwg")
                                                    .Where(filePath => project.DrawingNameFormat.IsMatch(Path.GetFileNameWithoutExtension(filePath)))
                                                    .Select(filePath => new Drawing
                                                    {
                                                        Name = Path.GetFileNameWithoutExtension(filePath),
                                                        Project = project,
                                                        TitleBlockAttributes = project.Settings.TitleBlockAttributes,
                                                        PageNumber = project.DrawingNameFormat.Match(Path.GetFileNameWithoutExtension(filePath)).Groups[1].Value,
                                                        Spare = true,
                                                    }));
                project.Drawings = project.Drawings.OrderBy(drawing => int.Parse(drawing.PageNumber)).ToList();
            }

            return project;
        }

        public static Project Import(string directoryPath) => WDP.Import(Directory.GetFiles(directoryPath, "*.wdp").FirstOrDefault());

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

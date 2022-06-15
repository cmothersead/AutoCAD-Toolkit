using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using ICA.AutoCAD.Adapter.Extensions;
using ICA.Schematic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace ICA.AutoCAD.Adapter
{
    [DataContract]
    public class Drawing : IDisposable
    {
        #region Properties

        #region Private Properties

        private bool disposedValue;

        private Database Database
        {
            get
            {
                if (Project is null)
                    return null;

                Database database;

                if (Application.DocumentManager.Contains(new Uri(FullPath)))
                {
                    IsLoaded = true;
                    return Application.DocumentManager.Get(FullPath).Database;
                }
                        
                if (!File.Exists(FullPath))
                    return null;

                database = new Database(false, true);
                database.ReadDwgFile(FullPath, FileOpenMode.OpenForReadAndAllShare, true, null);
                database.CloseInput(true);

                if (_spare != null)
                    database.GetTitleBlock().Spare = (bool)_spare;

                IsLoaded = true;

                return database;
            }
        }

        private Dictionary<string, string> Replacements => new Dictionary<string, string>
        {
            { "%P", $"{Project.Job.Code}" },
            { "%S", $"{int.Parse(PageNumber):D2}" },
        };

        #endregion

        #region Public Properties

        [DataMember, XmlAttribute]
        public string PageNumber { get; set; }
        [DataMember, XmlAttribute]
        public string Name { get; set; }
        public bool ShouldSerializeName() => Name != $"{Project.Job.Code}PG{int.Parse(PageNumber):D2}";

        private List<string> _description;
        [DataMember, XmlArrayItem("Value")]
        public List<string> Description
        {
            get => _description ?? Database?.GetDescription();
            set
            {
                _description = value;

                if (!IsLoaded)
                    return;

                Database?.SetDescription(value);
                UpdateTitleBlock();
            }
        }
        public bool ShouldSerializeDescription() => Description.Count != 0 && Description[0] != "SPARE SHEET";

        private bool? _spare;
        [DataMember, XmlAttribute, DefaultValue(false)]
        public bool Spare
        {
            get
            {
                if (_spare != null)
                    return (bool)_spare;

                if(IsLoaded)
                    _spare = TitleBlock.Spare;

                return (bool)(_spare ?? false);
            }
            set
            {
                _spare = value;

                if (!IsLoaded)
                    return;

                if (TitleBlock != null)
                    TitleBlock.Spare = value;

                if(value)
                    Description = new List<string> { "SPARE SHEET" };
            }
        }


        public Uri FileUri => new Uri($@"{Project.DirectoryUri}/{Name}.dwg");
        public Project Project { get; set; }
        public string FullPath => FileUri.LocalPath;
        [XmlIgnore]
        public List<TBAttribute> TitleBlockAttributes { get; set; }
        [XmlIgnore]
        public DrawingSettings Settings { get; set; }
        [XmlIgnore]
        private TitleBlock _titleBlock;
        public TitleBlock TitleBlock
        {
            get
            {
                if( _titleBlock != null)
                    return _titleBlock;

                _titleBlock = Database.GetTitleBlock(this);
                return _titleBlock;
            }
        }
        [XmlIgnore]
        public List<Component> Components => Database.GetParentSymbols()
                                                     .Select(symbol => new Component(symbol))
                                                     .ToList();
        public bool IsLoaded { get; set; }

        #endregion

        #endregion

        #region Constructor

        public Drawing() {}

        public Drawing(string name)
        {
            Name = name;
        }

        #endregion

        #region Methods

        public string ReplaceParameters(string formatString) => formatString.Replace(Replacements);

        public void UpdateTitleBlock()
        {
            if (TitleBlock is null)
                return;

            TitleBlock.Attributes = TitleBlockAttributes.Cast<ITBAttribute>()
                                                        .ToList();

            Save();
        }

        public void UpdatePageNumber()
        {
            Database.SetSheetNumber(PageNumber);
            Save();
        }

        public List<ChildSymbol> GetChildSymbols() => Database.GetChildSymbols();

        public void LogSymbols() => Database.LogSymbols();

        #region Description

        public bool AddDescription(string value)
        {
            if (Database is null)
                return false;

            List<string> description = Description;
            description.Add(value);
            Description = description;
            return true;
        }

        public bool RemoveDescription(string value)
        {
            if (Database is null)
                return false;

            List<string> description = Description;
            description.Remove(value);
            Description = description;
            return true;
        }

        public bool RemoveDescriptionAt(int index)
        {
            if (Database is null)
                return false;

            List<string> description = Description;
            description.RemoveAt(index);
            Description = description;
            return true;
        }

        #endregion

        public static Drawing CreateFromTemplate(Project project, string templatePath, string fileName)
        {
            CloneDatabase(templatePath, $"{project.DirectoryUri.LocalPath}\\{fileName}.dwg");
            return new Drawing()
            {
                Project = project,
                Name = fileName,
                PageNumber = $"{project.Drawings.Count + 1}"
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

        public void Save() => Database.SaveFile();

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Database.Dispose();
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

        #endregion
    }
}

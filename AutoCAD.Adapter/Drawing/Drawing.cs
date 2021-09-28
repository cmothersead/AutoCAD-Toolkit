﻿using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace ICA.AutoCAD.Adapter
{
    public class Drawing : IDisposable
    {
        #region Properties

        #region Private Properties

        private Database _database;
        private bool disposedValue;

        private Database Database
        {
            get
            {
                if (Project is null)
                    return null;

                if (_database is null)
                {
                    if (!File.Exists(FullPath))
                        return null;

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
        public Uri FileUri => new Uri(Project.FileUri, $"{Name}.dwg");
        [XmlIgnore]
        public Project Project { get; set; }
        public string FullPath => new Uri(Project.FileUri, FileUri).LocalPath;
        [XmlIgnore]
        public Dictionary<string, string> TitleBlockAttributes { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlIgnore]
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

        [XmlAttribute, DefaultValue(false)]
        public bool Spare { get; set; }

        [XmlIgnore]
        public DrawingSettings Settings { get; set; }

        [XmlIgnore]
        public List<ParentSymbol> Components => Database.GetObjectIds()
                                                        .Where(id => id.Open() is BlockReference)
                                                        .Select(id => id.Open() as BlockReference)
                                                        .Where(reference => reference.HasAttributeReference("TAG1"))
                                                        .Select(reference => new ParentSymbol(reference))
                                                        .ToList();

        #endregion

        #endregion

        #region Constructor

        public Drawing()
        {
            TitleBlockAttributes = new Dictionary<string, string>()
            {
                { "DWGNO", "Project.Job.Code" },
                { "SHTS", "Project.Drawings.Count" },
                { "TITLE1", "Project.Job.Name" },
                { "TITLE2", "Name" },
                { "SHT", "PageNumber" },
                { "CUST", "Project.Job.Customer.Name" }
            };
        }

        #endregion

        #region Methods

        public void UpdateTitleBlock()
        {
            TitleBlock test = Database.GetTitleBlock();
            Dictionary<string, string> dict = TitleBlockAttributes.ToDictionary(pair => pair.Key, pair => GetPropertyValue(pair.Value).ToUpper());
            test.Attributes = dict;
            Save();
        }

        public string GetPropertyValue(string name)
        {
            PropertyInfo currentProperty;
            object currentObject = this;
            List<string> names = name.Split('.').ToList();
            foreach (string propertyName in names)
            {
                currentProperty = currentObject.GetType().GetProperty(propertyName);
                if (currentProperty is null)
                    return null;

                currentObject = currentProperty.GetValue(currentObject);
            }
            return currentObject.ToString();
        }

        public List<ParentSymbol> GetSymbols() => Database.GetObjectIds()
                                                          .Where(id => id.Open() is BlockReference)
                                                          .Select(id => id.Open() as BlockReference)
                                                          .Where(reference => reference.Layer == ElectricalLayers.SymbolLayer.Name)
                                                          .Where(reference => reference.HasAttributeReference("TAG1"))
                                                          .Select(reference => new ParentSymbol(reference))
                                                          .ToList();

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

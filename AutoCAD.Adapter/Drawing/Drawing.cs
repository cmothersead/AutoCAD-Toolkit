﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using ICA.Schematic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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
                    if (Application.DocumentManager.Contains(new Uri(FullPath)))
                        return Application.DocumentManager.Get(FullPath).Database;

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
        public List<TitleBlock.Attribute> TitleBlockAttributes { get; set; }

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
        public List<Component> Components => Database.GetParentSymbols()
                                                     .Select(symbol => new Component(symbol))
                                                     .ToList();

        #endregion

        #endregion

        #region Constructor

        public Drawing() { }

        public Drawing(string name)
        {
            Name = name;
        }

        #endregion

        #region Methods

        public void UpdateTitleBlock()
        {
            TitleBlock titleBlock = Database.GetTitleBlock();
            if (titleBlock is null)
                return;

            Dictionary<string, string> dict = TitleBlockAttributes.ToDictionary(pair => pair.Tag,
                                                                                pair => GetPropertyValue(pair.Value)?.ToUpper()
                                                                                        ?? pair.Value);
            titleBlock.Attributes = dict.Select(pair => new TitleBlock.Attribute() { Tag = pair.Key, Value = pair.Value })
                                        .ToList();
            Save();
        }

        public string GetPropertyValue(string name)
        {
            PropertyInfo currentProperty;
            object currentObject = this;
            Regex propertyRegex = new Regex(@"(?<!\[)\w+(?!\])");
            Regex indexRegex = new Regex(@"(?<=\[)\d+(?=\])");
            foreach (Match match in propertyRegex.Matches(name))
            {
                currentProperty = currentObject.GetType().GetProperty(match.Value);
                if (currentProperty is null)
                    return null;

                currentObject = currentProperty.GetValue(currentObject);
            }
            if (int.TryParse(indexRegex.Match(name).Value, out int index) && currentObject is IList list)
                currentObject = list[index];
            return currentObject.ToString();
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

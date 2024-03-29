﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using ICA.AutoCAD.Adapter.Windows.Models;
using ICA.AutoCAD.Adapter.Windows.ViewModels;
using ICA.AutoCAD.Adapter.Windows.Views;
using ICA.AutoCAD.IO;
using ICA.Schematic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace ICA.AutoCAD.Adapter
{
    public class TitleBlock : ITitleBlock
    {
        #region Private Members

        private BlockTableRecord _blockTableRecord;

        #endregion

        #region Properties

        #region Private Properties

        private AttributeDefinition SpareAttribute => _blockTableRecord.GetAttributeDefinitions().FirstOrDefault(d => d.Tag == "SPARE");

        #endregion

        #region Public Properties

        public Uri FileUri { get; set; }
        public string Name => Path.GetFileNameWithoutExtension(FileUri?.LocalPath) ?? _blockTableRecord.Name;
        public bool IsLoaded => _blockTableRecord != null;
        public ObjectId ObjectId => _blockTableRecord.ObjectId;
        public Database Database => _blockTableRecord.Database;
        private Drawing _drawing;
        public Drawing Drawing
        {
            get
            {
                if (_drawing != null)
                    return _drawing;

                _drawing = Database?.GetDrawing();
                return _drawing;
            }
        }
        public List<ITBAttribute> Attributes
        {
            get => Reference?.GetAttributeReferences()
                             .Select(attRef => new TBAttribute() { Tag = attRef.Tag, Value = attRef.TextString })
                             .Cast<ITBAttribute>()
                             .ToList();
            set => Reference?.SetAttributeValues(value.ToDictionary(att => att.Tag, att => GetPropertyValue(att.Value, Drawing)?.ToUpper() ?? att.Value));
        }

        public bool IsInserted => _blockTableRecord.GetBlockReferenceIds(true, false).Count != 0;
        public BlockReference Reference => IsInserted ? _blockTableRecord.GetBlockReferenceIds(true, false)[0].Open() as BlockReference : null;
        public bool Spare
        {
            get => !SpareAttribute?.Invisible ?? false;
            set
            {
                SpareAttribute?.SetVisibility(value);
                Reference.RecordGraphicsModified(true);
            }
        }

        #endregion

        #endregion

        #region Constructors

        public TitleBlock(BlockTableRecord record)
        {
            if (!record.Name.Contains("Title Block"))
                throw new ArgumentException($"\"{record.Name}\" is not a valid title block. Name must contain \"Title Block\".");

            if (!record.HasAttribute("TB"))
                throw new ArgumentException($"\"{record.Name}\" is not a valid title block. Block must contain an attribute called \"TB\".");

            _blockTableRecord = record;
        }

        public TitleBlock(BlockTableRecord record, Drawing drawing)
        {
            if (!record.Name.Contains("Title Block"))
                throw new ArgumentException($"\"{record.Name}\" is not a valid title block. Name must contain \"Title Block\".");

            if (!record.HasAttribute("TB"))
                throw new ArgumentException($"\"{record.Name}\" is not a valid title block. Block must contain an attribute called \"TB\".");

            _blockTableRecord = record;

            if(record.Database.Filename == drawing.FileUri.LocalPath)
                _drawing = drawing;
        }

        public TitleBlock(Uri blockFileUri)
        {
            FileUri = blockFileUri;
            string fileName = Path.GetFileNameWithoutExtension(FileUri.LocalPath);

            if (!fileName.Contains("Title Block"))
                throw new ArgumentException($"\"{Path.GetFileName(FileUri.LocalPath)}\" is not a valid title block file. File name must contain \"Title Block\".");

            Database tempDatabase = Commands.LoadDatabase(blockFileUri);

            if (!tempDatabase.GetModelSpace()
                             .OfType<ObjectId>()
                             .Select(id => id.Open())
                             .OfType<AttributeDefinition>()
                             .Any(definition => definition.Tag == "TB"))
                throw new ArgumentException($"\"{Path.GetFileName(blockFileUri.LocalPath)}\" is not a valid title block file. File must contain an attribute called \"TB\".");
        }

        #endregion

        #region Public Methods

        public void Load(Database database)
        {
            Database titleBlockDatabase = Commands.LoadDatabase(FileUri);
            database.Insert(Name, titleBlockDatabase, true);
            database.SetCustomProperties(titleBlockDatabase.GetAllCustomProperties());
            _blockTableRecord = database.GetBlockTable().GetRecord(Name);
        }

        public void Insert()
        {
            if (!IsLoaded)
                throw new Exception("Title Block not loaded into database");

            if (IsInserted)
                return;

            new BlockReference(Point3d.Origin, _blockTableRecord.ObjectId).Insert(Database, ElectricalLayers.TitleBlockLayer);
            _blockTableRecord.Database.Limmax = Reference.GeometricExtents.MaxPoint.ToPoint2D();
            _blockTableRecord.Database.Limmin = Reference.GeometricExtents.MinPoint.ToPoint2D();
            SystemVariables.GridDisplay &= ~GridDisplay.BeyondLimits;
        }

        public void Remove()
        {
            if (!IsInserted)
                return;

            using (Transaction transaction = _blockTableRecord.Database.TransactionManager.StartTransaction())
            {
                LayerTableRecord titleBlockLayer = _blockTableRecord.Database.GetLayer(ElectricalLayers.TitleBlockLayer);
                titleBlockLayer.UnlockWithoutWarning();
                _blockTableRecord.GetBlockReferences(transaction)
                                 .ForEach(reference => reference.EraseObject(transaction));
                titleBlockLayer.LockWithWarning();
                SystemVariables.GridDisplay |= GridDisplay.BeyondLimits;
                transaction.Commit();
            }
        }

        public void Purge()
        {
            if (_blockTableRecord.GetBlockReferenceIds(true, false).Count != 0)
                Remove();

            _blockTableRecord.Database.RemoveCustomProperties(new TitleBlockSettings().ToDictionary().Select(kv => kv.Key));

            using (Transaction transaction = _blockTableRecord.Database.TransactionManager.StartTransaction())
            {
                _blockTableRecord.EraseObject(transaction);
                transaction.Commit();
            }
        }

        #endregion

        #region Public Static Methods

        #region Handlers

        private static List<ObjectId> _forDelete = new List<ObjectId>();
        private static Document CurrentDocument => Application.DocumentManager.MdiActiveDocument;

        public static void RemoveDuplicates(object sender, ObjectEventArgs args)
        {
            if (args.DBObject is BlockReference reference)
                if (reference.Name.Contains("Title Block"))
                {
                    if (reference.GetBlockTableRecord().GetBlockReferenceIds(true, false).Count > 1)
                    {
                        _forDelete.Add(reference.ObjectId);
                    }
                }
        }

        public static void Delete(object sender, EventArgs args)
        {
            Application.Idle -= Delete;
            if (_forDelete.Count > 0)
            {
                using (DocumentLock lockDoc = Application.DocumentManager.GetDocument(_forDelete[0].Database).LockDocument())
                {
                    using (Transaction transaction = CurrentDocument.TransactionManager.StartTransaction())
                    {
                        _forDelete.ForEach(id => id.Erase(transaction));
                        transaction.Commit();
                    }
                }
                _forDelete = new List<ObjectId>();
            }
            CurrentDocument.Editor.WriteMessage("\nTitle Block already present on drawing.");
        }

        #endregion

        #region Commands

        public static TitleBlock Select()
        {
            try
            {
                CurrentDocument.Database.ObjectAppended -= RemoveDuplicates;
                TitleBlockView titleBlockWindow = new TitleBlockView(new TitleBlockViewModel(Paths.TitleBlocks));
                ObservableCollection<TitleBlockFile> validFiles = new ObservableCollection<TitleBlockFile>();
                Dictionary<string, string> errorList = new Dictionary<string, string>();
                foreach (TitleBlockFile file in titleBlockWindow.ViewModel.TitleBlocks)
                {
                    string path = file.Uri.LocalPath;
                    if (!IsDefinitionFile(path))
                        errorList.Add(Path.GetFileName(path), DefinitionFileException(path));
                    else
                        validFiles.Add(file);
                }

                titleBlockWindow.ViewModel.TitleBlocks = validFiles;

                if (errorList.Count > 0)
                {
                    string errorMessage = "The following files were not loaded:\n";

                    if (errorList.Count == 1)
                        errorMessage = "The following file was not loaded:\n";

                    errorList.ForEach(entry => errorMessage += "\n\u2022 \"" + entry.Key + "\" : " + entry.Value);

                    Application.ShowAlertDialog(errorMessage);
                }

                TitleBlock currentTitleBlock = CurrentDocument.Database.GetTitleBlock();

                if (currentTitleBlock != null)
                    titleBlockWindow.ViewModel.SelectedTitleBlock = titleBlockWindow.ViewModel.TitleBlocks.FirstOrDefault(titleBlock => titleBlock.Name == currentTitleBlock.Name);

                Application.ShowModalWindow(titleBlockWindow);

                if ((bool)titleBlockWindow.DialogResult)
                {
                    TitleBlockFile SelectedTitleBlock = titleBlockWindow.ViewModel.SelectedTitleBlock;

                    if (currentTitleBlock != null && currentTitleBlock.Name == SelectedTitleBlock.Name)
                        return null;

                    return new TitleBlock(SelectedTitleBlock.Uri);
                }
            }
            catch (ArgumentException ex)
            {
                Application.ShowAlertDialog(ex.Message);
            }
            finally
            {
                //CurrentDocument.Database.ObjectAppended += RemoveDuplicates;
            }
            return null;
        }

        #endregion

        public static bool IsDefinitionFile(string path)
        {
            if (Path.GetExtension(path) != ".dwg")
                return false;

            if (!Path.GetFileNameWithoutExtension(path).ToUpper().Contains("TITLE BLOCK"))
                return false;

            Database tempDatabase = new Database();
            tempDatabase.ReadDwgFile(path, FileShare.Read, true, null);
            if (!tempDatabase.ContainsTBAtrribute())
                return false;

            return true;
        }

        public static string DefinitionFileException(string path)
        {
            if (Path.GetExtension(path) != ".dwg")
                return $"File must be of type \".dwg\"";

            if (!Path.GetFileNameWithoutExtension(path).ToUpper().Contains("TITLE BLOCK"))
                return $"File name must contain \"Title Block\"";

            Database tempDatabase = new Database();
            tempDatabase.ReadDwgFile(path, FileShare.Read, true, null);
            if (!tempDatabase.ContainsTBAtrribute())
                return $"File must contain attribute \"TB\"";

            return "No error.";
        }

        public static string GetPropertyValue(string name, object propertyObject)
        {
            if (!name.StartsWith("$"))
                return name;

            PropertyInfo currentProperty;
            Regex propertyRegex = new Regex(@"(?<!\[)\w+(?!\])"); //Word characters NOT wrapped in square braces
            Regex indexRegex = new Regex(@"(?<=\[)\d+(?=\])");    //Number characters wrapped in square braces
            foreach (Match match in propertyRegex.Matches(name))
            {
                currentProperty = propertyObject.GetType().GetProperty(match.Value);
                if (currentProperty is null)
                    return null;

                propertyObject = currentProperty.GetValue(propertyObject);
            }
            if (int.TryParse(indexRegex.Match(name).Value, out int index) && propertyObject is IList list && list.Count > index)
                propertyObject = list[index];
            return propertyObject?.ToString();
        }

        #endregion

    }
}

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ICA.AutoCAD.Adapter
{
    public class TitleBlock
    {
        #region Private Members

        private BlockTableRecord _blockTableRecord;

        #endregion

        #region Public Properties

        public Uri FilePath { get; set; }
        public ObjectId ObjectId => _blockTableRecord.ObjectId;
        public string Name => _blockTableRecord.Name;
        public List<string> Attributes => _blockTableRecord.AttributeDefinitions().Select(definition => definition.Tag).ToList();
        public BlockReference Reference => _blockTableRecord.GetBlockReferenceIds(true, false)[0].Open() as BlockReference;

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

        public TitleBlock(Database database, Uri blockFileUri)
        {
            FilePath = blockFileUri;
            string fileName = Path.GetFileNameWithoutExtension(FilePath.LocalPath);

            if (!fileName.Contains("Title Block"))
                throw new ArgumentException($"\"{Path.GetFileName(FilePath.LocalPath)}\" is not a valid title block file. File name must contain \"Title Block\".");

            Database tempDatabase = new Database(false, true);
            tempDatabase.ReadDwgFile(FilePath.LocalPath, FileShare.Read, true, null);

            bool isTitleBlockDefinition = false;
            foreach (ObjectId id in tempDatabase.GetModelSpace())
            {
                if (id.Open() is AttributeDefinition definition)
                    if (definition.Tag == "TB")
                    {
                        isTitleBlockDefinition = true;
                        break;
                    }
            }
            if (!isTitleBlockDefinition)
                throw new ArgumentException($"\"{Path.GetFileName(blockFileUri.LocalPath)}\" is not a valid title block file. File must contain an attribute called \"TB\".");

            database.Insert(fileName, tempDatabase, true);
            _blockTableRecord = database.GetBlockTable().GetRecord(fileName);
        }

        #endregion

        #region Public Methods

        public void Insert()
        {
            if (_blockTableRecord.GetBlockReferenceIds(true, false).Count > 0)
                return;

            new BlockReference(Point3d.Origin, _blockTableRecord.ObjectId) { Layer = _blockTableRecord.Database.GetLayer(ElectricalLayers.TitleBlockLayer).Name }.Insert();
            _blockTableRecord.Database.Limmax = Reference.GeometricExtents.MaxPoint.ToPoint2D();
            _blockTableRecord.Database.Limmin = Reference.GeometricExtents.MinPoint.ToPoint2D();
            GridDisplay.Limits = true;
        }

        public void Remove()
        {
            if (_blockTableRecord.GetBlockReferenceIds(true, false).Count == 0)
                return;

            using (Transaction transaction = _blockTableRecord.Database.TransactionManager.StartTransaction())
            {
                LayerTableRecord titleBlockLayer = _blockTableRecord.Database.GetLayer(ElectricalLayers.TitleBlockLayer);
                titleBlockLayer.UnlockWithoutWarning();
                foreach (ObjectId id in _blockTableRecord.GetBlockReferenceIds(true, false))
                    id.Erase(transaction);
                titleBlockLayer.LockWithWarning();
                GridDisplay.Limits = false;
                transaction.Commit();
            }
        }

        public void Purge()
        {
            if (_blockTableRecord.GetBlockReferenceIds(true, false).Count != 0)
                Remove();

            using (Transaction transaction = _blockTableRecord.Database.TransactionManager.StartTransaction())
            {
                _blockTableRecord.Erase(transaction);
                transaction.Commit();
            }
        }

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

        #endregion

        #region Public Static Methods

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
            if(_forDelete.Count > 0)
            {
                using (DocumentLock lockDoc = Application.DocumentManager.GetDocument(_forDelete[0].Database).LockDocument())
                {
                    using (Transaction transaction = CurrentDocument.TransactionManager.StartTransaction())
                    {
                        foreach (ObjectId id in _forDelete)
                        {
                            id.Erase(transaction);
                        }
                        transaction.Commit();
                    }
                }
                _forDelete = new List<ObjectId>();
            }
            CurrentDocument.Editor.WriteMessage("\nTitle Block already present on drawing.");
        }
        #endregion
    }
}

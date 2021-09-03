﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ICA.AutoCAD.Adapter
{
    public static class ElectricalLayers
    {
        #region Private Fields

        private static LayerTableRecord _forRelock;

        #endregion

        #region Private Properties

        private static Dictionary<string, LayerTableRecord> Layers => typeof(ElectricalLayers).GetProperties(BindingFlags.Static | BindingFlags.Public)
                                                                                      .ToDictionary(p => p.Name, p => p.GetValue(null) as LayerTableRecord);
        
        #endregion

        #region Public Properties

        public static LayerTableRecord SymbolLayer => new LayerTableRecord()
        {
            Name = "SYMS",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 7),
            Description = "Schematic symbol blocks"
        };
        public static LayerTableRecord TagLayer => new LayerTableRecord()
        {
            Name = "TAGS",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 51),
            Description = "Tag attributes",
        };
        public static LayerTableRecord DescriptionLayer => new LayerTableRecord()
        {
            Name = "DESC",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 1),
            Description = "Description attributes"
        };
        public static LayerTableRecord ChildDescriptionLayer => new LayerTableRecord()
        {
            Name = "DESCCHILD",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 20),
            Description = "Description attributes on child components (contacts, etc.)"
        };
        public static LayerTableRecord TerminalLayer => new LayerTableRecord()
        {
            Name = "TERMS",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 130),
            Description = "Terminal number attributes"
        };
        public static LayerTableRecord XrefLayer => new LayerTableRecord()
        {
            Name = "XREF",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 7),
            Description = "Cross reference attributes"
        };
        public static LayerTableRecord ChildXrefLayer => new LayerTableRecord()
        {
            Name = "XREFCHILD",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 134),
            Description = "Cross reference attributes on child components (contacts, etc.)"
        };
        public static LayerTableRecord LinkLayer => new LayerTableRecord()
        {
            Name = "LINK",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 9),
            Description = "Link lines"
        };
        public static LayerTableRecord RatingLayer => new LayerTableRecord()
        {
            Name = "RATING",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 11),
            Description = "Ratings"
        };
        public static LayerTableRecord MiscellaneousLayer = new LayerTableRecord()
        {
            Name = "MISC",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 40),
            Description = "Miscellaneous electrical attributes"
        };
        public static LayerTableRecord WireLayer => new LayerTableRecord()
        {
            Name = "WIRES",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 7),
            Description = "Default wires layer"
        };
        public static LayerTableRecord WireNumberLayer => new LayerTableRecord()
        {
            Name = "WIRENO",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 3),
            Description = "Wire numbers"
        };
        public static LayerTableRecord ManufacturerLayer => new LayerTableRecord()
        {
            Name = "MFG",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 140),
            Description = "Manufacturer name attributes"
        };
        public static LayerTableRecord PartNumberLayer => new LayerTableRecord()
        {
            Name = "CAT",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 40),
            Description = "Part number attributes"
        };
        public static LayerTableRecord LadderLayer => new LayerTableRecord()
        {
            Name = "LADDER",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 7),
            Description = "Ladder markings",
            IsLocked = true
        };
        public static LayerTableRecord TitleBlockLayer => new LayerTableRecord()
        {
            Name = "TITLE BLOCK",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 7),
            Description = "Title Blocks",
            IsLocked = true
        };
        public static LayerTableRecord ConductorLayer => new LayerTableRecord()
        {
            Name = "CONDUCTOR",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 31),
            Description = "Cable conductor markings"
        };
        public static LayerTableRecord GroundLayer => new LayerTableRecord()
        {
            Name = "GROUND",
            Color = Color.FromColorIndex(ColorMethod.ByAci, 7),
            Description = "Ground wires layer"
        };

        #endregion

        #region Methods

        public static void HandleLocks(Database database)
        {
            List<LayerTableRecord> lockedLayers = Layers.Select(l => l.Value).Where(l => l.IsLocked == true).ToList();
            foreach (LayerTableRecord lockedLayer in lockedLayers)
            {
                if (database.HasLayer(lockedLayer))
                {
                    using (Transaction transaction = database.TransactionManager.StartTransaction())
                    {
                        DBObject layer = transaction.GetObject(database.GetLayer(lockedLayer).ObjectId, OpenMode.ForWrite);
                        layer.Modified += UnlockWarning;
                    }
                }
            }
        }

        public static void UnlockWarning(object sender, EventArgs e)
        {
            if (!((string)Application.GetSystemVariable("CMDNAMES")).Contains("LAYER"))
                return;

            LayerTableRecord layer = sender as LayerTableRecord;
            if (layer.IsLocked == false)
            {
                MessageBoxResult result = MessageBox.Show("Manual modification of this layer and its contents can cause unexpected errors. Continue?",
                                                          "Unlock Layer",
                                                          MessageBoxButton.OKCancel,
                                                          MessageBoxImage.Warning);
                if (result != MessageBoxResult.OK)
                {
                    _forRelock = layer;
                    Application.Idle += Relock;
                }
            }
        }

        private static void Relock(object sender, EventArgs e)
        {
            Application.Idle -= Relock;
            using (DocumentLock lockDoc = Application.DocumentManager.GetDocument(_forRelock.Database).LockDocument())
                _forRelock.Lock();
        }

        public static void Update(Database database)
        {
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                foreach (LayerTableRecord layer in Layers.Select(v => v.Value))
                {
                    LayerTableRecord layerInDoc = database.GetLayer(transaction, layer).GetForWrite(transaction);
                    layerInDoc.Color = layer.Color;
                    layerInDoc.Description = layer.Description;
                }
                transaction.Commit();
            }
        }

        #endregion

    }
}

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Windows;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ICA.AutoCAD.Adapter
{
    public class Initializer : IExtensionApplication
    {
        private DocumentCollection DocumentCollection = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager;

        public void Initialize()
        {
            DocumentCollection.DocumentCreated += new DocumentCollectionEventHandler(DocumentCreated);
        }

        public void Terminate()
        {
        }

        private void DocumentCreated(object sender, DocumentCollectionEventArgs args)
        {
            if (args.Document.Database.HasLayer(ElectricalLayers.TitleBlockLayer))
                using(Transaction transaction = args.Document.Database.TransactionManager.StartTransaction())
                {
                    DBObject layer = transaction.GetObject(args.Document.Database.GetLayer(ElectricalLayers.TitleBlockLayer).ObjectId, OpenMode.ForWrite);
                    layer.Modified += new EventHandler(LayerUnlocked);
                }
        }

        private LayerTableRecord forRelock;

        public void LayerUnlocked(object sender, EventArgs e)
        {
            LayerTableRecord layer = sender as LayerTableRecord;
            if(layer.IsLocked == false)
            {
                MessageBoxResult result = MessageBox.Show("Modification of this layer and its contents can break automatic electrical functionality. Continue?",
                                                          "Unlock Layer",
                                                          MessageBoxButton.OKCancel,
                                                          MessageBoxImage.Warning);
                if (result != MessageBoxResult.OK)
                {
                    forRelock = layer;
                    Application.Idle += Application_Idle;
                }
            }
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            Application.Idle -= Application_Idle;
            using (DocumentLock lockDoc = Application.DocumentManager.GetDocument(forRelock.Database).LockDocument())
                forRelock.Lock();
        }
    }
}

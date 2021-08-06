using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using System;

namespace ICA.AutoCAD.Adapter
{
    public class Initializer : IExtensionApplication
    {
        private DocumentCollection DocumentCollection = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager;

        public void Initialize()
        {
            if (Environment.UserName.Contains("cmoth"))
                ConfigureDefaults();

            DocumentCollection.DocumentCreated += new DocumentCollectionEventHandler(DocumentCreated);
        }

        public void Terminate()
        {
        }

        private void DocumentCreated(object sender, DocumentCollectionEventArgs args)
        {
            ElectricalLayers.HandleLocks(args.Document.Database);
            if (args.Document.Database.GetTitleBlock() is TitleBlock titleBlock)
            {
                GridDisplay.Limits = true;
                Commands.ZoomExtents(args.Document, titleBlock.Reference.GeometricExtents);
            }
        }

        private void ConfigureDefaults()
        {
            Application.DocumentManager.MdiActiveDocument.SendStringToExecute("netload ", true, false, false);
        }
    }
}

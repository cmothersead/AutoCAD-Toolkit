using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.IO;

namespace ICA.AutoCAD.Adapter
{
    public class Initializer : IExtensionApplication
    {
        private DocumentCollection DocumentCollection => Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager;

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
                SystemVariables.GridDisplay &= ~GridDisplay.BeyondLimits;
                Commands.ZoomExtents(args.Document, titleBlock.Reference.GeometricExtents);
            }
        }

        private void ConfigureDefaults()
        {
            SystemVariables.Backup = false;
            SystemVariables.FileDialog = true;
            SystemVariables.PDFComments = false;
            SystemVariables.LockFade = 0;
            var test = SystemVariables.TrustedPaths;
            test.Add($"{Path.GetDirectoryName(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName)}\\...");
            SystemVariables.TrustedPaths = test;
        }
    }
}

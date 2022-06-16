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
            if (Environment.UserName.Contains("cmoth") || Environment.UserName.Contains("Colin"))
                ConfigureDefaults();
            //Overrule.AddOverrule(RXObject.GetClass(typeof(Entity)), new EraseLinksOverrule(), false);
            //Overrule.AddOverrule(RXObject.GetClass(typeof(Entity)), new GroupGripOverrule(), false);
            DocumentCollection.DocumentCreated += new DocumentCollectionEventHandler(DocumentCreated);
        }

        public void Terminate()
        {
        }

        private void DocumentCreated(object sender, DocumentCollectionEventArgs args)
        {
            ElectricalLayers.HandleLocks(args.Document.Database);
            if (args.Document.Database.GetTitleBlock() is TitleBlock titleBlock && titleBlock.IsInserted)
            {
                SystemVariables.GridDisplay &= ~GridDisplay.BeyondLimits;
                args.Document.ZoomExtents(titleBlock.Reference.GeometricExtents);
            }
        }

        private void ConfigureDefaults()
        {
            SystemVariables.Backup = false;
            SystemVariables.FileDialog = true;
            SystemVariables.PDFComments = false;
            SystemVariables.LockFade = 0;
            var test = SystemVariables.TrustedPaths;
            string trustedPath = $"{Path.GetDirectoryName(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName)}\\...";
            if (!test.Contains(trustedPath))
                test.Add(trustedPath);
            SystemVariables.TrustedPaths = test;
        }
    }
}

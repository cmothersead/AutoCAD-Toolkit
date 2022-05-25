using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Linq;

namespace ICA.AutoCAD
{
    public static class DocumentCollectionExtensions
    {
        public static bool Contains(this DocumentCollection collection, Uri uri) => collection.Cast<Document>()
                                                                                              .Where(document => document.IsNamedDrawing)
                                                                                              .Any(document => document.Name == uri.LocalPath);

        public static Document Get(this DocumentCollection collection, string fullPath) => collection.Cast<Document>()
                                                                                                 .Where(document => document.IsNamedDrawing)
                                                                                                 .FirstOrDefault(document => document.Name == fullPath);
    }
}

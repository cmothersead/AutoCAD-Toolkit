using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Linq;

namespace ICA.AutoCAD
{
    public static class DocumentCollectionExtensions
    {
        public static bool Contains(this DocumentCollection collection, Uri uri) => collection.Cast<Document>()
                                                                                              .Any(document => document.Name == uri.LocalPath);

        public static Document Get(this DocumentCollection collection, string name) => collection.Cast<Document>()
                                                                                                 .FirstOrDefault(document => document.Name == name);
    }
}

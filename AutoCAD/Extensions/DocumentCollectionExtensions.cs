using Autodesk.AutoCAD.ApplicationServices;
using System;

namespace ICA.AutoCAD
{
    public static class DocumentCollectionExtensions
    {
        public static bool Contains(this DocumentCollection collection, Uri uri)
        {
            foreach(Document document in collection)
                if (document.Name == uri.LocalPath)
                    return true;

            return false;
        }

        public static Document Get(this DocumentCollection collection, string name)
        {
            foreach (Document document in collection)
                if (document.Name == name)
                    return document;

            return null;
        }
    }
}

using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace ICA.AutoCAD.Adapter.Windows.Models
{
    public class TitleBlockFile
    {
        public Uri Uri { get; set; }
        public string Name => Path.GetFileNameWithoutExtension(Uri.LocalPath);
        public BitmapImage Preview => new BitmapImage(new Uri(Uri, $"images/{Path.GetFileNameWithoutExtension(Uri.LocalPath)}.png"));

        public TitleBlockFile(string path)
        {
            Uri = new Uri(path);
        }
    }
}

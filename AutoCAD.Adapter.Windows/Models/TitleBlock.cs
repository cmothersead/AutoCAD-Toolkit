using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ICA.AutoCAD.Adapter.Windows.Models
{
    public class TitleBlock
    {
        public Uri FilePath { get; set; }
        public string Name => Path.GetFileNameWithoutExtension(FilePath.LocalPath);
        public Image Preview => new Image { Source = new BitmapImage(new Uri(FilePath, $"images/{Path.GetFileNameWithoutExtension(FilePath.LocalPath)}.png")) };

        public TitleBlock(string path)
        {
            FilePath = new Uri(path);
        }
    }
}

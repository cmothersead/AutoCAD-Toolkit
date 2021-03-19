using System.Collections.ObjectModel;

namespace ICA.AutoCAD.Adapter.Windows.Models
{
    public class Manufacturer
    {
        public string Name { get; set; }
        public ObservableCollection<Part> Parts { get; set; }
    }
}

using System.Collections.ObjectModel;

namespace ICA.AutoCAD.Adapter.Windows.Models
{
    public class Family
    {
        public bool IsLoaded = false;
        public string FamilyCode { get; set; }
        public ObservableCollection<Manufacturer> Manufacturers { get; set; }
        public Manufacturer CurrentManufacturer { get; set; }
        public Part CurrentPart { get; set; }
    }
}

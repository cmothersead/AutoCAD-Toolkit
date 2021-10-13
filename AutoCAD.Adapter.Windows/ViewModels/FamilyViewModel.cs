using ICA.Schematic.Data;
using System.Collections.ObjectModel;
using System.Linq;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class FamilyViewModel
    {
        #region Properties

        #region Public Properties

        public bool IsLoaded { get; set; } = false;
        public string FamilyCode { get; set; }
        public ObservableCollection<Manufacturer> Manufacturers { get; set; }
        public Manufacturer CurrentManufacturer { get; set; }
        public Part CurrentPart { get; set; }

        #endregion

        #endregion

        #region Operators

        public static implicit operator FamilyViewModel(Family v) => new FamilyViewModel
        {
            FamilyCode = v.Code,
            Manufacturers = new ObservableCollection<Manufacturer>(v.Manufacturers.Select(manufacturer => new Manufacturer
            {
                Id = manufacturer.Id,
                Name = manufacturer.Name.ToUpper(),
                Parts = new ObservableCollection<Part>(manufacturer.Parts)
            }))
        };

        #endregion
    }
}

using ICA.Schematic.Data;
using System.Collections.ObjectModel;

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

        public static implicit operator FamilyViewModel(Family v)
        {
            var manufacturers = new ObservableCollection<Manufacturer>();
            foreach(var item in v.Manufacturers)
            {
                manufacturers.Add(new Manufacturer
                {
                    Id = item.Id,
                    Name = item.Name.ToUpper(),
                    Parts = new ObservableCollection<Part>(item.Parts)
                });
            }
            return new FamilyViewModel
            {
                FamilyCode = v.Code,
                Manufacturers = manufacturers
            };
        }

        #endregion
    }
}

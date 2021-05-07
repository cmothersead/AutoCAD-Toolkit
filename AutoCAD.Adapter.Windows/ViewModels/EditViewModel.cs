using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ICA.AutoCAD.Adapter.Windows.Models;
using ICA.Schematic;
using ICA.Schematic.Data;
using Family = ICA.AutoCAD.Adapter.Windows.Models.Family;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class EditViewModel
    {
        ISymbol _symbol;
        public string Tag => _symbol.Tag;
        public DescriptionCollection Description { get; set; }
        public string Installation => _symbol.Enclosure;
        public string Location => _symbol.Location;
        public Family Family { get; set; }

        public EditViewModel()
        {
        }

        public EditViewModel(ISymbol symbol)
        {
            _symbol = symbol;
            Description = new DescriptionCollection(_symbol.Description);
        }

        public async Task LoadFamilyDataAsync()
        {
            int familyId = await FamilyProcessor.GetFamilyIdAsync(Family.FamilyCode);
            var manufacturers = await ManufacturerProcessor.GetManufacturersUppercaseAsync(familyId);
            foreach (var manufacturer in manufacturers)
            {
                var items = await PartProcessor.GetPartNumbersAsync(familyId, manufacturer.ManufacturerId);
                var parts = new ObservableCollection<Models.Part>();
                foreach (var item in items)
                {
                    parts.Add(new Models.Part
                    {
                        Id = item.PartId,
                        Number = item.PartNumber
                    });
                }
                Family.Manufacturers.Add(new Models.Manufacturer
                {
                    Name = manufacturer.ManufacturerName,
                    Parts = parts
                });
            }
            Family.IsLoaded = true;
        }
    }
}

using System.Linq;
using System.Threading.Tasks;
using ICA.AutoCAD.Adapter.Windows.Models;
using ICA.Schematic;
using ICA.Schematic.Data;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class EditViewModel : BaseViewModel
    {
        public string Tag { get; set; }
        public DescriptionCollection Description { get; set; }
        public string Installation { get; set; }
        public string Location { get; set; }
        private FamilyViewModel _family;
        public FamilyViewModel Family
        {
            get => _family;
            set
            {
                _family = value;
                OnPropertyChanged(nameof(Family));
            }
        }

        public EditViewModel()
        {
        }

        public EditViewModel(ISymbol symbol)
        {
            Tag = symbol.Tag;
            Description = new DescriptionCollection(symbol.Description);
            Installation = symbol.Enclosure;
            Location = symbol.Location;
            Family = new FamilyViewModel
            {
                FamilyCode = symbol.Family,
                CurrentManufacturer = new Manufacturer
                {
                    Name = symbol.ManufacturerName
                },
                CurrentPart = new Part
                {
                    Number = symbol.PartNumber
                }
            };
        }

        public async Task LoadFamilyDataAsync()
        {
            string code = Family.FamilyCode;
            string currentManufacturerName = Family.CurrentManufacturer.Name;
            string currentPartNumber = Family.CurrentPart.Number;
            Family = await FamilyProcessor.GetFamilyAsync(code);
            Family.CurrentManufacturer = Family.Manufacturers.Single(m => m.Name == currentManufacturerName);
            Family.CurrentPart = Family.CurrentManufacturer.Parts.Single(p => p.Number == currentPartNumber);
            Family.IsLoaded = true;
        }
    }
}

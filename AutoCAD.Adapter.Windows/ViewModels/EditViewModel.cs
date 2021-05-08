using System.Linq;
using System.Threading.Tasks;
using ICA.AutoCAD.Adapter.Windows.Models;
using ICA.Schematic;
using ICA.Schematic.Data;
using Family = ICA.AutoCAD.Adapter.Windows.Models.Family;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class EditViewModel : BaseViewModel
    {
        ISymbol _symbol;
        public string Tag => _symbol.Tag;
        public DescriptionCollection Description { get; set; }
        public string Installation => _symbol.Enclosure;
        public string Location => _symbol.Location;
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
            _symbol = symbol;
            Description = new DescriptionCollection(symbol.Description);
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

using System.Linq;
using System.Threading.Tasks;
using ICA.AutoCAD.Adapter.Windows.Models;
using ICA.Schematic;
using ICA.Schematic.Data;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class ParentSymbolEditViewModel : BaseViewModel
    {
        IParentSymbol _symbol;
        public string Tag { get; set; }
        public DescriptionCollection Description { get; set; }
        public bool DescriptionHidden { get; set; }
        public string Installation { get; set; }
        public string Location { get; set; }
        public bool InstallationHidden { get; set; }
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
        public string Manufacturer { get; set; }
        public string PartNumber { get; set; }
        public bool PartInfoHidden { get; set; }

        public ParentSymbolEditViewModel(IParentSymbol symbol)
        {
            _symbol = symbol;
            Tag = _symbol.Tag;
            Description = new DescriptionCollection(_symbol.Description);
            DescriptionHidden = _symbol.DescriptionHidden;
            Installation = _symbol.Enclosure;
            Location = _symbol.Location;
            InstallationHidden = _symbol.InstallationHidden;
            Family = new FamilyViewModel()
            {
                FamilyCode = _symbol.Family,
                CurrentManufacturer = new Manufacturer() { Name = _symbol.ManufacturerName },
                CurrentPart = new Part() { Number = _symbol.PartNumber }
            };
            Manufacturer = _symbol.ManufacturerName;
            PartNumber = _symbol.PartNumber;
            PartInfoHidden = _symbol.PartInfoHidden;
        }

        public async Task LoadFamilyDataAsync()
        {
            string code = Family.FamilyCode;
            string currentManufacturerName = Family.CurrentManufacturer.Name;
            string currentPartNumber = Family.CurrentPart.Number;
            Family test = await FamilyProcessor.GetFamilyAsync(code);
            Family = test;
            Family.CurrentManufacturer = Family.Manufacturers.Single(m => m.Name == currentManufacturerName);
            Family.CurrentPart = Family.CurrentManufacturer.Parts.Single(p => p.Number == currentPartNumber);
            Family.IsLoaded = true;
        }

        public void UpdateSymbol()
        {
            _symbol.Tag = Tag;
            _symbol.Description = Description.Select(d => d.Value).Where(v => v != null).ToList();
            _symbol.DescriptionHidden = DescriptionHidden;
            _symbol.Enclosure = Installation;
            _symbol.Location = Location;
            _symbol.InstallationHidden = InstallationHidden;
            _symbol.ManufacturerName = Manufacturer;
            _symbol.PartNumber = PartNumber;
            _symbol.PartInfoHidden = PartInfoHidden;
            _symbol.CollapseAttributeStack();
        }
    }
}

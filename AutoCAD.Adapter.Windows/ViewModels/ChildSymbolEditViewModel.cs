using ICA.AutoCAD.Adapter.Windows.Models;
using ICA.Schematic;
using ICA.Schematic.Data;
using System.Linq;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class ChildSymbolEditViewModel : BaseViewModel
    {
        IChildSymbol _symbol;
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
        public bool PartInfoHidden { get; set; }

        public ChildSymbolEditViewModel(IChildSymbol symbol)
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
            PartInfoHidden = _symbol.PartInfoHidden;
        }

        public void UpdateSymbol()
        {
            _symbol.Tag = Tag;
            _symbol.Description = Description.Select(d => d.Value).Where(v => v != null).ToList();
            _symbol.DescriptionHidden = DescriptionHidden;
            _symbol.Enclosure = Installation;
            _symbol.Location = Location;
            _symbol.InstallationHidden = InstallationHidden;
            _symbol.CollapseAttributeStack();
        }
    }
}

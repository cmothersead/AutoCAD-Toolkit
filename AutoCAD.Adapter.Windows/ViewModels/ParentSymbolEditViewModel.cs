using ICA.AutoCAD.Adapter.Windows.Models;
using ICA.Schematic;
using ICA.Schematic.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class ParentSymbolEditViewModel : SymbolEditViewModel
    {
        #region Properties

        #region Private Properties

        private IParentSymbol ParentSymbol => (IParentSymbol)_symbol;

        #endregion

        #region Public Properties

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
        public string Rating { get; set; }
        public bool RatingHidden { get; set; }
        public List<string> Pins { get; set; }

        public CharacterCasing CharacterCasing { get; set; } = CharacterCasing.Upper;

        #endregion

        #endregion

        #region Constructors

        public ParentSymbolEditViewModel(Window view, IParentSymbol symbol) : base(view, symbol)
        {
            Description = new DescriptionCollection(ParentSymbol.Description);
            DescriptionHidden = ParentSymbol.DescriptionHidden;
            Installation = ParentSymbol.Enclosure;
            Location = ParentSymbol.Location;
            InstallationHidden = ParentSymbol.InstallationHidden;
            Family = new FamilyViewModel()
            {
                FamilyCode = ParentSymbol.Family,
                CurrentManufacturer = new Manufacturer() { Name = ParentSymbol.ManufacturerName },
                CurrentPart = new Part() { Number = ParentSymbol.PartNumber }
            };
            Manufacturer = ParentSymbol.ManufacturerName;
            PartNumber = ParentSymbol.PartNumber;
            PartInfoHidden = ParentSymbol.PartInfoHidden;
            Rating = ParentSymbol.Rating;
            RatingHidden = ParentSymbol.RatingHidden;
            OkCommand = new RelayCommand(UpdateAndClose);
        }

        #endregion

        #region Methods

        #region Private Methods

        private new void UpdateAndClose()
        {
            base.UpdateAndClose();
            ParentSymbol.Description = Description.Select(d => d.Value)
                                             .Where(v => v != null)
                                             .ToList();
            ParentSymbol.DescriptionHidden = DescriptionHidden;
            ParentSymbol.Enclosure = Installation;
            ParentSymbol.Location = Location;
            ParentSymbol.InstallationHidden = InstallationHidden;
            ParentSymbol.ManufacturerName = Manufacturer;
            ParentSymbol.PartNumber = PartNumber;
            ParentSymbol.PartInfoHidden = PartInfoHidden;
            ParentSymbol.Rating = Rating;
            ParentSymbol.RatingHidden = RatingHidden;
            ParentSymbol.UpdateTag();
            ParentSymbol.CollapseAttributeStack();
        }

        #endregion

        #region Public Methods

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

        #endregion

        #endregion
    }
}

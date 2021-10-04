using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ICA.AutoCAD.Adapter.Windows.Models;
using ICA.Schematic;
using ICA.Schematic.Data;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class ComponentEditViewModel : BaseViewModel
    {
        #region Properties

        #region Private Properties

        private IComponent Component { get; }

        #endregion

        #region Public Properties

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

        public ICommand OkCommand { get; set; }

        #endregion

        #endregion

        #region Constructors

        public ComponentEditViewModel(Window view, IComponent component)
        {
            _view = view;
            Tag = component.Tag;
            Description = new DescriptionCollection(Component.Description);
            Family = new FamilyViewModel()
            {
                FamilyCode = Component.Family,
                CurrentManufacturer = new Manufacturer() { Name = Component.ManufacturerName },
                CurrentPart = new Part() { Number = Component.PartNumber }
            };
            Manufacturer = Component.ManufacturerName;
            PartNumber = Component.PartNumber;
            OkCommand = new RelayCommand(UpdateAndClose);
        }

        #endregion

        #region Methods

        #region Private Methods

        private void UpdateAndClose()
        {
            Component.Description = Description.Select(d => d.Value)
                                             .Where(v => v != null)
                                             .ToList();
            Component.ManufacturerName = Manufacturer;
            Component.PartNumber = PartNumber;
            _view.DialogResult = true;
            _view.Close();
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using ICA.AutoCAD.Adapter.Windows.Models;
using ICA.Schematic;
using ICA.Schematic.Data;
using Family = ICA.AutoCAD.Adapter.Windows.Models.Family;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class EditViewModel
    {
        public string Tag { get; set; }
        public DescriptionCollection Description { get; set; }
        public string Installation { get; set; }
        public string Location { get; set; }
        public Family Family { get; set; }

        public EditViewModel()
        {
        }

        public EditViewModel(ISymbol symbol)
        {
            Tag = symbol.Tag;
            Description = new DescriptionCollection(symbol.Description);
            Installation = symbol.Enclosure;
            Location = symbol.Location;
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ICA.AutoCAD.Adapter.Windows.Models;

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
            Description = new DescriptionCollection
            {
                "New",
                "Data"
            };
            Family = new Family
            {
                FamilyCode = "CB",
                Manufacturers = new ObservableCollection<Manufacturer>
                {
                    new Manufacturer
                    {
                        Name = "ALLEN-BRADLEY",
                        Parts = new ObservableCollection<Part>
                        {
                            new Part
                            {
                                Id = 1,
                                Number = "1492-SPM1C020"
                            },
                            new Part
                            {
                                Id = 2,
                                Number = "1492-SPM1C050"
                            },
                            new Part
                            {
                                Id = 3,
                                Number = "1492-SPM1C200"
                            }
                        }
                    },
                    new Manufacturer
                    {
                        Name = "SPRECHER + SCHUH",
                        Parts = new ObservableCollection<Part>
                        {
                            new Part
                            {
                                Id = 4,
                                Number = "PART1"
                            },
                            new Part
                            {
                                Id = 5,
                                Number = "PART2"
                            },
                            new Part
                            {
                                Id = 6,
                                Number = "PART3"
                            }
                        }
                    }
                }
            };
        }
    }
}

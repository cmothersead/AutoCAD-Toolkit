using System.Collections.ObjectModel;
using ICA.AutoCAD.Adapter.Windows.Models;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class EditViewModel
    {
        public string Tag { get; set; }
        public ObservableCollection<Description> Description { get; set; }
        public string Installation { get; set; }
        public string Location { get; set; }

        public EditViewModel()
        {
            Description = new ObservableCollection<Description> { new Description { Value = "Test" }, new Description { Value = "Data" }, new Description { Value = "Entry" } };
            Tag = "This is a tag";
            Installation = "INST";
            Location = "LOC";
        }
    }
}

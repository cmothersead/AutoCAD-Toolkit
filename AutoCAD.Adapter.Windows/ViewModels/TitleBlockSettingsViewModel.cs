using ICA.AutoCAD.Adapter.Windows.Models;
using ICA.Schematic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class TitleBlockSettingsViewModel : BaseViewModel
    {
        private TitleBlockFile _titleBlockFile;
        private ITitleBlock _titleBlock;
        public ObservableCollection<ITBAttribute> Attributes { get; set; }
        public List<string> Tags => Attributes.Select(x => x.Tag).ToList();
        public ICommand OKCommand { get; set; }


        public TitleBlockSettingsViewModel(Window view, ITitleBlock titleBlock)
        {
            _view = view;
            _titleBlock = titleBlock;
            Attributes = new ObservableCollection<ITBAttribute>(titleBlock.Attributes);
            OKCommand = new RelayCommand(UpdateAndClose);
        }

        public TitleBlockSettingsViewModel()
        {
            Attributes = new ObservableCollection<ITBAttribute>()
            {
                new TBAttribute("Key1", "Value1"),
                new TBAttribute("Key2", "Value2"),
                new TBAttribute("Key3", "Value3"),
                new TBAttribute("Key4", "Value4"),
                new TBAttribute("Key5", "Value5"),
            };
        }

        private void UpdateAndClose()
        {
            _titleBlock.Attributes = Attributes.ToList();
            _view.DialogResult = true;
            _view.Close();
        }

        public class TBAttribute : ITBAttribute
        {
            public string Tag { get; set; }
            public string Value { get; set; }
            public TBAttribute(string key, string value)
            {
                Tag = key;
                Value = value;
            }
        }
    }
}

using ICA.AutoCAD.Adapter.Windows.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class TitleBlockViewModel : BaseViewModel
    {
        public ObservableCollection<TitleBlockFile> TitleBlocks { get; set; }
        private TitleBlockFile _selectedTitleBlock;
        public TitleBlockFile SelectedTitleBlock
        {
            get => _selectedTitleBlock;
            set
            {
                _selectedTitleBlock = value;
                OnPropertyChanged(nameof(SelectedTitleBlock));
            }
        }

        public TitleBlockViewModel(string titleBlockDirectory)
        {
            TitleBlocks = new ObservableCollection<TitleBlockFile>(Directory.GetFiles(titleBlockDirectory)
                                                                            .Select(filePath => new TitleBlockFile(filePath)));
        }
    }
}

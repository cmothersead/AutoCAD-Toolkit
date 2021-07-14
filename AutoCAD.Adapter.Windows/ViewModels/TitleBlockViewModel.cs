using ICA.AutoCAD.Adapter.Windows.Models;
using System.Collections.ObjectModel;
using System.IO;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class TitleBlockViewModel : BaseViewModel
    {
        public ObservableCollection<TitleBlockFile> TitleBlocks { get; set; } = new ObservableCollection<TitleBlockFile>();
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
            foreach (string filePath in Directory.GetFiles(titleBlockDirectory))
                TitleBlocks.Add(new TitleBlockFile(filePath));
        }
    }
}

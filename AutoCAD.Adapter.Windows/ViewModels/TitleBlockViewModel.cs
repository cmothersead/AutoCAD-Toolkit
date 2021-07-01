using ICA.AutoCAD.Adapter.Windows.Models;
using System.Collections.ObjectModel;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class TitleBlockViewModel : BaseViewModel
    {
        public ObservableCollection<TitleBlock> TitleBlocks { get; set; }
        private TitleBlock _selectedTitleBlock;
        public TitleBlock SelectedTitleBlock
        {
            get => _selectedTitleBlock;
            set
            {
                _selectedTitleBlock = value;
                OnPropertyChanged(nameof(SelectedTitleBlock));
            }
        }
    }
}

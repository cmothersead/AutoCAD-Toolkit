using ICA.AutoCAD.Adapter.Windows.ViewModels;
using ICA.Schematic;
using System.Windows;

namespace ICA.AutoCAD.Adapter.Windows.Views
{
    /// <summary>
    /// Interaction logic for TitleBlockSettingsView.xaml
    /// </summary>
    public partial class TitleBlockSettingsView : Window
    {
        public TitleBlockSettingsView(ITitleBlock titleBlock)
        {
            InitializeComponent();
            DataContext = new TitleBlockSettingsViewModel(this, titleBlock);
        }
    }
}

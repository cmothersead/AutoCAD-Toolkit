using ICA.AutoCAD.Adapter.Windows.ViewModels;
using System.Windows;

namespace ICA.AutoCAD.Adapter.Windows.Views
{
    /// <summary>
    /// Interaction logic for TitleBlockView.xaml
    /// </summary>
    public partial class TitleBlockView : Window
    {
        public TitleBlockView()
        {
            InitializeComponent();
        }

        public TitleBlockView(TitleBlockViewModel titleBlockViewModel)
        {
            InitializeComponent();
            DataContext = titleBlockViewModel;
        }
    }
}

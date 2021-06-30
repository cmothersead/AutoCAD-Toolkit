using ICA.AutoCAD.Adapter.Windows.ViewModels;
using System.Windows;

namespace ICA.AutoCAD.Adapter.Windows.Views
{
    /// <summary>
    /// Interaction logic for TitleBlockView.xaml
    /// </summary>
    public partial class TitleBlockView : Window
    {
        public TitleBlockViewModel ViewModel { get; }

        public TitleBlockView()
        {
            InitializeComponent();
        }

        public TitleBlockView(TitleBlockViewModel titleBlockViewModel)
        {
            InitializeComponent();
            TitleBlock_ComboBox.Text = "Please select a Title Block...";
            ViewModel = titleBlockViewModel;
            DataContext = ViewModel;
        }

        private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}

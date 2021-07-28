﻿using System.Windows;
using ICA.AutoCAD.Adapter.Windows.ViewModels;
using ICA.Schematic;

namespace ICA.AutoCAD.Adapter.Windows.Views
{
    /// <summary>
    /// Interaction logic for EditView.xaml
    /// </summary>
    public partial class ParentSymbolEditView : Window
    {
        private readonly ParentSymbolEditViewModel _viewModel;

        public ParentSymbolEditView(IParentSymbol symbol)
        {
            InitializeComponent();
            _viewModel = new ParentSymbolEditViewModel(symbol);
            DataContext = _viewModel;
        }

        public ParentSymbolEditView(ParentSymbolEditViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.UpdateSymbol();
            Close();
        }
    }
}

﻿using ICA.AutoCAD.Adapter.Windows.Models;
using ICA.Schematic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ICA.AutoCAD.Adapter.Windows.ViewModels
{
    public class ChildSymbolEditViewModel : SymbolEditViewModel
    {
        #region Properties

        #region Public Properties

        public IChildSymbol ChildSymbol => (IChildSymbol)_symbol;
        public bool TagHidden { get; set; }
        public DescriptionCollection Description { get; set; }
        public bool DescriptionHidden { get; set; }

        public CharacterCasing CharacterCasing { get; set; } = CharacterCasing.Upper;

        public ICommand SelectCommand { get; set; }

        #endregion

        #endregion

        #region Constructors

        public ChildSymbolEditViewModel(Window view, IChildSymbol symbol) : base(view, symbol)
        {
            Description = new DescriptionCollection(ChildSymbol.Description);
            DescriptionHidden = ChildSymbol.DescriptionHidden;
            OkCommand = new RelayCommand(UpdateSymbol);
            SelectCommand = new RelayCommand(SelectParent);
        }

        #endregion

        #region Methods

        #region Private Methods

        private void UpdateSymbol()
        {
            ChildSymbol.Description = Description.Select(d => d.Value).Where(v => v != null).ToList();
            ChildSymbol.DescriptionHidden = DescriptionHidden;
            ChildSymbol.CollapseAttributeStack();
            UpdateAndClose();
        }

        private void SelectParent()
        {
            IParentSymbol parent = ChildSymbol?.SelectParent();

            if (parent is null)
                return;

            Tag = parent.Tag;
            Description = new DescriptionCollection(parent.Description);
        }

        #endregion

        #endregion
    }
}

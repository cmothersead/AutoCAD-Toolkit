using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using ICA.AutoCAD.Adapter.Windows.ViewModels;
using ICA.AutoCAD.Adapter.Windows.Views;
using ICA.Schematic;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD.Adapter
{
    public class ChildSymbol : Symbol, IChildSymbol
    {
        #region Properties

        #region Private Properties

        protected override AttributeReference TagAttribute => BlockReference.GetAttributeReference("TAG2");

        private AttributeReference XrefAttribute => BlockReference.GetAttributeReference("XREF");

        private List<string> AttributeNames => new List<string>
        {
            "TAG",
            "MFG",
            "CAT",
            "DESC",
            "LOC",
            "INST"
        };

        private List<string> RequiredAttributes => new List<string>
        {
            "TAG2",
            "DESC1"
        };

        #endregion

        #region Public Properties

        public string Xref
        {
            get => XrefAttribute?.TextString;
            set => XrefAttribute?.SetValue(value);
        }

        #endregion

        #endregion

        #region Constructor

        public ChildSymbol(BlockReference blockReference) : base(blockReference)
        {
            Stack.Add(BlockReference.GetAttributeReferences()
                                    .Select(att => att.Tag)
                                    .Where(tag => AttributeNames.Any(att => tag.Contains(att)))
                                    .Union(RequiredAttributes));
        }

        #endregion

        #region Methods

        #region Public Methods

        public IParentSymbol SelectParent()
        {
            var components = Database.GetProject().Components
                                 .Where(component => component.Family == Family)
                                 .ToList();
            var view = new ComponentsListView(components);
            if (Application.ShowModalWindow(view) == true)
                return ((ComponentsListViewModel)view.DataContext).SelectedComponent.Symbol;

            return null;
        }

        public void SetParent(ParentSymbol parent)
        {
            Tag = parent.Tag;
            Xref = parent.LineNumber;
            Description = parent.Description;
        }

        #endregion

        #endregion
    }
}

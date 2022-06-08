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

        private string DictionaryName => "Children";

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
            if (Database != null)
            {
                Stack.Add(BlockReference.GetAttributeReferences()
                                        .Select(att => att.Tag)
                                        .Where(tag => AttributeNames.Any(att => tag.Contains(att)))
                                        .Union(RequiredAttributes));
                AddToDictionary(DictionaryName);
            }
        }

        #endregion

        #region Methods

        #region Public Methods

        public override bool Insert(Transaction transaction, Database database)
        {
            bool result = base.Insert(transaction, database);
            Stack.Add(BlockReference.GetAttributeReferences()
                                        .Select(att => att.Tag)
                                        .Where(tag => AttributeNames.Any(att => tag.Contains(att)))
                                        .Union(RequiredAttributes));
            AddToDictionary(DictionaryName);
            return result;
        }

        public IParentSymbol SelectParent()
        {
            var components = Database.GetProject().Components;
            components = components
                                     .Where(component => component.Family == Family)
                                     .ToList();
            var view = new ComponentsListView(components);
            if (Application.ShowModalWindow(view) == true && ((ComponentsListViewModel)view.DataContext).SelectedComponent is IComponent selected)
                return selected.Symbol;

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

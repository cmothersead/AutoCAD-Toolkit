using ICA.Schematic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter
{
    public class Component : IComponent
    {
        public string Tag
        {
            get => Symbol.Tag;
            set => Symbol.Tag = value;
        }
        public List<string> Description
        {
            get => Symbol.Description;
            set => Symbol.Description = value;
        }
        public string ManufacturerName
        {
            get => Symbol.ManufacturerName;
            set => Symbol.ManufacturerName = value;
        }
        public string PartNumber
        {
            get => Symbol.PartNumber;
            set => Symbol.PartNumber = value;
        }
        public string Family
        {
            get => Symbol.Family;
            set => Symbol.Family = value;
        }

        public IParentSymbol Symbol { get; }
        public List<IChildSymbol> Children { get; }

        public Project Project => ((ParentSymbol)Symbol).Database.GetProject();

        public Component(ParentSymbol parent)
        {
            Symbol = parent;
            //Children = Project.Drawings.SelectMany(drawing => drawing.GetChildSymbols(Tag))
            //                           .ToList();
        }

        public override string ToString() => Tag;

        public void UpdateTag()
        {
            ((ParentSymbol)Symbol).UpdateTag();
            //Children.ForEach(child => child.Tag = Tag);
        }
    }
}

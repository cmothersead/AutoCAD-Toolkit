using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ICA.AutoCAD.Adapter
{
    public class AttributeStack : IList<string>
    {
        private static List<string> Priority => new List<string>
        {
            "INST",
            "LOC",
            "DESC",
            "MFG",
            "CAT",
            "TAG"
        };

        private BlockReference Owner { get; }
        private Dictionary<string, LayerTableRecord> Layers { get; }
        public double TextHeight { get; set; } = 0.125;
        public double Spacing { get; set; } = 0.03125;
        public Point2d Position { get; set; }
        public AttachmentPoint Justification { get; set; }
        private List<string> Attributes { get; } = new List<string>();
        private List<AttributeReference> AttributeReferences => Attributes.Select(name => Owner.GetAttributeReference(name)).ToList();

        public int Count => Attributes.Count;

        public bool IsReadOnly => false;

        string IList<string>.this[int index] { get => Attributes[index]; set => Attributes[index] = value; }

        public AttributeStack(BlockReference owner, Dictionary<string, LayerTableRecord> layers)
        {
            Owner = owner;
            Layers = layers;
        }

        public void Collapse()
        {
            Point2d position = Position;
            foreach (AttributeReference reference in AttributeReferences)
            {
                reference.SetPosition(position.ToPoint3d());
                if (!(reference.Invisible | reference.TextString == ""))
                    position = new Point2d(position.X, position.Y + TextHeight + Spacing);
            }
        }

        private int PriorityOf(string item)
        {
            int value = Priority.IndexOf(Priority.Find(val => item.Contains(val))) * 100;
            Match match = Regex.Match(item, @"\d+");
            value += match.Success ? int.Parse(match.Value) : 0;
            return value;
        }

        private void AddAttribute(string name)
        {
            if (!Owner.HasAttributeReference(name))
            {
                AttributeReference reference = new AttributeReference
                {
                    Tag = name,
                    Position = Position.ToPoint3d(),
                    Justify = Justification,
                    Height = TextHeight,
                    LockPositionInBlock = true
                };
                reference.Invisible = !reference.Tag.Contains("TAG");
                Owner.AddAttributeReference(reference).SetLayer(GetLayer(name));
            }
                
            Attributes.Add(name);
        }

        private string GetLayer(string name)
        {
            LayerTableRecord layer = Layers.FirstOrDefault(pair => name.Contains(pair.Key)).Value;
            return Owner.Database.GetLayer(layer)?.Name;
        }

        public int IndexOf(string item) => Attributes.IndexOf(item);
        public void Insert(int index, string item) => throw new NotImplementedException();
        public void RemoveAt(int index) => Attributes.RemoveAt(index);
        public void Add(string item)
        {
            AddAttribute(item);
            Attributes.Sort();
            Attributes.Sort((x, y) => PriorityOf(y) - PriorityOf(x));
        }
        public void Add(IEnumerable<string> items)
        {
            items.ForEach(item => AddAttribute(item));
            Attributes.Sort();
            Attributes.Sort((x, y) => PriorityOf(y) - PriorityOf(x));
        }
        public void Clear() => Attributes.Clear();
        public bool Contains(string item) => Attributes.Contains(item);
        public void CopyTo(string[] array, int arrayIndex) => throw new NotImplementedException();
        public bool Remove(string item)
        {
            Owner.RemoveAttributeReference(item);
            return Attributes.Remove(item);
        }
        public IEnumerator<string> GetEnumerator() => Attributes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Attributes.GetEnumerator();
    }
}

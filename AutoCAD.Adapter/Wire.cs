using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class Wire
    {
        public List<Line> Lines { get; set; } = new List<Line>();

        public Wire()
        {
        }

        public Wire(ObjectIdCollection collection)
        {
            foreach(ObjectId id in collection)
                if (id.Open() is Line line)
                    if (line.Layer == ElectricalLayers.WireLayer.Name)
                        Lines.Add(line);
        }

        public void Highlight()
        {
            foreach (Line line in Lines)
                line.Highlight();
        }
    }
}

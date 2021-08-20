using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
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

        public static void Draw(Document document)
        {
            PromptPointResult result = document.Editor.GetPoint("\nSelect wire start point:");
            Point3d currentPoint = result.Value;
            do
            {
                Line line = DrawSegment(currentPoint);

                if (line is null)
                    break;

                currentPoint = line.EndPoint;
                line.Insert(document.Database);
                line.SetLayer(ElectricalLayers.WireLayer);

            } while (true);
        }

        private static Line DrawSegment(Point3d startPoint)
        {
            Line line = new Line(startPoint, Point3d.Origin);
            WireJig jig = new WireJig(line);

            if (jig.Run() != PromptStatus.OK)
                return null;

            return line;
        }
    }
}

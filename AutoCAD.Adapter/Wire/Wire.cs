using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using ICA.AutoCAD.Adapter.Prompt;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD.Adapter
{
    public class Wire
    {
        #region Properties

        #region Public Properties

        public List<Line> Lines { get; set; } = new List<Line>();

        public LayerTableRecord Layer
        {
            set => Lines.ForEach(line => line.SetLayer(value));
        }
        #endregion

        #endregion

        #region Constructors

        public Wire()
        {
        }

        public Wire(Line line)
        {
            if (line.Layer == ElectricalLayers.WireLayer.Name)
                Lines.AddRange(line.GetConnected(GetAllSegments(line.Database)));
        }

        public Wire(ObjectIdCollection collection)
        {
            foreach(ObjectId id in collection)
                if (id.Open() is Line line)
                    if (line.Layer == ElectricalLayers.WireLayer.Name)
                        Lines.Add(line);
        }

        #endregion

        #region Methods

        #region Public Methods

        public void Highlight()
        {
            foreach (Line line in Lines)
                line.Highlight();
        }

        public static void Draw(Document document)
        {
            //PromptPointResult result = document.Editor.GetPoint("\nSelect wire start point:");
            //Point3d currentPoint = result.Value;
            ParentSymbol symbol = Select.Symbol(document.Editor) as ParentSymbol;
            List<WireConnection> currentPoints = symbol.WireConnections;

            do
            {
                Line line = DrawSegment(currentPoints);

                if (line is null)
                    break;

                currentPoints = new List<WireConnection>() { new WireConnection(line) };
                line.Insert(document.Database);
                line.SetLayer(ElectricalLayers.WireLayer);

            } while (true);
        }

        private static Line DrawSegment(List<WireConnection> startPoints)
        {
            Line line = new Line(startPoints[0].Location.ToPoint3d(), Point3d.Origin);
            WireJig jig = new WireJig(line, startPoints);

            if (jig.Run() != PromptStatus.OK)
                return null;

            return line;
        }

        #endregion

        #region Public Static Methods

        public static List<Line> GetAllSegments(Database database) => database.GetLayer(ElectricalLayers.WireLayer)
                                                                              .GetEntities()
                                                                              .OfType<Line>()
                                                                              .ToList();

        #endregion

        #endregion
    }
}

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
                Lines = line.GetConnected(GetAllSegments(line.Database));
        }

        public Wire(ObjectIdCollection collection)
        {
            Lines = collection.Cast<ObjectId>()
                              .Select(id => id.Open())
                              .OfType<Line>()
                              .Where(line => line.Layer == ElectricalLayers.WireLayer.Name)
                              .ToList();
        }

        #endregion

        #region Methods

        #region Public Methods

        public void Highlight() => Lines.ForEach(line => line.Highlight());

        public static void Draw(Document document)
        {
            //PromptPointResult result = document.Editor.GetPoint("\nSelect wire start point:");
            //Point3d currentPoint = result.Value;
            Symbol symbol = Select.Symbol(document.Editor) as Symbol;
            List<WireConnection> currentPoints = symbol.WireConnections;

            Line previousLine = null, newLine;

            do
            {
                newLine = DrawSegment(currentPoints);

                if (newLine is null)
                    break;

                if (newLine.Angle == previousLine?.Angle)
                    using(Transaction transaction = document.Database.TransactionManager.StartTransaction())
                    {
                        previousLine.GetForWrite(transaction).EndPoint = newLine.EndPoint;
                        transaction.Commit();
                    }
                else
                {
                    currentPoints = new List<WireConnection>() { new WireConnection(newLine.EndPoint.ToPoint2D(), newLine.Angle) };
                    newLine.Insert(document.Database);
                    newLine.SetLayer(ElectricalLayers.WireLayer);

                    previousLine = newLine;
                }
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

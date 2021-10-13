using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD.Adapter
{
    public class Ladder : ILadder
    {
        #region Properties

        #region Private Properties

        private string _sheetNumber;
        private string SheetNumber
        {
            get
            {
                if (_sheetNumber != null)
                    return _sheetNumber;

                _sheetNumber = Database.GetSheetNumber();

                if (_sheetNumber is null | _sheetNumber == "")
                {
                    PromptStringOptions options = new PromptStringOptions("Sheet Number: ");
                    PromptResult result = Application.DocumentManager.MdiActiveDocument.Editor.GetString(options);
                    _sheetNumber = result.StringResult;
                    Database.SetSheetNumber(result.StringResult);
                }

                return _sheetNumber;
            }
        }

        #endregion

        #region Public Properties

        public Point2d Origin { get; }
        public double Height { get; }
        public double Width { get; }
        public double LineHeight { get; } = 0.5;
        public int FirstReference { get; } = 0;
        public int LastReference => FirstReference + (int)(Height / LineHeight);
        public int PhaseCount { get; } = 1;
        public Database Database { get; }

        private List<Line> _rails;
        public List<Line> Rails
        {
            get
            {
                if (_rails != null)
                    return _rails;

                _rails = new List<Line>();
                Origins.ForEach(origin => _rails.Add(new Rail(origin, Height)));
                return _rails;
            }
        }

        private List<BlockReference> _lineNumbers;
        public List<BlockReference> LineNumbers
        {
            get
            {
                if (_lineNumbers != null)
                    return _lineNumbers;

                _lineNumbers = new List<BlockReference>();
                int reference = FirstReference;
                for (double y = Origin.Y; Origin.Y - y <= Height; y -= LineHeight)
                {
                    _lineNumbers.Add(new LineNumber(Database, SheetNumber + reference.ToString("D2"), new Point2d(Origin.X, y)));
                    reference++;
                }
                return _lineNumbers;
            }
        }

        #endregion

        #endregion

        #region Subclass Members

        private List<Point2d> Origins
        {
            get
            {
                List<Point2d> value = new List<Point2d>();
                for (int i = 0; i <= PhaseCount; i++)
                    value.Add(new Point2d(Origin.X + i * Width, Origin.Y));
                if (PhaseCount == 3)
                    value.RemoveAt(PhaseCount);
                return value;
            }
        }

        private class Rail : Line
        {
            public Rail(Point2d top, Point2d bottom)
                : base(top.ToPoint3d(), bottom.ToPoint3d()) { }

            public Rail(Point2d top, double length)
                : base(top.ToPoint3d(), new Point3d(top.X, top.Y - length, 0)) { }
        }

        private class LineNumber : BlockReference
        {
            private readonly string _number;

            public LineNumber(Database database, string number, Point2d location) : base(location.ToPoint3d(), SchematicSymbolRecord.GetRecord(database, "LINENUMBER").ObjectId)
            {
                _number = number;
            }

            public void Insert(Transaction transaction, Database database)
            {
                this.Insert(transaction, database, new Dictionary<string, string>
                {
                    { "LINENUMBER", _number }
                });
            }
        }

        #endregion

        #region Constructors

        public Ladder(Database database, Point2d origin, double height, double width)
        {
            Database = database;
            Origin = origin;
            Height = height;
            Width = width;
        }

        public Ladder(Database database, Point2d origin, double height, double width, double lineHeight, int firstReference, int phaseCount)
        {
            Database = database;
            Origin = origin;
            Height = height;
            Width = width;
            LineHeight = lineHeight;
            FirstReference = firstReference;
            PhaseCount = phaseCount;
        }

        public Ladder(List<Entity> entities)
        {
            Database = entities[0].Database;

            _rails = entities.OfType<Line>()
                             .OrderBy(rail => rail.StartPoint.X)
                             .ToList();

            _lineNumbers = entities.OfType<BlockReference>()
                                   .OrderBy(linenumber => linenumber.GetAttributeValue("LINENUMBER"))
                                   .ToList();

            Origin = _rails[0].StartPoint.ToPoint2D();
            Height = _rails[0].StartPoint.Y - _rails[0].EndPoint.Y;
            Width = _rails[1].StartPoint.X - _rails[0].StartPoint.X;
            FirstReference = int.Parse(_lineNumbers[0].GetAttributeValue("LINENUMBER").Substring(SheetNumber.Length));
            LineHeight = _lineNumbers[0].Position.Y - _lineNumbers[1].Position.Y;
            PhaseCount = _rails.Count - 1;

            if (_rails.Count == 3)
                PhaseCount = 3;
        }

        #endregion

        #region Public Methods

        public void Insert()
        {
            using (Transaction transaction = Database.TransactionManager.StartTransaction())
            {
                Insert(transaction);
                transaction.Commit();
            }
        }

        public void Insert(Transaction transaction)
        {
            Rails.ForEach(rail => rail.Insert(transaction, Database, ElectricalLayers.LadderLayer));
            LineNumbers.ForEach(lineNumber => lineNumber.Insert(transaction, Database, ElectricalLayers.LadderLayer));
        }

        public string ClosestLineNumber(Point2d position) => LineNumbers.OrderByDescending(number => number.Position.X)
                                                                        .Where(number => number.Position.X < position.X)
                                                                        .Aggregate((closest, next) => Math.Abs(next.Position.Y - position.Y) < Math.Abs(closest.Position.Y - position.Y) ? next : closest)
                                                                        .GetAttributeValue("LINENUMBER");

        public string ClosestLineNumber(Point3d position) => ClosestLineNumber(position.ToPoint2D());

        #endregion

        #region Public Static Methods

        public static LadderTemplate Prompt(Document document = null)
        {
            if (document is null)
                document = Application.DocumentManager.MdiActiveDocument;

            if (document.Database.GetTitleBlock() == null)
            {
                document.Editor.WriteMessage("No title block found.");
                return null;
            }

            LadderTemplate template = new LadderTemplate(document.Database);

            PromptKeywordOptions typeOptions = new PromptKeywordOptions("\nChoose ladder type: ");
            typeOptions.Keywords.Add("1 Phase");
            typeOptions.Keywords.Add("3 Phase");
            typeOptions.Keywords.Default = "1 Phase";

            PromptKeywordOptions countOptions = new PromptKeywordOptions("\nChoose number of ladders: ");
            countOptions.Keywords.Add("1");
            if (template.Gap != 0)
                countOptions.Keywords.Add("2");
            countOptions.Keywords.Default = "1";
            PromptResult result = document.Editor.GetKeywords(typeOptions);
            if (result.Status != PromptStatus.OK)
                return null;

            template.PhaseCount = int.Parse(result.StringResult);

            if (countOptions.Keywords.Count > 1)
            {
                result = document.Editor.GetKeywords(countOptions);

                if (result.Status != PromptStatus.OK)
                    return null;

                template.LadderCount = int.Parse(result.StringResult);
            }

            return template;
        }

        public static void RemoveFrom(Database database)
        {
            if (!database.GetLayerTable().Has(ElectricalLayers.LadderLayer))
                return;

            LayerTableRecord ladderLayer = database.GetLayer(ElectricalLayers.LadderLayer);

            ladderLayer.UnlockWithoutWarning();
            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                ladderLayer.GetEntities(transaction)
                           .ForEach(entity => entity.Erase(transaction));
                transaction.Commit();
            }
            ladderLayer.LockWithWarning();
        }

        #endregion
    }
}

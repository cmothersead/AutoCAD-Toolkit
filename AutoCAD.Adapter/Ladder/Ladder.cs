using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class Ladder : Insertable, ILadder
    {
        #region Public Properties

        public Point2d Origin { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public double LineHeight { get; set; } = 0.5;
        public int FirstReference { get; set; } = 0;
        public int LastReference => FirstReference + (int)(Height / LineHeight);
        public int PhaseCount { get; set; } = 1;

        #endregion

        #region Private Properties

        private string _sheetNumber;
        private string SheetNumber
        {
            get
            {
                if (_sheetNumber != null)
                    return _sheetNumber;

                _sheetNumber = Application.DocumentManager.MdiActiveDocument.Database.GetPageNumber();

                if (_sheetNumber is null)
                {
                    Application.ShowAlertDialog("Sheet number not set."); //Have a set dialog instead of just an alert here
                    _sheetNumber = "1";
                }
                    
                return _sheetNumber;
            }
        }

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

        private List<Rail> Rails
        {
            get
            {
                List<Rail> value = new List<Rail>();
                foreach (Point2d origin in Origins)
                    value.Add(new Rail(origin, Height));
                return value;
            }
        }

        private class Rail : Line
        {
            public Rail(Point2d top, Point2d bottom)
                : base(top.ToPoint3d(), bottom.ToPoint3d())
            {
                Layer = Application.DocumentManager.MdiActiveDocument.Database.GetLayer(ElectricalLayers.LadderLayer).Name;
            }

            public Rail(Point2d top, double length)
                : base(top.ToPoint3d(), new Point3d(top.X, top.Y - length, 0))
            {
                Layer = Application.DocumentManager.MdiActiveDocument.Database.GetLayer(ElectricalLayers.LadderLayer).Name;
            }
        }

        private List<LineNumber> LineNumbers
        {
            get
            {
                List<LineNumber> list = new List<LineNumber>();
                int reference = FirstReference;
                for (double y = Origin.Y; Origin.Y - y <= Height; y -= LineHeight)
                {
                    list.Add(new LineNumber(SheetNumber + reference.ToString("D2"), new Point2d(Origin.X, y)));
                    reference++;
                }
                return list;
            }
        }

        private class LineNumber : BlockReference
        {
            private readonly string _number;

            public LineNumber(string number, Point2d location) : base(location.ToPoint3d(), SchematicSymbolRecord.GetRecord("LINENUMBER").ObjectId)
            {
                _number = number;
                Layer = "LADDER";
            }

            public void Insert(Database database, Transaction transaction)
            {
                this.Insert(database, transaction, new Dictionary<string, string>
                {
                    { "LINENUMBER", _number }
                });
            }
        }

        #endregion

        #region Constructors

        public Ladder() : base(null) { }

        #endregion

        #region Public Methods

        public override void Insert(Database database, Transaction transaction)
        {
            foreach (Rail rail in Rails)
                rail.Insert(database, transaction);

            foreach (LineNumber lineNumber in LineNumbers)
                lineNumber.Insert(database, transaction);
        }

        #endregion

        #region Public Static Methods

        public static LadderTemplate Prompt()
        {
            Document CurrentDocument = Application.DocumentManager.MdiActiveDocument;
            LadderTemplate template;

            PromptKeywordOptions typeOptions = new PromptKeywordOptions("\nChoose ladder type [1 Phase/3 Phase] <1>: ");
            typeOptions.Keywords.Add("1");
            typeOptions.Keywords.Add("3");

            PromptKeywordOptions countOptions = new PromptKeywordOptions("\nChoose number of ladders [1/2] <1>: ");
            countOptions.Keywords.Add("1");

            switch (CurrentDocument.Database.GetTitleBlock().Name)
            {
                case "ICA 8.5x11 Title Block":
                    template = new LadderTemplate()
                    {
                        Origin = new Point2d(2.5, 22.5),
                        Height = 19.5,
                        TotalWidth = 15
                    };
                    break;
                case "ICA 11x17 Title Block":
                    template = new LadderTemplate()
                    {
                        Origin = new Point2d(2.5, 22.5),
                        Height = 19.5,
                        TotalWidth = 32.5,
                        Gap = 2.5,
                    };
                    countOptions.Keywords.Add("2");
                    break;
                case "Nexteer 11x17 Title Block":
                    template = new LadderTemplate()
                    {
                        Origin = new Point2d(2.5, 22.5),
                        Height = 20,
                        TotalWidth = 25,
                        Gap = 5,
                    };
                    countOptions.Keywords.Add("2");
                    break;
                default:
                    return null;
            }

            PromptResult result = CurrentDocument.Editor.GetKeywords(typeOptions);
            if (result.Status == PromptStatus.OK)
            {
                template.PhaseCount = int.Parse(result.StringResult);
            }

            if (countOptions.Keywords.Count > 1)
            {
                result = CurrentDocument.Editor.GetKeywords(countOptions);
                if (result.Status == PromptStatus.OK)
                {
                    template.LadderCount = int.Parse(result.StringResult);
                }
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
                foreach (ObjectId id in database.GetLadder())
                    ((Entity)transaction.GetObject(id, OpenMode.ForWrite)).Erase();
                transaction.Commit();
            }
            ladderLayer.LockWithWarning();
        }

        #endregion
    }
}

using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD.Adapter
{
    public class DrawingSettings
    {
        #region Public Properties

        public Database Database { get; }
        public TitleBlockSettings TitleBlock { get; set; }

        public LadderSettings Ladder { get; set; }
        public ComponentSettings Component { get; set; }
        public WireSettings Wire { get; set; }
        public CrossReferenceSettings CrossReference { get; set; }

        #endregion

        #region Constructors

        public DrawingSettings() { }

        public DrawingSettings(Database database) 
        {
            Database = database;
            Ladder = new LadderSettings();
            Component = new ComponentSettings();
            Wire = new WireSettings();
            CrossReference = new CrossReferenceSettings();
            TitleBlock = new TitleBlockSettings();
        }

        public DrawingSettings(Dictionary<string, string>dictionary)
        {
            Ladder = new LadderSettings(dictionary);
            Component = new ComponentSettings(dictionary);
            Wire = new WireSettings(dictionary);
            CrossReference = new CrossReferenceSettings(dictionary);
            TitleBlock = new TitleBlockSettings(dictionary);
        }

        public DrawingSettings(ProjectSettings project)
        {
            Ladder = project.Ladder;
            Component = project.Component;
            Wire = project.Wire;
            CrossReference = project.CrossReference;
        }

        #endregion

        #region Public Methods

        public Dictionary<string, string> ToDictionary() => (Dictionary<string, string>)Ladder.ToDictionary()
                                                                                              .Concat(Component.ToDictionary())
                                                                                              .Concat(Wire.ToDictionary())
                                                                                              .Concat(CrossReference.ToDictionary());

        public static void Update()
        {
            // Must scan the database for active electrical components and update base on delta of properties
            throw new NotImplementedException();
        }

        #endregion
    }
}

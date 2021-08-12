using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class ElectricalDocumentProperties
    {
        #region Public Properties

        public Database Database { get; }
        public SheetProperties Sheet { get; set; }
        public LadderProperties Ladder { get; set; }
        public ComponentProperties Component { get; set; }
        public WireProperties Wire { get; set; }
        public CrossReferenceProperties CrossReference { get; set; }
        public TitleBlockProperties TitleBlock { get; set; }

        #endregion

        #region Constructors

        public ElectricalDocumentProperties(Database database) 
        {
            Database = database;
            Sheet = new SheetProperties();
            Ladder = new LadderProperties();
            Component = new ComponentProperties();
            Wire = new WireProperties();
            CrossReference = new CrossReferenceProperties();
            TitleBlock = new TitleBlockProperties();
        }

        public ElectricalDocumentProperties(Dictionary<string, string>dictionary)
        {
            Sheet = new SheetProperties(dictionary);
            Ladder = new LadderProperties(dictionary);
            Component = new ComponentProperties(dictionary);
            Wire = new WireProperties(dictionary);
            CrossReference = new CrossReferenceProperties(dictionary);
            TitleBlock = new TitleBlockProperties(dictionary);
        }

        public ElectricalDocumentProperties(ProjectProperties project)
        {
            Sheet = new SheetProperties()
            {
                Number = $"temp"
            };
            Ladder = project.Ladder;
            Component = project.Component;
            Wire = project.Wire;
        }

        #endregion

        #region Public Methods

        public Dictionary<string, string> ToDictionary()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (var property in Sheet.ToDictionary())
                dictionary.Add(property.Key, property.Value);
            foreach (var property in Ladder.ToDictionary())
                dictionary.Add(property.Key, property.Value);
            foreach (var property in Component.ToDictionary())
                dictionary.Add(property.Key, property.Value);
            foreach (var property in Wire.ToDictionary())
                dictionary.Add(property.Key, property.Value);
            foreach (var property in CrossReference.ToDictionary())
                dictionary.Add(property.Key, property.Value);
            return dictionary;
        }

        public static void Update()
        {
            // Must scan the database for active electrical components and update base on delta of properties
            throw new NotImplementedException();
        }

        #endregion
    }
}

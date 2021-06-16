using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class ElectricalDocumentProperties
    {
        public SheetProperties Sheet { get; set; }
        public LadderProperties Ladder { get; set; }
        public ComponentProperties Component { get; set; }
        public WireProperties Wire { get; set; }

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
            return dictionary;
        }

        public ElectricalDocumentProperties() { }

        public ElectricalDocumentProperties(Dictionary<string, string>dictionary)
        {
            Sheet = new SheetProperties(dictionary);
            Ladder = new LadderProperties(dictionary);
            Component = new ComponentProperties(dictionary);
            Wire = new WireProperties(dictionary);
        } 
    }
}

using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class ElectricalDocumentProperties
    {
        public SheetProperties SheetProperties { get; set; }
        public LadderProperties LadderProperties { get; set; }
        public ComponentProperties ComponentProperties { get; set; }
        public WireProperties WireProperties { get; set; }

        public Dictionary<string, string> ToDictionary()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (var property in SheetProperties.ToDictionary())
                dictionary.Add(property.Key, property.Value);
            foreach (var property in LadderProperties.ToDictionary())
                dictionary.Add(property.Key, property.Value);
            foreach (var property in ComponentProperties.ToDictionary())
                dictionary.Add(property.Key, property.Value);
            foreach (var property in WireProperties.ToDictionary())
                dictionary.Add(property.Key, property.Value);
            return dictionary;
        }

        public ElectricalDocumentProperties() { }

        public ElectricalDocumentProperties(Dictionary<string, string>dictionary)
        {
            SheetProperties = new SheetProperties(dictionary);
            LadderProperties = new LadderProperties(dictionary);
            ComponentProperties = new ComponentProperties(dictionary);
            WireProperties = new WireProperties(dictionary);
        } 
    }
}

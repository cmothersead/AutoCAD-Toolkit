using System.Xml;
using System.Xml.Serialization;

namespace ICA.AutoCAD.Adapter
{
    public class Job
    {
        [XmlAttribute]
        public int Id { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        public Customer Customer { get; set; }

        public string Code => $"{Customer.Id:D3}-{Id:D3}";

        public override string ToString() => $"{Customer.Id:D3}-{Id:D3} {Customer.Name} - {Name}";
    }
}

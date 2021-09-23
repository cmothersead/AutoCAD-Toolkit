using System.Text.RegularExpressions;
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

        public Job() { }

        public Job(string projectName)
        {
            Regex pattern = new Regex(@"(\d{3})-(\d{3}) (.*) - (.*)");
            Match match = pattern.Match(projectName);
            Id = int.Parse(match.Groups[2].Value);
            Name = match.Groups[4].Value;
            Customer = new Customer()
            {
                Id = int.Parse(match.Groups[1].Value),
                Name = match.Groups[3].Value
            };
        }

        public override string ToString() => $"{Customer.Id:D3}-{Id:D3} {Customer.Name} - {Name}";
    }
}

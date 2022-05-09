using ICA.Schematic;
using System.Xml.Serialization;

namespace ICA.AutoCAD.Adapter
{
    public class TBAttribute : ITBAttribute
    {
        [XmlAttribute]
        public string Tag { get; set; }
        [XmlText]
        public string Value { get; set; }

        public override int GetHashCode() => base.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(TBAttribute))
                return false;

            return ((TBAttribute)obj).Tag == Tag;
        }
    }
}

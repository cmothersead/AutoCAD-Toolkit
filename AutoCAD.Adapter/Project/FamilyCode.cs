using System.Xml.Serialization;

namespace ICA.AutoCAD.Adapter
{
    public class FamilyCode
    {
        #region Properties

        #region Public Properties

        [XmlAttribute]
        public string Replace { get; set; }
        [XmlAttribute]
        public string With { get; set; }

        #endregion

        #endregion
    }
}

using ICA.AutoCAD.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ICA.AutoCAD.Adapter
{
    public class ProjectSettings
    {
        public string SchematicTemplatePath { get; set; } = $"{Paths.Templates}\\ICA 11X17 Title Block.dwt";
        public string PanelTemplatePath { get; set; } = $"{Paths.Templates}\\ICA 11X17 Title Block.dwt";
        public string ReferenceTemplatePath { get; set; } = $"{Paths.Templates}\\ICA 11X17 Title Block.dwt";
        public string Library { get; set; } = Paths.Libraries;
        public string FileNameFormat = "%PPG%S";
        [XmlArrayItem("Family")]
        public List<FamilyCode> FamilyCodes { get; set; }
        [XmlArrayItem("Attribute")]
        public List<TBAttribute> TitleBlockAttributes { get; set; }
        public LadderSettings Ladder { get; set; } = new LadderSettings();
        public ComponentSettings Component { get; set; } = new ComponentSettings();
        public WireSettings Wire { get; set; } = new WireSettings();
        public CrossReferenceSettings CrossReference { get; set; } = new CrossReferenceSettings();

        public ProjectSettings()
        {

        }

        public ProjectSettings(Dictionary<string, string> dictionary)
        {
            Ladder = new LadderSettings(dictionary);
            Component = new ComponentSettings(dictionary);
            Wire = new WireSettings(dictionary);
            CrossReference = new CrossReferenceSettings(dictionary);
        }
    }
}

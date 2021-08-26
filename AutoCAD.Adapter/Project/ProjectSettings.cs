using ICA.AutoCAD.IO;
using System;
using System.Collections.Generic;

namespace ICA.AutoCAD.Adapter
{
    public class ProjectSettings
    {
        public Uri SchematicTemplate { get; set; } = new Uri($"{Paths.Templates}\\ICA 11X17 Title Block.dwg");
        public Uri PanelTemplate { get; set; } = new Uri($"{Paths.Templates}\\ICA 11X17 Title Block.dwg");
        public Uri ReferenceTemplate { get; set; } = new Uri($"{Paths.Templates}\\ICA 11X17 Title Block.dwg");
        public Uri Library { get; set; } = new Uri(Paths.Libraries);
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

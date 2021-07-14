namespace ICA.Schematic
{
    public interface IHideableAttributes
    {
        bool DescriptionHidden { get; set; }
        bool InstallationHidden { get; set; }
        bool PartInfoHidden { get; set; }

        void CollapseAttributeStack();
    }
}

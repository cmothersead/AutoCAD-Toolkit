namespace ICA.Schematic
{
    public interface IHideableAttributes
    {
        bool DescriptionHidden { get; set; }

        void CollapseAttributeStack();
    }
}

namespace ICA.Schematic.Data.Parts
{
    public interface IContact
    {
        ITerminal[] Terminals { get; set; }
        int MaxCurrent { get; set; }
    }
}

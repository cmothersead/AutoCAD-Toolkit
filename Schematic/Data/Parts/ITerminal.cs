namespace ICA.Schematic.Data.Parts
{
    public interface ITerminal
    {
        string TerminalNumber { get; set; }
        string TerminalDescription { get; set; }
        Potential ExpectedPotential { get; set; }
    }
}

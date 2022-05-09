namespace ICA.Schematic.Data.Parts.Contacts
{
    public class PowerContact : IContact
    {
        public ITerminal[] Terminals { get; set; } = new ITerminal[2];
        public int MaxCurrent { get; set; }
    }
}

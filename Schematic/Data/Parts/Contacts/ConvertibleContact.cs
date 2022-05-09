namespace ICA.Schematic.Data.Parts.Contacts
{
    public class ConvertibleContact : IContact
    {
        public ITerminal[] Terminals { get; set; } = new ITerminal[3];
        public int MaxCurrent { get; set; }
    }
}

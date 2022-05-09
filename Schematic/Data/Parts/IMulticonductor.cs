namespace ICA.Schematic.Data.Parts
{
    interface IMulticonductor : ICable
    {
        bool Shielded { get; set; }
    }
}

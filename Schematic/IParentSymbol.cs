
namespace ICA.Schematic
{
    public interface IParentSymbol : ISymbol, IHideableAttributes
    {
        string Enclosure { get; set; }
        string Location { get; set; }
        string ManufacturerName { get; set; }
        string PartNumber { get; set; }

        bool InstallationHidden { get; set; }
        bool PartInfoHidden { get; set; }

        void UpdateTag(string format = null);
    }
}
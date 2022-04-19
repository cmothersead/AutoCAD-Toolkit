
namespace ICA.Schematic
{
    public interface IParentSymbol : ISymbol, IHideableAttributes
    {
        string Enclosure { get; set; }
        string Location { get; set; }
        string ManufacturerName { get; set; }
        string PartNumber { get; set; }
        string Rating { get; set; }

        bool InstallationHidden { get; set; }
        bool PartInfoHidden { get; set; }
        bool RatingHidden { get; set; }
        
        void UpdateTag(string format = null);
    }
}
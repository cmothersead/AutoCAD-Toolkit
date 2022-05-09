namespace ICA.Schematic.Data
{
    public interface IPart
    {
        int Id { get; set; }
        Family Family { get; }
        Manufacturer Manufacturer { get; set; }
        int FamilyId { get; }
        int ManufacturerId { get; }
        string Number { get; set; }
    }
}

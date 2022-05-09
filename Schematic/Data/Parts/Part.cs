namespace ICA.Schematic.Data
{
    public class Part : IPart
    {
        public int Id { get; set; }
        public int FamilyId => Family.Id;
        public int ManufacturerId => Manufacturer.Id;
        public Family Family { get; }
        public Manufacturer Manufacturer { get; set; }
        public string Number { get; set; }
    }
}

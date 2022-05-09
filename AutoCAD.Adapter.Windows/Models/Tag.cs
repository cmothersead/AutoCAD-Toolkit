namespace ICA.AutoCAD.Adapter.Windows.Models
{
    public class Tag
    {
        private string _value;
        public string Value
        {
            get => _value;
            set => _value = value.ToUpper();
        }
    }
}

using Autodesk.AutoCAD.DatabaseServices;

namespace ICA.AutoCAD
{
    public class XData
    {
        public DxfCode TypeCode { get; set; }
        public object Value { get; set; }

        public XData() { }

        public XData(DxfCode typeCode, object value)
        {
            TypeCode = typeCode;
            Value = value;
        }

        public XData(TypedValue value)
        {
            TypeCode = (DxfCode)value.TypeCode;
            Value = value.Value;
        }

        public static implicit operator TypedValue(XData value) => new TypedValue((int)value.TypeCode, value.Value);
        public static explicit operator XData(TypedValue value) => new XData(value);

        public override string ToString() => Value.ToString();

        public override int GetHashCode() => base.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is XData data)
                return data.TypeCode.Equals(TypeCode) && data.Value.Equals(Value);

            return false;
        }

        public static XData RegAppName => new XData(DxfCode.ExtendedDataRegAppName, "ICA");
    }
}

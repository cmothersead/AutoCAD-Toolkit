using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Linq;

namespace ICA.AutoCAD.Adapter
{
    public static class DBObjectExtensions
    {
        #region Public Transactionless Extension Methods

        public static bool HasXData(this DBObject obj) => obj.XData != null;

        #endregion

        #region Public Extension Methods

        public static bool HasXData(this DBObject obj, Transaction transaction, XData value) => obj.GetXData(transaction).Contains(value);

        public static bool HasXData(this DBObject obj, Transaction transaction, DxfCode typeCode, object value) => obj.HasXData(transaction, new XData(typeCode, value));

        public static List<XData> GetXData(this DBObject obj, Transaction transaction)
        {
            if (obj.XData is null)
                return new List<XData>();

            return obj.XData.Cast<TypedValue>()
                            .Select(value => new XData(value))
                            .Where(data => !data.Equals(XData.RegAppName))
                            .ToList();
        }

        public static void SetXData(this DBObject obj, Transaction transaction, List<XData> values)
        {
            obj.Database.AddRegApp(transaction);
            ResultBuffer buffer = new ResultBuffer() { XData.RegAppName };
            values.ForEach(value => buffer.Add(value));
            obj.GetForWrite(transaction).XData = buffer;
        }

        public static void ClearXData(this DBObject obj, Transaction transaction) => obj.SetXData(transaction, new List<XData>());

        public static void AddXData(this DBObject obj, Transaction transaction, XData value)
        {
            List<XData> xData = obj.GetXData(transaction);
            xData.Add(value);
            obj.SetXData(transaction, xData);
        }

        public static void AddXData(this DBObject obj, Transaction transaction, DxfCode typeCode, object value) => obj.AddXData(new XData(typeCode, value));

        public static void RemoveXData(this DBObject obj, Transaction transaction, XData value)
        {
            List<XData> xData = obj.GetXData(transaction);
            xData.Remove(value);
            obj.SetXData(transaction, xData);
        }

        public static void RemoveXData(this DBObject obj, Transaction transaction, DxfCode typeCode, object value) => obj.RemoveXData(new XData(typeCode, value));

        #endregion

        #region Transacted Overloads

        public static bool HasXData(this DBObject obj, XData value) => obj.Transact(HasXData, value);

        public static bool HasXData(this DBObject obj, DxfCode typeCode, object value) => obj.Transact(HasXData, typeCode, value);

        public static List<XData> GetXData(this DBObject obj) => obj.Transact(GetXData);

        public static void SetXData(this DBObject obj, List<XData> values) => obj.Transact(SetXData, values);

        public static void ClearXData(this DBObject obj) => obj.Transact(ClearXData);

        public static void AddXData(this DBObject obj, XData value) => obj.Transact(AddXData, value);

        public static void AddXData(this DBObject obj, DxfCode typeCode, object value) => obj.Transact(AddXData, typeCode, value);

        public static void RemoveXData(this DBObject obj, XData value) => obj.Transact(RemoveXData, value);

        public static void RemoveXData(this DBObject obj, DxfCode typeCode, object value) => obj.Transact(RemoveXData, typeCode, value);

        #endregion
    }
}

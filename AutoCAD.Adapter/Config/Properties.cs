using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ICA.AutoCAD.Adapter
{
    public class Properties
    {
        public Dictionary<string, string> ToDictionary()
        {
            PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (PropertyInfo info in properties)
            {
                var value = info.GetValue(this, null);
                var test = value.GetType();
            }
            return null;
        }
    }
}

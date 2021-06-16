using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ICA.AutoCAD.Adapter
{
    public class Properties
    {
        public Properties() { }

        public Properties(Dictionary<string, string> dictionary)
        {
            PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo property in properties)
            {
                try
                {
                    if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(this, int.Parse(dictionary[property.Name]));
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        property.SetValue(this, double.Parse(dictionary[property.Name]));
                    }
                    else if (property.PropertyType == typeof(List<string>))
                    {
                        property.SetValue(this, dictionary[property.Name].Split(',').ToList());
                    }
                    else if (property.PropertyType == typeof(Enum))
                    {
                        property.SetValue(this, Enum.Parse(property.PropertyType, dictionary[property.Name]));
                    }
                    else
                    {
                        property.SetValue(this, dictionary[property.Name]);
                    }
                }
                catch { }
            }
        }

        public Dictionary<string, string> ToDictionary()
        {
            PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (PropertyInfo property in properties)
            {
                var value = property.GetValue(this, null);
                if (value is null)
                    dictionary.Add(property.Name, "");
                else if (value is IEnumerable<string> enumerable)
                    dictionary.Add(property.Name, string.Join(",", enumerable));
                else
                    dictionary.Add(property.Name, value.ToString());
            }
            return dictionary;
        }
    }
}

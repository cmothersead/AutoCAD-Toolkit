using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ICA.AutoCAD.Adapter
{
    public class Properties
    {
        #region Constructors

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
                    else if (property.PropertyType == typeof(LeaderInsertMode))
                    {
                        LeaderInsertMode value;
                        switch (dictionary[property.Name])
                        {
                            case "0":
                            case nameof(LeaderInsertMode.AsRequired):
                                value = LeaderInsertMode.AsRequired;
                                break;
                            case "1":
                            case nameof(LeaderInsertMode.Always):
                                value = LeaderInsertMode.Always;
                                break;
                            default:
                                value = LeaderInsertMode.Never;
                                break;
                        }
                        property.SetValue(this, value);
                    }
                    else if(property.PropertyType == typeof(NumberMode))
                    {
                        NumberMode value;
                        switch (dictionary[property.Name])
                        {
                            case "S":
                            case nameof(NumberMode.Sequentical):
                                value = NumberMode.Sequentical;
                                break;
                            default:
                                value = NumberMode.Referential;
                                break;
                        }
                        property.SetValue(this, value);
                    }
                    else if(property.PropertyType ==typeof(RungOrientation))
                    {
                        RungOrientation value;
                        switch (dictionary[property.Name])
                        {
                            case "V":
                            case nameof(RungOrientation.Vertical):
                                value = RungOrientation.Vertical;
                                break;
                            default:
                                value = RungOrientation.Horizontal;
                                break;
                        }
                        property.SetValue(this, value);
                    }
                    else if(property.PropertyType == typeof(WireGapStyle))
                    {
                        WireGapStyle value;
                        switch(dictionary[property.Name])
                        {
                            case "0":
                            case nameof(WireGapStyle.Gap):
                                value = WireGapStyle.Gap;
                                break;
                            case "2":
                            case nameof(WireGapStyle.Cross):
                                value = WireGapStyle.Cross;
                                break;
                            default:
                                value = WireGapStyle.Loop;
                                break;
                        }
                        property.SetValue(this, value);
                    }
                    else if(property.PropertyType == typeof(WireSortMode?))
                    {
                        WireSortMode? value;
                        switch(dictionary[property.Name])
                        {
                            case "0":
                            case nameof(WireSortMode.LeftRightTopBottom):
                                value = WireSortMode.LeftRightTopBottom;
                                break;
                            case "1":
                            case nameof(WireSortMode.LeftRightBottomTop):
                                value = WireSortMode.LeftRightBottomTop;
                                break;
                            case "2":
                            case nameof(WireSortMode.RightLeftTopBottom):
                                value = WireSortMode.RightLeftTopBottom;
                                break;
                            case "3":
                            case nameof(WireSortMode.RightLeftBottomTop):
                                value = WireSortMode.RightLeftBottomTop;
                                break;
                            case "4":
                            case nameof(WireSortMode.TopBottomLeftRight):
                                value = WireSortMode.TopBottomLeftRight;
                                break;
                            case "5":
                            case nameof(WireSortMode.BottomTopLeftRight):
                                value = WireSortMode.BottomTopLeftRight;
                                break;
                            case "6":
                            case nameof(WireSortMode.TopBottomRightLeft):
                                value = WireSortMode.TopBottomRightLeft;
                                break;
                            case "7":
                            case nameof(WireSortMode.BottomTopRightLeft):
                                value = WireSortMode.BottomTopRightLeft;
                                break;
                            default:
                                value = null;
                                break;
                        }
                        property.SetValue(this, value);
                    }
                    else
                    {
                        property.SetValue(this, dictionary[property.Name]);
                    }
                }
                catch { }
            }
        }

        #endregion

        #region Public Methods

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

        #endregion
    }
}

using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ICA.AutoCAD.Adapter
{
    public class Settings
    {
        #region Constructors

        public Settings() { }

        public Settings(string prefix, Dictionary<string, string> dictionary)
        {
            PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo property in properties)
            {
                try
                {
                    //Ifs used because switch/case not available of type until C#9
                    string value = dictionary[prefix + property.Name];
                    if (property.PropertyType == typeof(int))
                        property.SetValue(this, Convert.ToInt(value));
                    else if (property.PropertyType == typeof(double))
                        property.SetValue(this, Convert.ToDouble(value));
                    else if (property.PropertyType == typeof(List<string>))
                        property.SetValue(this, Convert.ToList(value));
                    else if (property.PropertyType == typeof(Point2d))
                        property.SetValue(this, Convert.ToPoint(value));
                    else if (property.PropertyType == typeof(LeaderInsertMode))
                        property.SetValue(this, Convert.ToLeaderInsertMode(value));
                    else if (property.PropertyType == typeof(NumberMode))
                        property.SetValue(this, Convert.ToNumberMode(value));
                    else if (property.PropertyType == typeof(RungOrientation))
                        property.SetValue(this, Convert.ToRungOrientation(value));
                    else if (property.PropertyType == typeof(WireGapStyle))
                        property.SetValue(this, Convert.ToWireGapStyle(value));
                    else if (property.PropertyType == typeof(WireSortMode?))
                        property.SetValue(this, Convert.ToWireSortMode(value));
                    else
                        property.SetValue(this, value);
                }
                catch { }
            }
        }

        #endregion

        #region Public Methods

        public Dictionary<string, string> ToDictionary(string prefix) => GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                                                  .ToDictionary(property => prefix + property.Name, property => Convert.ToString(property.GetValue(this, null)));

        #endregion

        #region Converter Subclass

        public class Convert
        {
            public static string ToString(object value)
            {
                switch (value)
                {
                    case null:
                        return "";
                    case IEnumerable<object> enumerable:
                        return string.Join(",", enumerable);
                    case Point2d point:
                        return $"{point.X},{point.Y}";
                    default:
                        return value.ToString();
                }
            }

            public static int ToInt(string value, int defaultValue = default) => int.TryParse(value, out int result) ? result : defaultValue;

            public static double ToDouble(string value, double defaultValue = default) => double.TryParse(value, out double result) ? result : defaultValue;

            public static List<string> ToList(string value, char delimiter = ',') => value.Split(delimiter).ToList();

            public static Point2d ToPoint(string value, char delimiter = ',') => new Point2d(value.Split(delimiter).Select(s => double.Parse(s)).ToArray());

            public static LeaderInsertMode ToLeaderInsertMode(string value, LeaderInsertMode defaultValue = LeaderInsertMode.Never)
            {
                switch (value)
                {
                    case "0":
                    case nameof(LeaderInsertMode.AsRequired):
                        return LeaderInsertMode.AsRequired;
                    case "1":
                    case nameof(LeaderInsertMode.Always):
                        return LeaderInsertMode.Always;
                    case "2":
                    case nameof(LeaderInsertMode.Never):
                        return LeaderInsertMode.Never;
                    default:
                        return defaultValue;
                }
            }

            public static NumberMode ToNumberMode(string value, NumberMode defaultValue = NumberMode.Referential)
            {
                switch (value)
                {
                    case "R":
                    case nameof(NumberMode.Referential):
                        return NumberMode.Referential;
                    case "S":
                    case nameof(NumberMode.Sequentical):
                        return NumberMode.Sequentical;
                    default:
                        return defaultValue;
                }
            }

            public static RungOrientation ToRungOrientation(string value, RungOrientation defaultValue = RungOrientation.Horizontal)
            {
                switch (value)
                {
                    case "H":
                    case nameof(RungOrientation.Horizontal):
                        return RungOrientation.Horizontal;
                    case "V":
                    case nameof(RungOrientation.Vertical):
                        return RungOrientation.Vertical;
                    default:
                        return defaultValue;
                }
            }

            public static WireGapStyle ToWireGapStyle(string value, WireGapStyle defaultValue = WireGapStyle.Loop)
            {
                switch (value)
                {
                    case "0":
                    case nameof(WireGapStyle.Gap):
                        return WireGapStyle.Gap;
                    case "1":
                    case nameof(WireGapStyle.Loop):
                        return WireGapStyle.Loop;
                    case "2":
                    case nameof(WireGapStyle.Cross):
                        return WireGapStyle.Cross;
                    default:
                        return defaultValue;
                }
            }

            public static WireSortMode? ToWireSortMode(string value, WireSortMode? defaultValue = null)
            {
                switch (value)
                {
                    case "0":
                    case nameof(WireSortMode.LeftRightTopBottom):
                        return WireSortMode.LeftRightTopBottom;
                    case "1":
                    case nameof(WireSortMode.LeftRightBottomTop):
                        return WireSortMode.LeftRightBottomTop;
                    case "2":
                    case nameof(WireSortMode.RightLeftTopBottom):
                        return WireSortMode.RightLeftTopBottom;
                    case "3":
                    case nameof(WireSortMode.RightLeftBottomTop):
                        return WireSortMode.RightLeftBottomTop;
                    case "4":
                    case nameof(WireSortMode.TopBottomLeftRight):
                        return WireSortMode.TopBottomLeftRight;
                    case "5":
                    case nameof(WireSortMode.BottomTopLeftRight):
                        return WireSortMode.BottomTopLeftRight;
                    case "6":
                    case nameof(WireSortMode.TopBottomRightLeft):
                        return WireSortMode.TopBottomRightLeft;
                    case "7":
                    case nameof(WireSortMode.BottomTopRightLeft):
                        return WireSortMode.BottomTopRightLeft;
                    default:
                        return defaultValue;
                }
            }
        }

        #endregion
    }
}

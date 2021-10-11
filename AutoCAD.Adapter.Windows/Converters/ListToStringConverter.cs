using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ICA.AutoCAD.Adapter.Windows.Converters
{
    [ValueConversion(typeof(List<string>), typeof(string))]
    public class ListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
                throw new InvalidOperationException("Target must be a string.");

            return string.Join("\n", ((List<string>)value).ToArray());
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

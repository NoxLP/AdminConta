using System;
using System.Windows.Data;

namespace Converters
{
    /// <summary>
    /// Converter for TabbedExpander Asiento datagrid height.
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string param = parameter as string;

            if (value == null || targetType == null)
                return null;
            else if (parameter == null)
                return value;
            
            switch (param)
            {
                case "PADLEFT2":
                    return ((string)value).PadLeft(2, '0');
                case "PADLEFT4":
                    return ((string)value).PadLeft(4, '0');
                case "PADLEFT10":
                    return ((string)value).PadLeft(10, '0');
                case "DATEd":
                    return ((DateTime)value).ToString("d");
                default:
                    return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}

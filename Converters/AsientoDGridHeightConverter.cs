using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Converters
{
    /// <summary>
    /// Converter for TabbedExpander Asiento datagrid height.
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    public class AsientoDGridHeightConverter : IValueConverter
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
                case "AS":
                    double dValue = (double)value;
                    return dValue - 110;
                case "TABBEDDIARIO":
                    dValue = (double)value;
                    return dValue - 83;
                case "WINDOWED":
                    dValue = (double)value;
                    return dValue - 86;
                case "VMSECHEIGHT":
                    //GridLength gValue = (GridLength)value;
                    //return gValue.Value;
                    dValue = (double)value;
                    return dValue;
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

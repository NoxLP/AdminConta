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
    public class HeightToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string param = parameter as string;
            
            if (value == null || targetType == null)
                return null;
            else if (parameter == null)
                return value;

            double height = (double)value;
            switch (param)
            {
                case "NORMAL":
                    GridLength gl = (height == double.NaN ? new GridLength(1, GridUnitType.Auto) : new GridLength(height));
                    return gl;
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

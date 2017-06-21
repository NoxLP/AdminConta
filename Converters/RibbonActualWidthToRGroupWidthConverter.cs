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
    /// Converter for Ribbon width stretcher.
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    public class RibbonActualWidthToRGroupWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string param = parameter as string;

            if (value == null || targetType == null)
                return null;
            else if (parameter == null)
                return value;
            else if (param.Contains("NORMAL"))
            {
                double dValue = (double)value;
                return dValue - 5;
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Converters
{
    public class BoolLogicalMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string param = parameter as string;

            if (values == null || targetType == null || parameter == null)
                return null;
            
            switch (param)
            {
                case "AND":
                    bool result = true;
                    foreach(object value in values)
                    {
                        result = result && (bool)value;
                    }
                    return result;
                case "OR":
                    result = false;
                    foreach(object value in values)
                    {
                        result = result || (bool)value;
                    }
                    return result;
                default:
                    return null;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class BoolLogicalConverter : IValueConverter
    {
        public object Convert(object values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string param = parameter as string;

            if (values == null || targetType == null || parameter == null)
                return null;

            switch (param)
            {
                case "NOT":
                    bool result = true;
                    
                    return !result;
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}

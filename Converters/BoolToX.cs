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
    /// Converter for Ribbon height expander behaviour.
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    public class BoolToHeightConverter : IValueConverter
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
                case "TogToGrid":
                    bool bValue = (bool)value;
                    return bValue ?
                        new System.Windows.GridLength(13.5d, System.Windows.GridUnitType.Pixel) :
                        new System.Windows.GridLength(85d, System.Windows.GridUnitType.Pixel);
                case "TEIsExpandedToSplitterHeight":
                    bValue = (bool)value;
                    return bValue ? 4 : 0;
                default:
                    return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class BoolHeightToHeightMulticonverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string param = parameter as string;

            if (values == null || targetType == null)
                return null;
            else if (parameter == null)
                return null;

            bool IsExpanded = (bool)values[0];
            double dProperty = (double)values[1];
            double notExpandedHeight = (double)values[2];
            switch(param)
            {
                case "GRID":
                    return (IsExpanded ? new GridLength(dProperty) : new GridLength(notExpandedHeight));
                default:
                    return null;
            }

            /*bool bValue = (bool)values[0];
            double gridValue = (double)values[1];
            double TEValue = (double)values[2];
            switch (param)
            {
                case "RowToTE":
                    return bValue ?
                        gridValue :
                        TEValue;
                case "TEToRow":
                    return bValue ?
                        new GridLength(gridValue, GridUnitType.Pixel) :
                        new GridLength(TEValue + 5, GridUnitType.Pixel);
                default:
                    return null;
            }*/
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    /// <summary>
    /// Converter for Splitter visibility.
    /// </summary>
    [ValueConversion(typeof(object), typeof(bool))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string param = parameter as string;

            if (value == null || targetType == null)
                return null;
            else if (parameter == null)
                return null;

            switch (param)
            {
                case "TESplitter":
                    bool bValue = (bool)value;
                    return bValue ? Visibility.Visible : Visibility.Collapsed;
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

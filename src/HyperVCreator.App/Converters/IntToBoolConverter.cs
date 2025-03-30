using System;
using System.Globalization;
using System.Windows.Data;

namespace HyperVCreator.App.Converters
{
    /// <summary>
    /// Converts an integer value to boolean based on if it equals a parameter value
    /// </summary>
    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue && parameter != null)
            {
                if (int.TryParse(parameter.ToString(), out int compareValue))
                {
                    return intValue == compareValue;
                }
            }
            
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter != null)
            {
                if (int.TryParse(parameter.ToString(), out int result))
                {
                    return result;
                }
            }
            
            return 0;
        }
    }
} 
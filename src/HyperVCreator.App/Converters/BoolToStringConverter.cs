using System;
using System.Globalization;
using System.Windows.Data;

namespace HyperVCreator.App.Converters
{
    /// <summary>
    /// Converts a boolean value to one of two strings.
    /// </summary>
    public class BoolToStringConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to one of two strings.
        /// </summary>
        /// <param name="value">The boolean value.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">A string parameter containing two values separated by a semicolon. The first value is used when true, the second when false.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>The first string if the value is true, the second string if the value is false.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string strParameter)
            {
                string[] options = strParameter.Split(';');
                if (options.Length >= 2)
                {
                    return boolValue ? options[0] : options[1];
                }
                else if (options.Length == 1)
                {
                    return boolValue ? options[0] : string.Empty;
                }
            }
            
            return value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Converts a string value back to a boolean.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">A string parameter containing two values separated by a semicolon.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>True if the string equals the first option, otherwise false.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue && parameter is string strParameter)
            {
                string[] options = strParameter.Split(';');
                if (options.Length >= 1)
                {
                    return strValue.Equals(options[0], StringComparison.OrdinalIgnoreCase);
                }
            }
            
            return false;
        }
    }
} 
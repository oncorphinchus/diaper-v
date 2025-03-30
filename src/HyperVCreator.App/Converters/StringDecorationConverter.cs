using System;
using System.Globalization;
using System.Windows.Data;

namespace HyperVCreator.App.Converters
{
    /// <summary>
    /// Decorates a string with a specified format
    /// </summary>
    public class StringDecorationConverter : IValueConverter
    {
        /// <summary>
        /// Decorates a string with a specified format.
        /// </summary>
        /// <param name="value">The string to decorate.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The format string, with {0} placeholder for the input value.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>The decorated string.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            string input = value.ToString();
            
            if (parameter is string format && !string.IsNullOrEmpty(format))
            {
                return string.Format(format, input);
            }
            
            return input;
        }

        /// <summary>
        /// Not implemented for this converter.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 
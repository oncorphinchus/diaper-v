using System;
using System.Globalization;
using System.Windows.Data;

namespace HyperVCreator.App.Converters
{
    /// <summary>
    /// Inverts a boolean value.
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Inverts a boolean value.
        /// </summary>
        /// <param name="value">The boolean value to invert.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>The inverted boolean value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            
            return value;
        }

        /// <summary>
        /// Inverts a boolean value.
        /// </summary>
        /// <param name="value">The boolean value to invert.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>The inverted boolean value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            
            return value;
        }
    }
} 
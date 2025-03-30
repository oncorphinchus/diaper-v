using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HyperVCreator.App.Converters
{
    /// <summary>
    /// Converts a boolean value to a Visibility value.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a Visibility value.
        /// </summary>
        /// <param name="value">The boolean value.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">A parameter to customize the conversion. If "Inverse" is specified, the result is inverted.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>Visibility.Visible if the value is true, otherwise Visibility.Collapsed.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = false;
            if (value is bool boolValue)
            {
                visible = boolValue;
            }

            // Check if we should invert the result
            if (parameter != null && parameter.ToString().Equals("Inverse", StringComparison.OrdinalIgnoreCase))
            {
                visible = !visible;
            }

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a Visibility value back to a boolean value.
        /// </summary>
        /// <param name="value">The Visibility value.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">A parameter to customize the conversion. If "Inverse" is specified, the result is inverted.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>True if the value is Visibility.Visible, otherwise false.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;
            if (value is Visibility visibility)
            {
                result = visibility == Visibility.Visible;
            }

            // Check if we should invert the result
            if (parameter != null && parameter.ToString().Equals("Inverse", StringComparison.OrdinalIgnoreCase))
            {
                result = !result;
            }

            return result;
        }
    }
} 
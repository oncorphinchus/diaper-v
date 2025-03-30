using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HyperVCreator.App.Converters
{
    /// <summary>
    /// Converts a string value to a Visibility value.
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a string value to a Visibility value.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">A parameter to customize the conversion. If "Inverse" is specified, the result is inverted.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>Visibility.Visible if the string is not null or empty, otherwise Visibility.Collapsed.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = false;
            if (value is string stringValue)
            {
                visible = !string.IsNullOrWhiteSpace(stringValue);
            }

            // Check if we should invert the result
            if (parameter != null && parameter.ToString().Equals("Inverse", StringComparison.OrdinalIgnoreCase))
            {
                visible = !visible;
            }

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a Visibility value back to a string value.
        /// </summary>
        /// <param name="value">The Visibility value.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">A parameter to customize the conversion.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>Returns an empty string if Visibility.Collapsed, otherwise null (since we can't know the original string).</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool isVisible = visibility == Visibility.Visible;
                
                // Check if we should invert the result
                if (parameter != null && parameter.ToString().Equals("Inverse", StringComparison.OrdinalIgnoreCase))
                {
                    isVisible = !isVisible;
                }
                
                return isVisible ? null : string.Empty;
            }
            
            return null;
        }
    }
} 
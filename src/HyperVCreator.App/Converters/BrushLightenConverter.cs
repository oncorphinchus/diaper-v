using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace HyperVCreator.App.Converters
{
    /// <summary>
    /// Lightens or darkens a brush by a specified percentage
    /// </summary>
    public class BrushLightenConverter : IValueConverter
    {
        /// <summary>
        /// Lightens or darkens a brush by the specified percentage.
        /// </summary>
        /// <param name="value">The brush to adjust.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The percentage to adjust brightness. Positive values lighten, negative values darken.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>The adjusted brush.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not SolidColorBrush sourceBrush)
                return value;

            // Try to parse the parameter as a double
            double percentage = 0;
            if (parameter is string paramStr)
            {
                if (!double.TryParse(paramStr, out percentage))
                    percentage = 0;
            }
            else if (parameter is double paramDouble)
            {
                percentage = paramDouble;
            }

            // Adjust the color
            Color sourceColor = sourceBrush.Color;
            
            // Calculate new color components
            byte r = AdjustColorComponent(sourceColor.R, percentage);
            byte g = AdjustColorComponent(sourceColor.G, percentage);
            byte b = AdjustColorComponent(sourceColor.B, percentage);
            
            // Create and return the new brush
            Color adjustedColor = Color.FromArgb(sourceColor.A, r, g, b);
            return new SolidColorBrush(adjustedColor);
        }

        /// <summary>
        /// Adjusts a color component (R, G, or B) by a percentage
        /// </summary>
        private byte AdjustColorComponent(byte component, double percentage)
        {
            double adjustedValue;
            if (percentage >= 0)
            {
                // Lighten: Move toward 255
                adjustedValue = component + ((255 - component) * (percentage / 100.0));
            }
            else
            {
                // Darken: Move toward 0
                adjustedValue = component * (1 + (percentage / 100.0));
            }
            
            return (byte)Math.Max(0, Math.Min(255, adjustedValue));
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
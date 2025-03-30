using System;
using System.Globalization;
using System.Windows.Data;

namespace HyperVCreator.App.Converters
{
    /// <summary>
    /// Converts a value to a boolean indicating if it equals the converter parameter.
    /// </summary>
    public class EqualityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value to a boolean indicating if it equals the converter parameter.
        /// </summary>
        /// <param name="value">The value to check equality.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The value to compare against.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>True if the value equals the parameter, otherwise false.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null && parameter == null)
            {
                return true;
            }
            
            if (value == null || parameter == null)
            {
                return false;
            }
            
            // Handle numeric comparisons
            if (value is IConvertible && parameter is IConvertible)
            {
                try
                {
                    // Try to convert both values to double for numeric comparison
                    double doubleValue = System.Convert.ToDouble(value);
                    double doubleParameter = System.Convert.ToDouble(parameter);
                    
                    return Math.Abs(doubleValue - doubleParameter) < 0.000001;
                }
                catch
                {
                    // If conversion fails, fall back to string comparison
                }
            }
            
            // Default string comparison
            return value.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Converts back from a boolean to the original value type.
        /// </summary>
        /// <param name="value">The boolean value.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The comparison parameter.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>The parameter if the boolean is true, otherwise the default value of the target type.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                // If true, return the parameter value (possibly converted to the target type)
                if (parameter != null && targetType != null)
                {
                    try
                    {
                        return System.Convert.ChangeType(parameter, targetType);
                    }
                    catch
                    {
                        // Conversion failed
                    }
                }
                
                return parameter;
            }
            
            // If false, return the default value of the target type
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }
    }
} 
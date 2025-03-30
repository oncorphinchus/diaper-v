using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace HyperVCreator.Core.PowerShell
{
    /// <summary>
    /// Extension methods for PowerShell-related classes
    /// </summary>
    public static class PowerShellExtensions
    {
        /// <summary>
        /// Gets the string representation of PSObject items in the Output list
        /// </summary>
        /// <param name="result">The PowerShellResult</param>
        /// <returns>A list of string representations of output items</returns>
        public static IEnumerable<string> GetOutputStrings(this PowerShellResult result)
        {
            if (result == null || result.Output == null)
                return Enumerable.Empty<string>();
                
            return result.Output.Select(o => o?.ToString() ?? string.Empty);
        }
        
        /// <summary>
        /// Checks if the PowerShellResult contains the specified output string
        /// </summary>
        /// <param name="result">The PowerShellResult</param>
        /// <param name="value">The string value to check for</param>
        /// <returns>True if the output contains the string, false otherwise</returns>
        public static bool ContainsOutput(this PowerShellResult result, string value)
        {
            if (result == null)
                return false;
                
            return result.GetOutputStrings().Any(s => s.Contains(value, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Safely checks if a PowerShellResult was successful
        /// </summary>
        /// <param name="result">The PowerShellResult</param>
        /// <returns>True if Success is true, false otherwise</returns>
        public static bool WasSuccessful(this PowerShellResult result)
        {
            return result?.Success == true;
        }
        
        /// <summary>
        /// Gets a strongly-typed value from a PSObject property
        /// </summary>
        /// <typeparam name="T">The target type</typeparam>
        /// <param name="psObject">The PSObject</param>
        /// <param name="propertyName">The property name</param>
        /// <param name="defaultValue">Default value if property doesn't exist or conversion fails</param>
        /// <returns>The property value or default</returns>
        public static T GetPropertyValue<T>(this PSObject psObject, string propertyName, T defaultValue = default)
        {
            if (psObject == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return defaultValue;
            }
            
            var property = psObject.Properties[propertyName];
            if (property == null || property.Value == null)
            {
                return defaultValue;
            }
            
            try
            {
                if (property.Value is T typedValue)
                {
                    return typedValue;
                }
                
                return (T)Convert.ChangeType(property.Value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
} 
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HyperVCreator.App.Services;

namespace HyperVCreator.App.Converters
{
    /// <summary>
    /// Base class for theme preview converters
    /// </summary>
    public abstract class ThemePreviewConverterBase : IValueConverter
    {
        // Theme color preview values
        protected static readonly Color ClassicBackgroundColor = Colors.White;
        protected static readonly Color ClassicPrimaryColor = Color.FromRgb(0, 120, 215); // #0078D7
        protected static readonly Color ClassicSecondaryColor = Color.FromRgb(90, 97, 112); // #5A6170
        
        protected static readonly Color DarkBackgroundColor = Color.FromRgb(18, 18, 18); // #121212
        protected static readonly Color DarkPrimaryColor = Color.FromRgb(30, 136, 229); // #1E88E5
        protected static readonly Color DarkSecondaryColor = Color.FromRgb(117, 117, 117); // #757575
        
        protected static readonly Color SanrioBackgroundColor = Colors.White;
        protected static readonly Color SanrioPrimaryColor = Color.FromRgb(255, 94, 148); // #FF5E94
        protected static readonly Color SanrioSecondaryColor = Color.FromRgb(116, 215, 236); // #74D7EC
        
        /// <summary>
        /// Not implemented for this converter.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Converts a theme type to a brush.
        /// </summary>
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);
    }
    
    /// <summary>
    /// Converts a theme type to a background color for theme preview
    /// </summary>
    public class ThemePreviewBackgroundConverter : ThemePreviewConverterBase
    {
        /// <summary>
        /// Converts a theme type to a background brush.
        /// </summary>
        /// <param name="value">The theme type.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>A brush for the theme background.</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ThemeType themeType)
            {
                Color backgroundColor = themeType switch
                {
                    ThemeType.Dark => DarkBackgroundColor,
                    ThemeType.Sanrio => SanrioBackgroundColor,
                    _ => ClassicBackgroundColor
                };
                
                return new SolidColorBrush(backgroundColor);
            }
            
            return new SolidColorBrush(Colors.White);
        }
    }
    
    /// <summary>
    /// Converts a theme type to a primary color for theme preview
    /// </summary>
    public class ThemePreviewPrimaryConverter : ThemePreviewConverterBase
    {
        /// <summary>
        /// Converts a theme type to a primary brush.
        /// </summary>
        /// <param name="value">The theme type.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>A brush for the theme primary color.</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ThemeType themeType)
            {
                Color primaryColor = themeType switch
                {
                    ThemeType.Dark => DarkPrimaryColor,
                    ThemeType.Sanrio => SanrioPrimaryColor,
                    _ => ClassicPrimaryColor
                };
                
                return new SolidColorBrush(primaryColor);
            }
            
            return new SolidColorBrush(ClassicPrimaryColor);
        }
    }
    
    /// <summary>
    /// Converts a theme type to a secondary color for theme preview
    /// </summary>
    public class ThemePreviewSecondaryConverter : ThemePreviewConverterBase
    {
        /// <summary>
        /// Converts a theme type to a secondary brush.
        /// </summary>
        /// <param name="value">The theme type.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>A brush for the theme secondary color.</returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ThemeType themeType)
            {
                Color secondaryColor = themeType switch
                {
                    ThemeType.Dark => DarkSecondaryColor,
                    ThemeType.Sanrio => SanrioSecondaryColor,
                    _ => ClassicSecondaryColor
                };
                
                return new SolidColorBrush(secondaryColor);
            }
            
            return new SolidColorBrush(ClassicSecondaryColor);
        }
    }
} 
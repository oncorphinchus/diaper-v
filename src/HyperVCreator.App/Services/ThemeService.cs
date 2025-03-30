using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Linq;

namespace HyperVCreator.App.Services
{
    /// <summary>
    /// Theme type enum for the application
    /// </summary>
    public enum ThemeType
    {
        Classic,
        Dark,
        Sanrio
    }

    /// <summary>
    /// Service for managing application themes
    /// </summary>
    public class ThemeService
    {
        private const string ThemeSettingsFile = "ThemeSettings.txt";
        private const string DefaultTheme = "ClassicTheme";
        
        private readonly Dictionary<ThemeType, string> _themeResourcePaths = new Dictionary<ThemeType, string>
        {
            { ThemeType.Classic, "Themes/ClassicTheme/ClassicTheme.xaml" },
            { ThemeType.Dark, "Themes/DarkTheme/DarkTheme.xaml" },
            { ThemeType.Sanrio, "Themes/SanrioTheme/SanrioTheme.xaml" }
        };
        
        /// <summary>
        /// Gets or sets the current theme
        /// </summary>
        public ThemeType CurrentTheme { get; private set; }
        
        /// <summary>
        /// Event that is raised when the theme changes
        /// </summary>
        public event EventHandler<ThemeType> ThemeChanged;
        
        /// <summary>
        /// Initializes a new instance of the ThemeService class
        /// </summary>
        public ThemeService()
        {
            // Load saved theme or use default
            CurrentTheme = LoadSavedTheme();
        }
        
        /// <summary>
        /// Changes the application theme
        /// </summary>
        /// <param name="theme">The theme to apply</param>
        public void ChangeTheme(ThemeType theme)
        {
            if (!_themeResourcePaths.TryGetValue(theme, out string themePath))
            {
                throw new ArgumentException($"Theme {theme} not found", nameof(theme));
            }
            
            // Update resource dictionaries
            var appResources = Application.Current.Resources.MergedDictionaries;
            
            // Find the index of the current theme dictionary
            int themeIndex = FindThemeResourceIndex(appResources);
            
            if (themeIndex >= 0)
            {
                // Replace the theme dictionary
                var newThemeDict = new ResourceDictionary
                {
                    Source = new Uri(themePath, UriKind.Relative)
                };
                
                appResources[themeIndex] = newThemeDict;
                
                // Update current theme and save setting
                CurrentTheme = theme;
                SaveThemeSetting(theme);
                
                // Raise theme changed event
                ThemeChanged?.Invoke(this, theme);
            }
        }
        
        /// <summary>
        /// Gets the available theme options
        /// </summary>
        /// <returns>List of available themes</returns>
        public IEnumerable<ThemeType> GetAvailableThemes()
        {
            return Enum.GetValues(typeof(ThemeType)).Cast<ThemeType>();
        }
        
        /// <summary>
        /// Gets the display name for a theme
        /// </summary>
        /// <param name="theme">The theme</param>
        /// <returns>User-friendly theme name</returns>
        public string GetThemeDisplayName(ThemeType theme)
        {
            return theme switch
            {
                ThemeType.Classic => "Classic",
                ThemeType.Dark => "Dark Mode",
                ThemeType.Sanrio => "Sanrio",
                _ => theme.ToString()
            };
        }
        
        /// <summary>
        /// Finds the index of the theme resource dictionary
        /// </summary>
        private int FindThemeResourceIndex(IList<ResourceDictionary> resources)
        {
            for (int i = 0; i < resources.Count; i++)
            {
                var dictionary = resources[i];
                if (dictionary.Source != null && 
                    dictionary.Source.OriginalString.Contains("/Theme") && 
                    dictionary.Source.OriginalString.EndsWith("Theme.xaml"))
                {
                    return i;
                }
            }
            
            return -1;
        }
        
        /// <summary>
        /// Loads the saved theme setting
        /// </summary>
        private ThemeType LoadSavedTheme()
        {
            string settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HyperVCreator",
                ThemeSettingsFile);
            
            if (File.Exists(settingsPath))
            {
                try
                {
                    string themeName = File.ReadAllText(settingsPath).Trim();
                    if (Enum.TryParse<ThemeType>(themeName, out ThemeType savedTheme))
                    {
                        return savedTheme;
                    }
                }
                catch
                {
                    // Ignore reading errors and use default
                }
            }
            
            return ThemeType.Classic; // Default theme
        }
        
        /// <summary>
        /// Saves the current theme setting
        /// </summary>
        private void SaveThemeSetting(ThemeType theme)
        {
            string settingsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HyperVCreator");
            
            string settingsPath = Path.Combine(settingsDirectory, ThemeSettingsFile);
            
            try
            {
                if (!Directory.Exists(settingsDirectory))
                {
                    Directory.CreateDirectory(settingsDirectory);
                }
                
                File.WriteAllText(settingsPath, theme.ToString());
            }
            catch
            {
                // Ignore writing errors
            }
        }
    }
} 
using System;
using System.Collections.Generic;
using System.IO;

namespace HyperVCreator.Core.Services
{
    public class ThemeService : IThemeService
    {
        private readonly string _settingsPath;
        private string _currentTheme;
        private readonly List<string> _availableThemes;

        public event EventHandler ThemeChanged;

        public string CurrentTheme => _currentTheme;
        public List<string> AvailableThemes => _availableThemes;

        public ThemeService()
        {
            // Set up settings path
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string settingsDir = Path.Combine(appData, "HyperVCreator", "Settings");
            _settingsPath = Path.Combine(settingsDir, "theme.txt");

            // Ensure directory exists
            if (!Directory.Exists(settingsDir))
            {
                Directory.CreateDirectory(settingsDir);
            }

            // Available themes
            _availableThemes = new List<string>
            {
                "Classic",
                "Dark",
                "Sanrio"
            };

            // Load theme preference or use default
            _currentTheme = LoadThemePreference() ?? "Classic";
        }

        public void SetTheme(string themeName)
        {
            if (string.IsNullOrWhiteSpace(themeName) || !_availableThemes.Contains(themeName))
            {
                throw new ArgumentException($"Invalid theme name: {themeName}");
            }

            if (_currentTheme != themeName)
            {
                _currentTheme = themeName;
                OnThemeChanged();
            }
        }

        public void SaveThemePreference(string themeName)
        {
            try
            {
                File.WriteAllText(_settingsPath, themeName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving theme preference: {ex.Message}");
            }
        }

        public string LoadThemePreference()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string theme = File.ReadAllText(_settingsPath).Trim();
                    if (_availableThemes.Contains(theme))
                    {
                        return theme;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading theme preference: {ex.Message}");
            }

            return "Classic"; // Default theme
        }

        protected virtual void OnThemeChanged()
        {
            SaveThemePreference(_currentTheme);
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
} 
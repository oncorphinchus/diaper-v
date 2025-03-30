using System;
using System.Collections.Generic;
using System.Windows;

namespace HyperVCreator.Core.Services
{
    public interface IThemeService
    {
        event EventHandler ThemeChanged;
        
        string CurrentTheme { get; }
        List<string> AvailableThemes { get; }
        
        void SetTheme(string themeName);
        void SaveThemePreference(string themeName);
        string LoadThemePreference();
    }
} 
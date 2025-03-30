using System;
using System.IO;
using System.Windows;
using HyperVCreator.App.Services;
using HyperVCreator.Core.Services;
using CoreServices = HyperVCreator.Core.Services;
using CorePowerShell = HyperVCreator.Core.PowerShell;
using AppServices = HyperVCreator.App.Services;

namespace HyperVCreator.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IThemeService _themeService;
        private Core.PowerShell.PowerShellService _powerShellService;
        private IHyperVService _hyperVService;
        private IConfigurationService _configurationService;
        private TemplateService _templateService;
        
        /// <summary>
        /// Application startup handler
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Initialize services
            InitializeServices();
            
            // Apply theme
            ApplyInitialTheme();
            
            // Register services in the application resources
            RegisterServiceResources();
        }
        
        /// <summary>
        /// Initialize application services
        /// </summary>
        private void InitializeServices()
        {
            // Initialize the core services
            _powerShellService = new CorePowerShell.PowerShellService();
            _themeService = new AppServices.ThemeService();
            _hyperVService = new CoreServices.HyperVService(_powerShellService);
            _configurationService = new CoreServices.ConfigurationService(_powerShellService);
            _templateService = new CoreServices.TemplateService(_powerShellService);
        }
        
        /// <summary>
        /// Register services in application resources for easy access
        /// </summary>
        private void RegisterServiceResources()
        {
            // Register services in application resources
            Resources["ThemeService"] = _themeService;
            Resources["PowerShellService"] = _powerShellService;
            Resources["HyperVService"] = _hyperVService;
            Resources["ConfigurationService"] = _configurationService;
            Resources["TemplateService"] = _templateService;
        }
        
        /// <summary>
        /// Apply the initial theme based on saved settings
        /// </summary>
        private void ApplyInitialTheme()
        {
            try
            {
                string themeName = _themeService.CurrentTheme;
                
                // Clear existing theme dictionaries
                foreach (var dict in Resources.MergedDictionaries)
                {
                    if (dict.Source != null && dict.Source.ToString().Contains("Theme") && 
                        !dict.Source.ToString().Contains("BaseTheme"))
                    {
                        Resources.MergedDictionaries.Remove(dict);
                        break;
                    }
                }
                
                // Add the new theme dictionary
                var themeUri = new Uri($"Themes/{themeName}Theme/{themeName}Theme.xaml", UriKind.Relative);
                Resources.MergedDictionaries.Add(new ResourceDictionary { Source = themeUri });
            }
            catch (Exception ex)
            {
                // Log the error but continue with default theme
                Console.WriteLine($"Error applying theme: {ex.Message}");
            }
        }
    }
} 
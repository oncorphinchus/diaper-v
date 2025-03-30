using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using HyperVCreator.Core.Services;

namespace HyperVCreator.App.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly PowerShellService _powerShellService;
        private readonly Dictionary<string, object> _settings;
        private readonly string _settingsPath;
        
        public ConfigurationService(PowerShellService powerShellService)
        {
            _powerShellService = powerShellService ?? throw new ArgumentNullException(nameof(powerShellService));
            _settings = new Dictionary<string, object>();
            
            // Set the settings path
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string settingsDir = Path.Combine(appData, "HyperVCreator", "Settings");
            _settingsPath = Path.Combine(settingsDir, "settings.json");
            
            // Ensure directory exists
            if (!Directory.Exists(settingsDir))
            {
                Directory.CreateDirectory(settingsDir);
            }
            
            // Load settings
            LoadSettings();
        }
        
        public async Task<bool> ConfigureDomainController(string vmName, string domainName, string netbiosName, string adminPassword)
        {
            // Dummy implementation
            return true;
        }
        
        public async Task<bool> ConfigureRDSH(string vmName, string domain, string adminUser, string adminPassword)
        {
            // Dummy implementation
            return true;
        }
        
        public async Task<bool> ConfigureFileServer(string vmName, string shareName, string sharePath, string permissions)
        {
            // Dummy implementation
            return true;
        }
        
        public async Task<bool> ConfigureWebServer(string vmName, string siteName, int port, string physicalPath)
        {
            // Dummy implementation
            return true;
        }
        
        public async Task<bool> ConfigureSQLServer(string vmName, string instanceName, string adminPassword, bool mixedMode)
        {
            // Dummy implementation
            return true;
        }
        
        public async Task<bool> ConfigureDHCPServer(string vmName, string scopeName, string startRange, string endRange, string subnetMask)
        {
            // Dummy implementation
            return true;
        }
        
        public async Task<bool> ConfigureDNSServer(string vmName, string zoneName, string zoneType)
        {
            // Dummy implementation
            return true;
        }
        
        public T GetSetting<T>(string key, T defaultValue = default)
        {
            if (_settings.TryGetValue(key, out object value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }
                
                try
                {
                    // Try to convert the value
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    // Ignore conversion errors
                }
            }
            
            return defaultValue;
        }
        
        public void SaveSetting<T>(string key, T value)
        {
            _settings[key] = value;
            SaveSettings();
        }
        
        public async Task<T> GetSettingAsync<T>(string key, T defaultValue = default)
        {
            return GetSetting(key, defaultValue);
        }
        
        public async Task SaveSettingAsync<T>(string key, T value)
        {
            SaveSetting(key, value);
            await Task.CompletedTask;
        }
        
        public void ResetSettings()
        {
            _settings.Clear();
            SaveSettings();
        }
        
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    var loadedSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    
                    if (loadedSettings != null)
                    {
                        _settings.Clear();
                        foreach (var kvp in loadedSettings)
                        {
                            _settings[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
        }
        
        private void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(_settings);
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
    }
} 
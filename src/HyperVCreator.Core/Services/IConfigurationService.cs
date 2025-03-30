using System.Threading.Tasks;

namespace HyperVCreator.Core.Services
{
    public interface IConfigurationService
    {
        Task<bool> ConfigureDomainController(string vmName, string domainName, string netbiosName, string adminPassword);
        Task<bool> ConfigureRDSH(string vmName, string domain, string adminUser, string adminPassword);
        Task<bool> ConfigureFileServer(string vmName, string shareName, string sharePath, string permissions);
        Task<bool> ConfigureWebServer(string vmName, string siteName, int port, string physicalPath);
        Task<bool> ConfigureSQLServer(string vmName, string instanceName, string adminPassword, bool mixedMode);
        Task<bool> ConfigureDHCPServer(string vmName, string scopeName, string startRange, string endRange, string subnetMask);
        Task<bool> ConfigureDNSServer(string vmName, string zoneName, string zoneType);
        T GetSetting<T>(string key, T defaultValue = default);
        void SaveSetting<T>(string key, T value);
        Task<T> GetSettingAsync<T>(string key, T defaultValue = default);
        Task SaveSettingAsync<T>(string key, T value);
        void ResetSettings();
    }
} 
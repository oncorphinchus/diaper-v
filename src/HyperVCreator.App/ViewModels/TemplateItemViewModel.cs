using System;
using System.IO;
using HyperVCreator.Core.Services;
using HyperVCreator.Core.Models;

namespace HyperVCreator.App.ViewModels
{
    public class TemplateItemViewModel : ViewModelBase
    {
        private readonly VMTemplate _template;
        private readonly string _fileName;

        public TemplateItemViewModel(VMTemplate template)
        {
            _template = template ?? throw new ArgumentNullException(nameof(template));
            // Extract filename from template name and role
            _fileName = $"{template.ServerRole}-{template.TemplateName.Replace(" ", "-")}.json";
        }

        public string Name => _template.TemplateName;
        public string Role => _template.ServerRole;
        public string Description => _template.Description;
        public string Author => _template.Metadata.Author;
        public DateTime LastModified => _template.Metadata.LastModifiedDate;
        public string FileName => _fileName;
        public VMTemplate Template => _template;

        // Additional properties for UI display
        public string LastModifiedDisplay => LastModified.ToString("yyyy-MM-dd HH:mm");
        public string VCPUs => _template.HardwareConfiguration.ProcessorCount.ToString();
        public string MemoryGB => _template.HardwareConfiguration.MemoryGB.ToString();
        public string StorageGB => _template.HardwareConfiguration.StorageGB.ToString();
        public string OSVersion => _template.OSConfiguration.OSVersion;
        
        // Status indicators for UI
        public bool IsBuiltIn => _template.Metadata.Author.Equals("System", StringComparison.OrdinalIgnoreCase);
        
        // Icon based on role
        public string RoleIcon => Role switch
        {
            "DomainController" => "Icon_DomainController",
            "FileServer" => "Icon_FileServer",
            "WebServer" => "Icon_WebServer",
            "SQLServer" => "Icon_SQLServer",
            "RDSH" => "Icon_RDSH",
            "DHCPServer" => "Icon_DHCP",
            "DNSServer" => "Icon_DNS",
            "CustomVM" => "Icon_CustomVM",
            _ => "Icon_Default"
        };
    }
} 
using System;
using System.Collections.Generic;

namespace HyperVCreator.Core.Models
{
    public class HardwareConfig
    {
        public int ProcessorCount { get; set; } = 2;
        public int MemoryGB { get; set; } = 4;
        public int StorageGB { get; set; } = 80;
        public int Generation { get; set; } = 2;
        public bool EnableSecureBoot { get; set; } = true;
        public List<DiskConfig> AdditionalDisks { get; set; } = new List<DiskConfig>();
    }
    
    public class DiskConfig
    {
        public int SizeGB { get; set; }
        public string Letter { get; set; }
        public string Label { get; set; }
    }
    
    public class NetworkConfig
    {
        public string VirtualSwitch { get; set; } = "Default Switch";
        public bool DynamicIP { get; set; } = true;
        public StaticIPConfig StaticIPConfiguration { get; set; } = new StaticIPConfig();
    }
    
    public class StaticIPConfig
    {
        public string IPAddress { get; set; }
        public string SubnetMask { get; set; } = "255.255.255.0";
        public string DefaultGateway { get; set; }
        public List<string> DNSServers { get; set; } = new List<string>();
    }
    
    public class OSConfig
    {
        public string OSVersion { get; set; } = "Windows Server 2022 Standard";
        public string ProductKey { get; set; }
        public int TimeZone { get; set; } = 85;
        public string AdminPassword { get; set; } = "P@ssw0rd";
        public string ComputerName { get; set; }
        public string Organization { get; set; } = "Custom Organization";
        public string Owner { get; set; } = "Administrator";
    }
    
    public class AdditionalConfig
    {
        public bool AutoStartVM { get; set; }
        public bool UseUnattendXML { get; set; } = true;
        public bool EnableRDP { get; set; } = true;
        public bool EnablePSRemoting { get; set; } = true;
    }
    
    public class TemplateMetadata
    {
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;
        public string Author { get; set; } = Environment.UserName;
        public List<string> Tags { get; set; } = new List<string>();
    }
} 
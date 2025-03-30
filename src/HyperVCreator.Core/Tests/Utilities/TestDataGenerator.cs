using System;
using System.Collections.Generic;
using HyperVCreator.Core.Models;
using HyperVCreator.Core.Services;

namespace HyperVCreator.Core.Tests.Utilities
{
    /// <summary>
    /// Utility class for generating test data
    /// </summary>
    public static class TestDataGenerator
    {
        /// <summary>
        /// Generate a sample template with random values
        /// </summary>
        /// <param name="serverRole">Optional server role</param>
        /// <returns>A VM template with randomized properties</returns>
        public static VMTemplate GenerateRandomTemplate(string serverRole = null)
        {
            var random = new Random();
            var templateId = Guid.NewGuid().ToString().Substring(0, 8);
            
            return new VMTemplate
            {
                TemplateName = $"Test Template {templateId}",
                ServerRole = serverRole ?? GetRandomServerRole(),
                Description = $"Automatically generated test template {templateId}",
                HardwareConfiguration = GenerateRandomHardwareConfig(random),
                NetworkConfiguration = GenerateRandomNetworkConfig(random),
                OSConfiguration = GenerateRandomOSConfig(random),
                AdditionalConfiguration = GenerateRandomAdditionalConfig(random),
                Metadata = GenerateRandomMetadata()
            };
        }
        
        /// <summary>
        /// Generate a list of templates for testing
        /// </summary>
        /// <param name="count">Number of templates to generate</param>
        /// <returns>A list of templates</returns>
        public static List<VMTemplate> GenerateRandomTemplates(int count)
        {
            var templates = new List<VMTemplate>();
            for (int i = 0; i < count; i++)
            {
                templates.Add(GenerateRandomTemplate());
            }
            return templates;
        }
        
        /// <summary>
        /// Generate random hardware configuration
        /// </summary>
        private static Models.HardwareConfig GenerateRandomHardwareConfig(Random random)
        {
            return new Models.HardwareConfig
            {
                ProcessorCount = random.Next(1, 8),
                MemoryGB = random.Next(2, 32),
                StorageGB = random.Next(20, 500),
                Generation = random.Next(1, 3),
                EnableSecureBoot = random.Next(2) == 1,
                AdditionalDisks = GenerateRandomDisks(random)
            };
        }
        
        /// <summary>
        /// Generate random network configuration
        /// </summary>
        private static Models.NetworkConfig GenerateRandomNetworkConfig(Random random)
        {
            bool dynamicIp = random.Next(2) == 1;
            
            return new Models.NetworkConfig
            {
                VirtualSwitch = GetRandomVirtualSwitch(),
                DynamicIP = dynamicIp,
                StaticIPConfiguration = dynamicIp ? new Models.StaticIPConfig() : GenerateRandomStaticIpConfig(random)
            };
        }
        
        /// <summary>
        /// Generate random OS configuration
        /// </summary>
        private static Models.OSConfig GenerateRandomOSConfig(Random random)
        {
            return new Models.OSConfig
            {
                OSVersion = GetRandomOSVersion(),
                ProductKey = random.Next(2) == 1 ? GenerateRandomProductKey() : null,
                TimeZone = random.Next(0, 100),
                AdminPassword = "P@ssw0rd", // Fixed for testing
                ComputerName = $"TESTVM-{random.Next(1000, 9999)}"
            };
        }
        
        /// <summary>
        /// Generate random additional configuration
        /// </summary>
        private static Models.AdditionalConfig GenerateRandomAdditionalConfig(Random random)
        {
            return new Models.AdditionalConfig
            {
                AutoStartVM = random.Next(2) == 1,
                UseUnattendXML = random.Next(2) == 1,
                EnableRDP = random.Next(2) == 1,
                EnablePSRemoting = random.Next(2) == 1
            };
        }
        
        /// <summary>
        /// Generate random metadata
        /// </summary>
        private static Models.TemplateMetadata GenerateRandomMetadata()
        {
            return new Models.TemplateMetadata
            {
                CreatedDate = DateTime.Now.AddDays(-new Random().Next(0, 30)),
                LastModifiedDate = DateTime.Now,
                Author = "Test Automation",
                Tags = GetRandomTags()
            };
        }
        
        /// <summary>
        /// Generate random disks
        /// </summary>
        private static List<Models.DiskConfig> GenerateRandomDisks(Random random)
        {
            var disks = new List<Models.DiskConfig>();
            int diskCount = random.Next(0, 3);
            
            for (int i = 0; i < diskCount; i++)
            {
                disks.Add(new Models.DiskConfig
                {
                    SizeGB = random.Next(10, 200),
                    Letter = ((char)('D' + i)).ToString(),
                    Label = $"DATA{i + 1}"
                });
            }
            
            return disks;
        }
        
        /// <summary>
        /// Generate random static IP config
        /// </summary>
        private static Models.StaticIPConfig GenerateRandomStaticIpConfig(Random random)
        {
            return new Models.StaticIPConfig
            {
                IPAddress = $"192.168.1.{random.Next(2, 254)}",
                SubnetMask = "255.255.255.0",
                DefaultGateway = "192.168.1.1",
                DNSServers = new List<string> { "192.168.1.1", "8.8.8.8" }
            };
        }
        
        /// <summary>
        /// Generate a random product key
        /// </summary>
        private static string GenerateRandomProductKey()
        {
            var random = new Random();
            return $"{RandomString(5)}-{RandomString(5)}-{RandomString(5)}-{RandomString(5)}-{RandomString(5)}";
        }
        
        /// <summary>
        /// Generate a random string of characters
        /// </summary>
        private static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var stringChars = new char[length];
            
            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            
            return new string(stringChars);
        }
        
        /// <summary>
        /// Get a random server role
        /// </summary>
        private static string GetRandomServerRole()
        {
            var roles = new[] 
            { 
                "DomainController", 
                "RDSH", 
                "FileServer", 
                "WebServer", 
                "SQLServer", 
                "DHCPServer", 
                "DNSServer", 
                "CustomVM" 
            };
            
            return roles[new Random().Next(roles.Length)];
        }
        
        /// <summary>
        /// Get random virtual switch name
        /// </summary>
        private static string GetRandomVirtualSwitch()
        {
            var switches = new[] 
            { 
                "Default Switch", 
                "Internal Switch", 
                "Private Switch", 
                "External Switch" 
            };
            
            return switches[new Random().Next(switches.Length)];
        }
        
        /// <summary>
        /// Get random OS version
        /// </summary>
        private static string GetRandomOSVersion()
        {
            var versions = new[]
            {
                "Windows Server 2016 Standard",
                "Windows Server 2016 Datacenter",
                "Windows Server 2019 Standard",
                "Windows Server 2019 Datacenter",
                "Windows Server 2022 Standard",
                "Windows Server 2022 Datacenter"
            };
            
            return versions[new Random().Next(versions.Length)];
        }
        
        /// <summary>
        /// Get random tags
        /// </summary>
        private static List<string> GetRandomTags()
        {
            var allTags = new[]
            {
                "Test", "Automated", "Dev", "QA", "Production",
                "Temporary", "Permanent", "High-Memory", "Low-CPU",
                "Isolated", "Core", "GUI", "DC", "SQL", "Web"
            };
            
            var random = new Random();
            var tags = new List<string>();
            int tagCount = random.Next(0, 4);
            
            for (int i = 0; i < tagCount; i++)
            {
                var tag = allTags[random.Next(allTags.Length)];
                if (!tags.Contains(tag))
                {
                    tags.Add(tag);
                }
            }
            
            return tags;
        }
    }
} 
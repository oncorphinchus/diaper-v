using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace HyperVCreator.Core.Services
{
    /// <summary>
    /// Converts VM templates to PowerShell parameters and vice versa
    /// </summary>
    public class TemplateConverter
    {
        /// <summary>
        /// Converts a VM template to PowerShell parameters
        /// </summary>
        /// <param name="template">The VM template to convert</param>
        /// <returns>A dictionary of PowerShell parameters</returns>
        public Dictionary<string, object> ConvertToPowerShellParameters(VMTemplate template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }
            
            var parameters = new Dictionary<string, object>();
            
            // Basic VM parameters
            parameters["VMName"] = template.TemplateName;
            parameters["OSVersion"] = template.OSConfiguration.OSVersion;
            parameters["VMDescription"] = template.Description ?? $"{template.ServerRole} Virtual Machine";
            
            // Hardware configuration
            parameters["CPUCount"] = template.HardwareConfiguration.ProcessorCount;
            parameters["MemoryGB"] = template.HardwareConfiguration.MemoryGB;
            parameters["StorageGB"] = template.HardwareConfiguration.StorageGB;
            parameters["Generation"] = template.HardwareConfiguration.Generation;
            parameters["EnableSecureBoot"] = template.HardwareConfiguration.EnableSecureBoot;
            
            // Network configuration
            parameters["VirtualSwitch"] = template.NetworkConfiguration.VirtualSwitch;
            
            if (!template.NetworkConfiguration.DynamicIP)
            {
                parameters["IPAddress"] = template.NetworkConfiguration.StaticIPConfiguration.IPAddress;
                parameters["SubnetMask"] = template.NetworkConfiguration.StaticIPConfiguration.SubnetMask;
                parameters["DefaultGateway"] = template.NetworkConfiguration.StaticIPConfiguration.DefaultGateway;
                
                if (template.NetworkConfiguration.StaticIPConfiguration.DNSServers.Count > 0)
                {
                    parameters["DNSServer"] = template.NetworkConfiguration.StaticIPConfiguration.DNSServers[0];
                }
            }
            
            // OS configuration
            parameters["ProductKey"] = template.OSConfiguration.ProductKey;
            parameters["ComputerName"] = template.OSConfiguration.ComputerName;
            parameters["AdminPassword"] = template.OSConfiguration.AdminPassword;
            parameters["TimeZone"] = template.OSConfiguration.TimeZone;
            
            // Additional configuration
            parameters["AutoStart"] = template.AdditionalConfiguration.AutoStartVM;
            parameters["UseUnattendXML"] = template.AdditionalConfiguration.UseUnattendXML;
            
            // Handle additional disks if present
            if (template.HardwareConfiguration.AdditionalDisks.Count > 0)
            {
                var additionalDisk = template.HardwareConfiguration.AdditionalDisks[0];
                if (additionalDisk.SizeGB > 0)
                {
                    parameters["AdditionalDiskSizeGB"] = additionalDisk.SizeGB;
                }
            }
            
            // Role-specific parameters
            switch (template.ServerRole)
            {
                case "DomainController":
                    // Add domain controller specific parameters
                    break;
                    
                case "FileServer":
                    // Add file server specific parameters
                    break;
                    
                case "WebServer":
                    // Add web server specific parameters
                    break;
                    
                case "SQLServer":
                    // Add SQL Server specific parameters
                    break;
                    
                case "DHCPServer":
                    // Add DHCP server specific parameters
                    break;
                    
                case "DNSServer":
                    // Add DNS server specific parameters
                    break;
                    
                case "RDSH":
                    // Add RDSH specific parameters
                    break;
            }
            
            return parameters;
        }
        
        /// <summary>
        /// Converts PowerShell parameters to a VM template
        /// </summary>
        /// <param name="parameters">The PowerShell parameters</param>
        /// <param name="serverRole">The server role for the template</param>
        /// <returns>A VM template</returns>
        public VMTemplate ConvertFromPowerShellParameters(Dictionary<string, object> parameters, string serverRole)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }
            
            if (string.IsNullOrWhiteSpace(serverRole))
            {
                throw new ArgumentException("Server role is required", nameof(serverRole));
            }
            
            var template = new VMTemplate
            {
                ServerRole = serverRole,
                TemplateName = GetParameterValue<string>(parameters, "VMName", $"New {serverRole}"),
                Description = GetParameterValue<string>(parameters, "VMDescription", $"{serverRole} Virtual Machine"),
                
                HardwareConfiguration = new HardwareConfig
                {
                    ProcessorCount = GetParameterValue<int>(parameters, "CPUCount", 2),
                    MemoryGB = GetParameterValue<int>(parameters, "MemoryGB", 4),
                    StorageGB = GetParameterValue<int>(parameters, "StorageGB", 80),
                    Generation = GetParameterValue<int>(parameters, "Generation", 2),
                    EnableSecureBoot = GetParameterValue<bool>(parameters, "EnableSecureBoot", true),
                    AdditionalDisks = new List<DiskConfig>()
                },
                
                NetworkConfiguration = new NetworkConfig
                {
                    VirtualSwitch = GetParameterValue<string>(parameters, "VirtualSwitch", "Default Switch"),
                    DynamicIP = !parameters.ContainsKey("IPAddress"),
                    StaticIPConfiguration = new StaticIPConfig
                    {
                        IPAddress = GetParameterValue<string>(parameters, "IPAddress", ""),
                        SubnetMask = GetParameterValue<string>(parameters, "SubnetMask", "255.255.255.0"),
                        DefaultGateway = GetParameterValue<string>(parameters, "DefaultGateway", ""),
                        DNSServers = new List<string>()
                    }
                },
                
                OSConfiguration = new OSConfig
                {
                    OSVersion = GetParameterValue<string>(parameters, "OSVersion", "Windows Server 2022 Standard"),
                    ProductKey = GetParameterValue<string>(parameters, "ProductKey", ""),
                    ComputerName = GetParameterValue<string>(parameters, "ComputerName", ""),
                    AdminPassword = GetParameterValue<string>(parameters, "AdminPassword", "P@ssw0rd"),
                    TimeZone = GetParameterValue<int>(parameters, "TimeZone", 85),
                    Organization = GetParameterValue<string>(parameters, "Organization", "Custom Organization"),
                    Owner = GetParameterValue<string>(parameters, "Owner", "Administrator")
                },
                
                AdditionalConfiguration = new AdditionalConfig
                {
                    AutoStartVM = GetParameterValue<bool>(parameters, "AutoStart", false),
                    UseUnattendXML = GetParameterValue<bool>(parameters, "UseUnattendXML", true),
                    EnableRDP = GetParameterValue<bool>(parameters, "EnableRDP", true),
                    EnablePSRemoting = GetParameterValue<bool>(parameters, "EnablePSRemoting", true)
                },
                
                Metadata = new TemplateMetadata
                {
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    Author = Environment.UserName,
                    Tags = new List<string> { serverRole }
                }
            };
            
            // Add DNS server if provided
            if (parameters.ContainsKey("DNSServer"))
            {
                var dnsServer = GetParameterValue<string>(parameters, "DNSServer", "");
                if (!string.IsNullOrWhiteSpace(dnsServer))
                {
                    template.NetworkConfiguration.StaticIPConfiguration.DNSServers.Add(dnsServer);
                }
            }
            
            // Add additional disk if size is provided
            if (parameters.ContainsKey("AdditionalDiskSizeGB"))
            {
                var diskSize = GetParameterValue<int>(parameters, "AdditionalDiskSizeGB", 0);
                if (diskSize > 0)
                {
                    template.HardwareConfiguration.AdditionalDisks.Add(new DiskConfig
                    {
                        SizeGB = diskSize,
                        Letter = "D",
                        Label = "Data"
                    });
                }
            }
            
            return template;
        }
        
        /// <summary>
        /// Converts a VM template to PowerShell parameters for use with PowerShell cmdlets
        /// </summary>
        /// <param name="template">The VM template to convert</param>
        /// <returns>A dictionary of PowerShell parameters</returns>
        public CommandParameterCollection ConvertToPowerShellCommandParameters(VMTemplate template)
        {
            var parameters = ConvertToPowerShellParameters(template);
            var commandParameters = new CommandParameterCollection();
            
            foreach (var param in parameters)
            {
                if (param.Value != null)
                {
                    commandParameters.Add(new CommandParameter(param.Key, param.Value));
                }
            }
            
            return commandParameters;
        }
        
        /// <summary>
        /// Gets a parameter value of the specified type, or returns the default value if not found
        /// </summary>
        /// <typeparam name="T">The parameter type</typeparam>
        /// <param name="parameters">The parameters dictionary</param>
        /// <param name="key">The parameter key</param>
        /// <param name="defaultValue">The default value to return if not found</param>
        /// <returns>The parameter value or default</returns>
        private T GetParameterValue<T>(Dictionary<string, object> parameters, string key, T defaultValue)
        {
            if (parameters.TryGetValue(key, out var value))
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
                    // Return default if conversion fails
                    return defaultValue;
                }
            }
            
            return defaultValue;
        }
    }
} 
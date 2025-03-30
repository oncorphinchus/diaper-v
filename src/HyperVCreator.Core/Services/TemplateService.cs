using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HyperVCreator.Core.Models;
using HyperVCreator.Core.PowerShell;

namespace HyperVCreator.Core.Services
{
    /// <summary>
    /// Service for managing VM templates
    /// </summary>
    public class TemplateService
    {
        private readonly string _templateDirectory;
        private readonly Dictionary<string, VMTemplate> _templates;
        private readonly JsonSerializerOptions _jsonOptions;
        
        public TemplateService(string customTemplateDirectory = null)
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            // Set template directory
            if (string.IsNullOrWhiteSpace(customTemplateDirectory))
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                _templateDirectory = Path.Combine(appDataPath, "HyperVCreator", "Templates");
            }
            else
            {
                _templateDirectory = customTemplateDirectory;
            }

            // Ensure template directory exists
            if (!Directory.Exists(_templateDirectory))
            {
                Directory.CreateDirectory(_templateDirectory);
            }

            // Load templates
            _templates = new Dictionary<string, VMTemplate>();
            LoadTemplates();
        }
        
        /// <summary>
        /// Gets all available templates
        /// </summary>
        /// <returns>A list of template objects</returns>
        public List<VMTemplate> GetAllTemplates()
        {
            return _templates.Values.ToList();
        }
        
        /// <summary>
        /// Gets a template by name
        /// </summary>
        /// <param name="name">The name of the template</param>
        /// <returns>The template with the specified name, or null if not found</returns>
        public VMTemplate GetTemplateByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            
            return _templates.TryGetValue(name, out var template) ? template : null;
        }
        
        /// <summary>
        /// Gets templates for a specific server role
        /// </summary>
        /// <param name="role">The server role to filter by</param>
        /// <returns>A list of templates for the specified role</returns>
        public async Task<List<VMTemplate>> GetTemplatesByRoleAsync(string role)
        {
            var allTemplates = await GetAllTemplatesAsync();
            return allTemplates.Where(t => t.ServerRole.Equals(role, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        
        /// <summary>
        /// Gets all available templates
        /// </summary>
        /// <returns>A list of template objects</returns>
        public async Task<List<VMTemplate>> GetAllTemplatesAsync()
        {
            var templates = new List<VMTemplate>();
            
            foreach (var file in Directory.GetFiles(_templateDirectory, "*.json"))
            {
                try
                {
                    var template = await LoadTemplateAsync(file);
                    if (template != null)
                    {
                        templates.Add(template);
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but continue with other templates
                    Console.WriteLine($"Error loading template {file}: {ex.Message}");
                }
            }
            
            return templates;
        }
        
        /// <summary>
        /// Loads a template from a file
        /// </summary>
        /// <param name="filePath">The path to the template file</param>
        /// <returns>The loaded template, or null if loading failed</returns>
        public async Task<VMTemplate> LoadTemplateAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Template file not found: {filePath}");
            }
            
            try
            {
                using var stream = File.OpenRead(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return await JsonSerializer.DeserializeAsync<VMTemplate>(stream, options);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Error deserializing template: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Saves a template to a file
        /// </summary>
        /// <param name="template">The template to save</param>
        /// <param name="fileName">Optional file name, or null to generate from template name</param>
        /// <returns>The path to the saved template file</returns>
        public async Task<string> SaveTemplateAsync(VMTemplate template, string fileName = null)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }
            
            if (string.IsNullOrWhiteSpace(template.TemplateName))
            {
                throw new ArgumentException("Template must have a name");
            }
            
            // Generate file name from template name if not provided
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = $"{template.ServerRole}-{template.TemplateName.Replace(" ", "-")}.json";
            }
            
            // Ensure the file has .json extension
            if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".json";
            }
            
            var filePath = Path.Combine(_templateDirectory, fileName);
            
            // Update metadata
            template.Metadata.LastModifiedDate = DateTime.Now;
            
            try
            {
                using var stream = File.Create(filePath);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                await JsonSerializer.SerializeAsync(stream, template, options);
                return filePath;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error saving template: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Saves a template
        /// </summary>
        /// <param name="template">The template to save</param>
        /// <returns>True if the save was successful, false otherwise</returns>
        public bool SaveTemplate(VMTemplate template)
        {
            if (template == null || string.IsNullOrWhiteSpace(template.TemplateName))
            {
                return false;
            }
            
            try
            {
                // Update the template in memory
                _templates[template.TemplateName] = template;
                
                // Save to disk
                var fileName = $"{template.ServerRole}-{template.TemplateName.Replace(" ", "-")}.json";
                var filePath = Path.Combine(_templateDirectory, fileName);
                
                // Update metadata
                template.Metadata.LastModifiedDate = DateTime.Now;
                
                // Serialize and save
                var json = JsonSerializer.Serialize(template, _jsonOptions);
                File.WriteAllText(filePath, json);
                
                return true;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error saving template: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Deletes a template file
        /// </summary>
        /// <param name="templateName">The name of the template to delete</param>
        /// <returns>True if the template was deleted, false otherwise</returns>
        public bool DeleteTemplate(string templateName)
        {
            if (string.IsNullOrWhiteSpace(templateName) || !_templates.ContainsKey(templateName))
            {
                return false;
            }
            
            try
            {
                // Find the file
                var template = _templates[templateName];
                var filePattern = $"{template.ServerRole}-{templateName.Replace(" ", "-")}*.json";
                var files = Directory.GetFiles(_templateDirectory, filePattern);
                
                if (files.Length == 0)
                {
                    // Try a more general search
                    files = Directory.GetFiles(_templateDirectory, "*.json");
                    files = files.Where(f => Path.GetFileNameWithoutExtension(f).Contains(templateName)).ToArray();
                }
                
                if (files.Length > 0)
                {
                    // Delete the file(s)
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }
                }
                
                // Remove from memory
                _templates.Remove(templateName);
                
                return true;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error deleting template: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Creates a copy of a template with a new name
        /// </summary>
        /// <param name="template">The template to clone</param>
        /// <param name="newName">The name for the cloned template</param>
        /// <returns>The cloned template</returns>
        public VMTemplate CloneTemplate(VMTemplate template, string newName)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }
            
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("New template name must be provided");
            }
            
            // Create a deep copy by serializing and deserializing
            var json = JsonSerializer.Serialize(template);
            var clone = JsonSerializer.Deserialize<VMTemplate>(json);
            
            // Update the new template
            clone.TemplateName = newName;
            clone.Metadata.CreatedDate = DateTime.Now;
            clone.Metadata.LastModifiedDate = DateTime.Now;
            clone.Metadata.Author = Environment.UserName;
            
            return clone;
        }

        private void LoadTemplates()
        {
            try
            {
                // Load from template directory
                string[] files = Directory.GetFiles(_templateDirectory, "*.json");
                foreach (string file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        VMTemplate? template = JsonSerializer.Deserialize<VMTemplate>(json, _jsonOptions);
                        
                        if (template != null && !string.IsNullOrWhiteSpace(template.TemplateName))
                        {
                            _templates[template.TemplateName] = template;
                        }
                    }
                    catch
                    {
                        // Skip invalid template files
                    }
                }

                // Load default templates if none exist
                if (_templates.Count == 0)
                {
                    LoadDefaultTemplates();
                }
            }
            catch (Exception)
            {
                // Fallback to default templates
                LoadDefaultTemplates();
            }
        }

        private void LoadDefaultTemplates()
        {
            // Load built-in templates
            CreateDefaultCustomVMTemplate();
            CreateDefaultDomainControllerTemplate();
            CreateDefaultFileServerTemplate();
            CreateDefaultSQLServerTemplate();
            CreateDefaultRDSHTemplate();
            CreateDefaultWebServerTemplate();
            CreateDefaultDHCPServerTemplate();
            CreateDefaultDNSServerTemplate();
        }

        private void CreateDefaultCustomVMTemplate()
        {
            VMTemplate template = new VMTemplate
            {
                TemplateName = "Default Custom VM",
                ServerRole = "CustomVM",
                Description = "A customizable virtual machine template",
                HardwareConfiguration = new HardwareConfig
                {
                    ProcessorCount = 2,
                    MemoryGB = 4,
                    StorageGB = 80,
                    Generation = 2,
                    EnableSecureBoot = true,
                    AdditionalDisks = new List<DiskConfig>
                    {
                        new DiskConfig { SizeGB = 0, Letter = "D", Label = "Data" }
                    }
                },
                NetworkConfiguration = new NetworkConfig
                {
                    VirtualSwitch = "Default Switch",
                    DynamicIP = true,
                    StaticIPConfiguration = new StaticIPConfig
                    {
                        IPAddress = "",
                        SubnetMask = "255.255.255.0",
                        DefaultGateway = "",
                        DNSServers = new List<string> { "", "" }
                    }
                },
                OSConfiguration = new OSConfig
                {
                    OSVersion = "Windows Server 2022 Standard",
                    ProductKey = "",
                    TimeZone = 85,
                    AdminPassword = "P@ssw0rd",
                    ComputerName = "",
                    Organization = "Custom Organization",
                    Owner = "Administrator"
                },
                AdditionalConfiguration = new AdditionalConfig
                {
                    AutoStartVM = false,
                    UseUnattendXML = true,
                    EnableRDP = true,
                    EnablePSRemoting = true
                },
                Metadata = new TemplateMetadata
                {
                    CreatedDate = DateTime.Parse("2023-03-29T00:00:00"),
                    LastModifiedDate = DateTime.Parse("2023-03-29T00:00:00"),
                    Author = "System",
                    Tags = new List<string> { "Custom", "Basic" }
                }
            };

            _templates[template.TemplateName] = template;
            SaveTemplateAsync(template).Wait();
        }

        private void CreateDefaultDomainControllerTemplate()
        {
            VMTemplate template = new VMTemplate
            {
                TemplateName = "Default Domain Controller",
                ServerRole = "DomainController",
                Description = "Default template for Domain Controller",
                HardwareConfiguration = new HardwareConfig
                {
                    ProcessorCount = 2,
                    MemoryGB = 4,
                    StorageGB = 80,
                    Generation = 2,
                    EnableSecureBoot = true,
                    AdditionalDisks = new List<DiskConfig>()
                },
                NetworkConfiguration = new NetworkConfig
                {
                    VirtualSwitch = "Default Switch",
                    DynamicIP = false,
                    StaticIPConfiguration = new StaticIPConfig
                    {
                        IPAddress = "192.168.1.10",
                        SubnetMask = "255.255.255.0",
                        DefaultGateway = "192.168.1.1",
                        DNSServers = new List<string> { "127.0.0.1", "8.8.8.8" }
                    }
                },
                OSConfiguration = new OSConfig
                {
                    OSVersion = "Windows Server 2022 Standard",
                    ProductKey = "",
                    TimeZone = 85,
                    AdminPassword = "P@ssw0rd",
                    ComputerName = "DC01",
                    Organization = "Contoso",
                    Owner = "Administrator"
                },
                AdditionalConfiguration = new AdditionalConfig
                {
                    AutoStartVM = true,
                    UseUnattendXML = true,
                    EnableRDP = true,
                    EnablePSRemoting = true
                },
                Metadata = new TemplateMetadata
                {
                    CreatedDate = DateTime.Parse("2023-03-29T00:00:00"),
                    LastModifiedDate = DateTime.Parse("2023-03-29T00:00:00"),
                    Author = "System",
                    Tags = new List<string> { "DomainController", "Active Directory" }
                }
            };

            _templates[template.TemplateName] = template;
            SaveTemplateAsync(template).Wait();
        }

        private void CreateDefaultFileServerTemplate()
        {
            VMTemplate template = new VMTemplate
            {
                TemplateName = "Default File Server",
                ServerRole = "FileServer",
                Description = "Default template for File Server",
                Metadata = new TemplateMetadata
                {
                    CreatedDate = DateTime.Parse("2023-03-29T00:00:00"),
                    LastModifiedDate = DateTime.Parse("2023-03-29T00:00:00"),
                    Author = "System",
                    Tags = new List<string> { "FileServer" }
                }
            };

            _templates[template.TemplateName] = template;
            SaveTemplateAsync(template).Wait();
        }

        private void CreateDefaultSQLServerTemplate()
        {
            VMTemplate template = new VMTemplate
            {
                TemplateName = "Default SQL Server",
                ServerRole = "SQLServer",
                Description = "Default template for SQL Server",
                Metadata = new TemplateMetadata
                {
                    CreatedDate = DateTime.Parse("2023-03-29T00:00:00"),
                    LastModifiedDate = DateTime.Parse("2023-03-29T00:00:00"),
                    Author = "System",
                    Tags = new List<string> { "SQLServer", "Database" }
                }
            };

            _templates[template.TemplateName] = template;
            SaveTemplateAsync(template).Wait();
        }

        private void CreateDefaultRDSHTemplate()
        {
            VMTemplate template = new VMTemplate
            {
                TemplateName = "Default RDSH",
                ServerRole = "RDSH",
                Description = "Default template for Remote Desktop Session Host",
                Metadata = new TemplateMetadata
                {
                    CreatedDate = DateTime.Parse("2023-03-29T00:00:00"),
                    LastModifiedDate = DateTime.Parse("2023-03-29T00:00:00"),
                    Author = "System",
                    Tags = new List<string> { "RDSH", "RemoteDesktop" }
                }
            };

            _templates[template.TemplateName] = template;
            SaveTemplateAsync(template).Wait();
        }

        private void CreateDefaultWebServerTemplate()
        {
            VMTemplate template = new VMTemplate
            {
                TemplateName = "Default Web Server",
                ServerRole = "WebServer",
                Description = "Default template for Web Server",
                Metadata = new TemplateMetadata
                {
                    CreatedDate = DateTime.Parse("2023-03-29T00:00:00"),
                    LastModifiedDate = DateTime.Parse("2023-03-29T00:00:00"),
                    Author = "System",
                    Tags = new List<string> { "WebServer", "IIS" }
                }
            };

            _templates[template.TemplateName] = template;
            SaveTemplateAsync(template).Wait();
        }

        private void CreateDefaultDHCPServerTemplate()
        {
            VMTemplate template = new VMTemplate
            {
                TemplateName = "Default DHCP Server",
                ServerRole = "DHCPServer",
                Description = "Default template for DHCP Server",
                Metadata = new TemplateMetadata
                {
                    CreatedDate = DateTime.Parse("2023-03-29T00:00:00"),
                    LastModifiedDate = DateTime.Parse("2023-03-29T00:00:00"),
                    Author = "System",
                    Tags = new List<string> { "DHCPServer" }
                }
            };

            _templates[template.TemplateName] = template;
            SaveTemplateAsync(template).Wait();
        }

        private void CreateDefaultDNSServerTemplate()
        {
            VMTemplate template = new VMTemplate
            {
                TemplateName = "Default DNS Server",
                ServerRole = "DNSServer",
                Description = "Default template for DNS Server",
                Metadata = new TemplateMetadata
                {
                    CreatedDate = DateTime.Parse("2023-03-29T00:00:00"),
                    LastModifiedDate = DateTime.Parse("2023-03-29T00:00:00"),
                    Author = "System",
                    Tags = new List<string> { "DNSServer" }
                }
            };

            _templates[template.TemplateName] = template;
            SaveTemplateAsync(template).Wait();
        }
    }
} 
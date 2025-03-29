using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HyperVCreator.Core.Services
{
    /// <summary>
    /// Service for managing VM templates
    /// </summary>
    public class TemplateService
    {
        private readonly string _templateDirectory;
        
        public TemplateService(string templateDirectory = null)
        {
            // Use provided directory or default to the application's template directory
            _templateDirectory = templateDirectory ?? 
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
            
            // Ensure the directory exists
            if (!Directory.Exists(_templateDirectory))
            {
                Directory.CreateDirectory(_templateDirectory);
            }
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
        /// Deletes a template file
        /// </summary>
        /// <param name="fileName">The name of the template file to delete</param>
        public void DeleteTemplate(string fileName)
        {
            var filePath = Path.Combine(_templateDirectory, fileName);
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Template file not found: {filePath}");
            }
            
            File.Delete(filePath);
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
    }
    
    /// <summary>
    /// Represents a virtual machine template
    /// </summary>
    public class VMTemplate
    {
        public string TemplateName { get; set; }
        public string ServerRole { get; set; }
        public string Description { get; set; }
        public HardwareConfig HardwareConfiguration { get; set; }
        public NetworkConfig NetworkConfiguration { get; set; }
        public OSConfig OSConfiguration { get; set; }
        public AdditionalConfig AdditionalConfiguration { get; set; }
        public TemplateMetadata Metadata { get; set; } = new TemplateMetadata();
    }
    
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
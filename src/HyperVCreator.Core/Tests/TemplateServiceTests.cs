using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HyperVCreator.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HyperVCreator.Core.Tests
{
    [TestClass]
    public class TemplateServiceTests
    {
        private string _testTemplateDirectory;
        private TemplateService _templateService;
        
        [TestInitialize]
        public void Initialize()
        {
            // Create a temporary directory for templates
            _testTemplateDirectory = Path.Combine(Path.GetTempPath(), "HyperVCreatorTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testTemplateDirectory);
            
            // Initialize the service with the test directory
            _templateService = new TemplateService(_testTemplateDirectory);
        }
        
        [TestCleanup]
        public void Cleanup()
        {
            // Clean up the test directory
            if (Directory.Exists(_testTemplateDirectory))
            {
                Directory.Delete(_testTemplateDirectory, true);
            }
        }
        
        [TestMethod]
        public async Task SaveAndLoadTemplate_ShouldWorkCorrectly()
        {
            // Arrange
            var template = CreateTestTemplate();
            
            // Act
            var savedPath = await _templateService.SaveTemplateAsync(template);
            var loadedTemplate = await _templateService.LoadTemplateAsync(savedPath);
            
            // Assert
            Assert.IsNotNull(loadedTemplate);
            Assert.AreEqual(template.TemplateName, loadedTemplate.TemplateName);
            Assert.AreEqual(template.ServerRole, loadedTemplate.ServerRole);
            Assert.AreEqual(template.HardwareConfiguration.ProcessorCount, loadedTemplate.HardwareConfiguration.ProcessorCount);
            Assert.AreEqual(template.OSConfiguration.OSVersion, loadedTemplate.OSConfiguration.OSVersion);
        }
        
        [TestMethod]
        public async Task GetAllTemplates_ShouldReturnAllTemplates()
        {
            // Arrange
            var template1 = CreateTestTemplate("Template 1", "CustomVM");
            var template2 = CreateTestTemplate("Template 2", "DomainController");
            var template3 = CreateTestTemplate("Template 3", "FileServer");
            
            await _templateService.SaveTemplateAsync(template1);
            await _templateService.SaveTemplateAsync(template2);
            await _templateService.SaveTemplateAsync(template3);
            
            // Act
            var templates = await _templateService.GetAllTemplatesAsync();
            
            // Assert
            Assert.AreEqual(3, templates.Count);
            Assert.IsTrue(templates.Any(t => t.TemplateName == "Template 1"));
            Assert.IsTrue(templates.Any(t => t.TemplateName == "Template 2"));
            Assert.IsTrue(templates.Any(t => t.TemplateName == "Template 3"));
        }
        
        [TestMethod]
        public async Task GetTemplatesByRole_ShouldFilterCorrectly()
        {
            // Arrange
            var template1 = CreateTestTemplate("Template 1", "CustomVM");
            var template2 = CreateTestTemplate("Template 2", "CustomVM");
            var template3 = CreateTestTemplate("Template 3", "FileServer");
            
            await _templateService.SaveTemplateAsync(template1);
            await _templateService.SaveTemplateAsync(template2);
            await _templateService.SaveTemplateAsync(template3);
            
            // Act
            var customVMTemplates = await _templateService.GetTemplatesByRoleAsync("CustomVM");
            var fileServerTemplates = await _templateService.GetTemplatesByRoleAsync("FileServer");
            var nonExistingRoleTemplates = await _templateService.GetTemplatesByRoleAsync("NonExisting");
            
            // Assert
            Assert.AreEqual(2, customVMTemplates.Count);
            Assert.AreEqual(1, fileServerTemplates.Count);
            Assert.AreEqual(0, nonExistingRoleTemplates.Count);
        }
        
        [TestMethod]
        public async Task DeleteTemplate_ShouldRemoveFile()
        {
            // Arrange
            var template = CreateTestTemplate();
            var fileName = "test-template.json";
            await _templateService.SaveTemplateAsync(template, fileName);
            
            // Act
            _templateService.DeleteTemplate(fileName);
            
            // Assert
            Assert.IsFalse(File.Exists(Path.Combine(_testTemplateDirectory, fileName)));
            
            // Act & Assert - Trying to delete non-existing file should throw
            Assert.ThrowsException<FileNotFoundException>(() => _templateService.DeleteTemplate(fileName));
        }
        
        [TestMethod]
        public void CloneTemplate_ShouldCreateNewInstance()
        {
            // Arrange
            var template = CreateTestTemplate();
            
            // Act
            var clonedTemplate = _templateService.CloneTemplate(template, "Cloned Template");
            
            // Assert
            Assert.IsNotNull(clonedTemplate);
            Assert.AreEqual("Cloned Template", clonedTemplate.TemplateName);
            Assert.AreEqual(template.ServerRole, clonedTemplate.ServerRole);
            
            // Verify deep copy by modifying the clone and checking the original
            clonedTemplate.OSConfiguration.OSVersion = "Modified Version";
            Assert.AreNotEqual(clonedTemplate.OSConfiguration.OSVersion, template.OSConfiguration.OSVersion);
        }
        
        private VMTemplate CreateTestTemplate(string name = "Test Template", string role = "CustomVM")
        {
            return new VMTemplate
            {
                TemplateName = name,
                ServerRole = role,
                Description = "Test template description",
                HardwareConfiguration = new HardwareConfig
                {
                    ProcessorCount = 2,
                    MemoryGB = 4,
                    StorageGB = 80,
                    Generation = 2,
                    EnableSecureBoot = true
                },
                NetworkConfiguration = new NetworkConfig
                {
                    VirtualSwitch = "Test Switch",
                    DynamicIP = true
                },
                OSConfiguration = new OSConfig
                {
                    OSVersion = "Windows Server 2022 Standard",
                    TimeZone = 85,
                    AdminPassword = "TestPassword",
                    ComputerName = "TEST-VM"
                },
                AdditionalConfiguration = new AdditionalConfig
                {
                    AutoStartVM = false,
                    UseUnattendXML = true,
                    EnableRDP = true
                },
                Metadata = new TemplateMetadata
                {
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    Author = "Test Author",
                    Tags = new System.Collections.Generic.List<string> { "Test", "UnitTest" }
                }
            };
        }
    }
} 
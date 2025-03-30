using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HyperVCreator.Core.Models;
using HyperVCreator.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HyperVCreator.Core.Tests
{
    [TestClass]
    public class TemplateServiceTests
    {
        private string _testTemplatePath;
        private TemplateService _templateService;
        
        [TestInitialize]
        public void Setup()
        {
            // Create a temporary directory for test templates
            _testTemplatePath = Path.Combine(Path.GetTempPath(), "HyperVCreatorTests", "Templates", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testTemplatePath);
            
            // Create template service with test path
            _templateService = new TemplateService(_testTemplatePath);
        }
        
        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test directory
            if (Directory.Exists(_testTemplatePath))
            {
                Directory.Delete(_testTemplatePath, true);
            }
        }
        
        [TestMethod]
        public void GetAllTemplates_ReturnsDefaultTemplates()
        {
            // Act
            var templates = _templateService.GetAllTemplates();
            
            // Assert
            Assert.IsNotNull(templates);
            Assert.IsTrue(templates.Count > 0);
            Assert.IsTrue(templates.Any(t => t.ServerRole == "CustomVM"));
            Assert.IsTrue(templates.Any(t => t.ServerRole == "DomainController"));
        }
        
        [TestMethod]
        public void GetTemplateByName_ReturnsCorrectTemplate()
        {
            // Arrange
            string templateName = "Default Custom VM";
            
            // Act
            var template = _templateService.GetTemplateByName(templateName);
            
            // Assert
            Assert.IsNotNull(template);
            Assert.AreEqual(templateName, template.TemplateName);
            Assert.AreEqual("CustomVM", template.ServerRole);
        }
        
        [TestMethod]
        public void GetTemplateByName_ReturnsNull_WhenTemplateDoesNotExist()
        {
            // Act
            var template = _templateService.GetTemplateByName("Non-existent template");
            
            // Assert
            Assert.IsNull(template);
        }
        
        [TestMethod]
        public void SaveTemplate_SavesTemplateSuccessfully()
        {
            // Arrange
            var template = new VMTemplate
            {
                TemplateName = "Test Template",
                ServerRole = "TestRole",
                Description = "Test description"
            };
            
            // Act
            bool result = _templateService.SaveTemplate(template);
            
            // Assert
            Assert.IsTrue(result);
            
            // Verify template was saved
            var savedTemplate = _templateService.GetTemplateByName("Test Template");
            Assert.IsNotNull(savedTemplate);
            Assert.AreEqual("Test Template", savedTemplate.TemplateName);
            Assert.AreEqual("TestRole", savedTemplate.ServerRole);
            Assert.AreEqual("Test description", savedTemplate.Description);
        }
        
        [TestMethod]
        public void SaveTemplate_ReturnsFalse_WhenTemplateNameIsNull()
        {
            // Arrange
            var template = new VMTemplate
            {
                TemplateName = null,
                ServerRole = "TestRole"
            };
            
            // Act
            bool result = _templateService.SaveTemplate(template);
            
            // Assert
            Assert.IsFalse(result);
        }
        
        [TestMethod]
        public void DeleteTemplate_DeletesTemplateSuccessfully()
        {
            // Arrange
            var template = new VMTemplate
            {
                TemplateName = "Template To Delete",
                ServerRole = "TestRole"
            };
            _templateService.SaveTemplate(template);
            
            // Act
            bool result = _templateService.DeleteTemplate("Template To Delete");
            
            // Assert
            Assert.IsTrue(result);
            
            // Verify template was deleted
            var deletedTemplate = _templateService.GetTemplateByName("Template To Delete");
            Assert.IsNull(deletedTemplate);
        }
        
        [TestMethod]
        public void DeleteTemplate_ReturnsFalse_WhenTemplateDoesNotExist()
        {
            // Act
            bool result = _templateService.DeleteTemplate("Non-existent template");
            
            // Assert
            Assert.IsFalse(result);
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
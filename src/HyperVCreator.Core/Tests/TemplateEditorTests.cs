using System;
using System.IO;
using System.Threading.Tasks;
using HyperVCreator.Core.Services;
using HyperVCreator.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HyperVCreator.Core.Tests
{
    [TestClass]
    public class TemplateEditorTests
    {
        private readonly string _testTemplatePath = Path.Combine(Path.GetTempPath(), "HyperVCreatorTests", "Templates");
        private TemplateService _templateService;

        [TestInitialize]
        public void Initialize()
        {
            // Ensure test directory exists
            if (Directory.Exists(_testTemplatePath))
            {
                Directory.Delete(_testTemplatePath, true);
            }
            
            Directory.CreateDirectory(_testTemplatePath);
            
            // Create template service using test directory
            _templateService = new TemplateService(_testTemplatePath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testTemplatePath))
            {
                Directory.Delete(_testTemplatePath, true);
            }
        }

        [TestMethod]
        public async Task CreateAndSaveTemplate_Success()
        {
            // Arrange
            var template = new Models.VMTemplate
            {
                TemplateName = "Test Template",
                ServerRole = "CustomVM",
                Description = "Test template for unit tests",
                HardwareConfiguration = new Models.HardwareConfig
                {
                    ProcessorCount = 2,
                    MemoryGB = 4,
                    StorageGB = 80,
                    Generation = 2,
                    EnableSecureBoot = true
                },
                NetworkConfiguration = new Models.NetworkConfig
                {
                    VirtualSwitch = "Default Switch",
                    DynamicIP = true
                },
                OSConfiguration = new Models.OSConfig
                {
                    OSVersion = "Windows Server 2022 Standard",
                    AdminPassword = "P@ssw0rd"
                },
                AdditionalConfiguration = new Models.AdditionalConfig
                {
                    UseUnattendXML = true,
                    EnableRDP = true
                },
                Metadata = new Models.TemplateMetadata
                {
                    Author = "Test User",
                    Tags = new System.Collections.Generic.List<string> { "Test", "UnitTest" }
                }
            };

            // Act
            string filePath = await _templateService.SaveTemplateAsync(template);
            var loadedTemplate = await _templateService.LoadTemplateAsync(filePath);

            // Assert
            Assert.IsNotNull(loadedTemplate);
            Assert.AreEqual("Test Template", loadedTemplate.TemplateName);
            Assert.AreEqual("CustomVM", loadedTemplate.ServerRole);
            Assert.AreEqual(2, loadedTemplate.HardwareConfiguration.ProcessorCount);
            Assert.AreEqual(4, loadedTemplate.HardwareConfiguration.MemoryGB);
            Assert.IsTrue(loadedTemplate.NetworkConfiguration.DynamicIP);
            Assert.AreEqual("Windows Server 2022 Standard", loadedTemplate.OSConfiguration.OSVersion);
            Assert.IsTrue(loadedTemplate.AdditionalConfiguration.EnableRDP);
            Assert.AreEqual(2, loadedTemplate.Metadata.Tags.Count);
        }

        [TestMethod]
        public async Task GetTemplatesByRole_ReturnsMatchingTemplates()
        {
            // Arrange
            var template1 = new Models.VMTemplate { TemplateName = "Template1", ServerRole = "SQLServer" };
            var template2 = new Models.VMTemplate { TemplateName = "Template2", ServerRole = "WebServer" };
            var template3 = new Models.VMTemplate { TemplateName = "Template3", ServerRole = "SQLServer" };
            
            await _templateService.SaveTemplateAsync(template1);
            await _templateService.SaveTemplateAsync(template2);
            await _templateService.SaveTemplateAsync(template3);

            // Act
            var sqlTemplates = await _templateService.GetTemplatesByRoleAsync("SQLServer");
            var webTemplates = await _templateService.GetTemplatesByRoleAsync("WebServer");
            var fileTemplates = await _templateService.GetTemplatesByRoleAsync("FileServer");

            // Assert
            Assert.AreEqual(2, sqlTemplates.Count);
            Assert.AreEqual(1, webTemplates.Count);
            Assert.AreEqual(0, fileTemplates.Count);
        }

        [TestMethod]
        public async Task DeleteTemplate_RemovesFromList()
        {
            // Arrange
            var template = new Models.VMTemplate { TemplateName = "ToDelete", ServerRole = "CustomVM" };
            string filePath = await _templateService.SaveTemplateAsync(template);
            string fileName = Path.GetFileName(filePath);
            
            var initialTemplates = await _templateService.GetAllTemplatesAsync();
            Assert.AreEqual(1, initialTemplates.Count);

            // Act
            _templateService.DeleteTemplate(fileName);
            var remainingTemplates = await _templateService.GetAllTemplatesAsync();

            // Assert
            Assert.AreEqual(0, remainingTemplates.Count);
            Assert.IsFalse(File.Exists(filePath));
        }

        [TestMethod]
        public void CloneTemplate_CreatesDistinctCopy()
        {
            // Arrange
            var originalTemplate = new Models.VMTemplate
            {
                TemplateName = "Original",
                ServerRole = "CustomVM",
                Description = "Original template",
                Metadata = new Models.TemplateMetadata
                {
                    Author = "Original Author",
                    Tags = new System.Collections.Generic.List<string> { "Original" }
                }
            };

            // Act
            var clonedTemplate = _templateService.CloneTemplate(originalTemplate, "Cloned");

            // Assert
            Assert.AreEqual("Cloned", clonedTemplate.TemplateName);
            Assert.AreEqual("CustomVM", clonedTemplate.ServerRole);
            Assert.AreEqual("Original template", clonedTemplate.Description);
            Assert.AreNotEqual(originalTemplate.Metadata.Author, clonedTemplate.Metadata.Author);
            Assert.AreEqual(Environment.UserName, clonedTemplate.Metadata.Author);

            // Ensure they are separate objects
            originalTemplate.Description = "Modified original";
            Assert.AreNotEqual(originalTemplate.Description, clonedTemplate.Description);
        }
    }
} 
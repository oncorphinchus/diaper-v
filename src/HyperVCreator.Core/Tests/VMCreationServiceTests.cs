using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HyperVCreator.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HyperVCreator.Core.Models;
using System.Linq;

namespace HyperVCreator.Core.Tests
{
    [TestClass]
    public class VMCreationServiceTests
    {
        private string _testDirectory;
        private string _templatesDirectory;
        private string _scriptsDirectory;
        private VMCreationService _vmCreationService;
        
        [TestInitialize]
        public void Initialize()
        {
            // Create temporary directories for testing
            _testDirectory = Path.Combine(Path.GetTempPath(), "HyperVCreatorTests", Guid.NewGuid().ToString());
            _templatesDirectory = Path.Combine(_testDirectory, "Templates");
            _scriptsDirectory = Path.Combine(_testDirectory, "Scripts");
            
            Directory.CreateDirectory(_testDirectory);
            Directory.CreateDirectory(_templatesDirectory);
            Directory.CreateDirectory(_scriptsDirectory);
            Directory.CreateDirectory(Path.Combine(_scriptsDirectory, "RoleConfiguration"));
            
            // Create a mock script for testing
            CreateMockScript();
            
            // Create a test template
            CreateTestTemplate();
            
            // Initialize the service with test directories
            _vmCreationService = new VMCreationService(_scriptsDirectory, _templatesDirectory);
        }
        
        [TestCleanup]
        public void Cleanup()
        {
            // Dispose the service
            _vmCreationService?.Dispose();
            
            // Clean up test directories
            if (Directory.Exists(_testDirectory))
            {
                try
                {
                    Directory.Delete(_testDirectory, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
        
        [TestMethod]
        public async Task CreateVM_WithValidTemplate_ShouldSucceed()
        {
            // Arrange
            var template = CreateVMTemplate();
            var progressList = new List<VMCreationProgress>();
            var progress = new Progress<VMCreationProgress>(p => progressList.Add(p));
            
            // Act
            var result = await _vmCreationService.CreateVMAsync(template, progress);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual("Test VM", result.TemplateName);
            Assert.AreEqual("CustomVM", result.ServerRole);
            
            // Check progress reporting
            Assert.IsTrue(progressList.Count > 0);
            Assert.AreEqual(100, progressList[progressList.Count - 1].PercentComplete);
        }
        
        [TestMethod]
        public async Task CreateVMFromRole_WithValidParameters_ShouldSucceed()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["VMName"] = "TestRoleVM",
                ["CPUCount"] = 2,
                ["MemoryGB"] = 4,
                ["StorageGB"] = 80
            };
            
            var progressList = new List<VMCreationProgress>();
            var progress = new Progress<VMCreationProgress>(p => progressList.Add(p));
            
            // Act
            var result = await _vmCreationService.CreateVMForRoleAsync("CustomVM", parameters, progress);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual("CustomVM", result.ServerRole);
            
            // Check progress reporting
            Assert.IsTrue(progressList.Count > 0);
            Assert.AreEqual(100, progressList[progressList.Count - 1].PercentComplete);
        }
        
        [TestMethod]
        public async Task CreateVMFromTemplateFile_WithValidPath_ShouldSucceed()
        {
            // Arrange
            var templatePath = Path.Combine(_templatesDirectory, "test-template.json");
            var progressList = new List<VMCreationProgress>();
            var progress = new Progress<VMCreationProgress>(p => progressList.Add(p));
            
            // Act
            var result = await _vmCreationService.CreateVMFromTemplateFileAsync(templatePath, progress);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual("Test VM", result.TemplateName);
            Assert.AreEqual("CustomVM", result.ServerRole);
            
            // Check progress reporting
            Assert.IsTrue(progressList.Count > 0);
            Assert.AreEqual(100, progressList[progressList.Count - 1].PercentComplete);
        }
        
        [TestMethod]
        public async Task CreateVM_WithInvalidPath_ShouldFail()
        {
            // Arrange
            var templatePath = Path.Combine(_templatesDirectory, "non-existent-template.json");
            var progressList = new List<VMCreationProgress>();
            var progress = new Progress<VMCreationProgress>(p => progressList.Add(p));
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () => 
                await _vmCreationService.CreateVMFromTemplateFileAsync(templatePath, progress));
        }
        
        [TestMethod]
        public async Task CreateVM_WithCancellation_ShouldCancel()
        {
            // Arrange
            var template = CreateVMTemplate();
            var progressList = new List<VMCreationProgress>();
            var progress = new Progress<VMCreationProgress>(p => progressList.Add(p));
            
            // Create cancellation token and cancel immediately
            var cts = new CancellationTokenSource();
            cts.Cancel();
            
            // Act & Assert
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => 
                await _vmCreationService.CreateVMAsync(template, progress, cts.Token));
        }
        
        private VMTemplate CreateVMTemplate()
        {
            return new VMTemplate
            {
                TemplateName = "Test VM",
                ServerRole = "CustomVM",
                Description = "Test VM for unit testing",
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
                    Tags = new List<string> { "Test", "UnitTest" }
                }
            };
        }
        
        private void CreateTestTemplate()
        {
            var template = CreateVMTemplate();
            var templateService = new TemplateService(_templatesDirectory);
            templateService.SaveTemplateAsync(template, "test-template.json").GetAwaiter().GetResult();
        }
        
        private void CreateMockScript()
        {
            // Create a mock CustomVM.ps1 script
            var scriptPath = Path.Combine(_scriptsDirectory, "RoleConfiguration", "CustomVM.ps1");
            var directoryPath = Path.GetDirectoryName(scriptPath);
            
            // Ensure directory exists before creating the file
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            // Simple script that returns success and status updates
            string scriptContent = @"
param (
    [Parameter(Mandatory=$true)]
    [string]$VMName,
    
    [Parameter(Mandatory=$false)]
    [int]$CPUCount = 2,
    
    [Parameter(Mandatory=$false)]
    [int]$MemoryGB = 4,
    
    [Parameter(Mandatory=$false)]
    [int]$StorageGB = 80,
    
    [Parameter(Mandatory=$false)]
    [string]$OSVersion = ""Windows Server 2022 Standard""
)

# Send status updates
$statusUpdate1 = [PSCustomObject]@{
    Type = ""StatusUpdate""
    PercentComplete = 25
    StatusMessage = ""Creating virtual machine...""
}
Write-Output $statusUpdate1

Start-Sleep -Milliseconds 100

$statusUpdate2 = [PSCustomObject]@{
    Type = ""StatusUpdate""
    PercentComplete = 50
    StatusMessage = ""Configuring operating system...""
}
Write-Output $statusUpdate2

Start-Sleep -Milliseconds 100

$statusUpdate3 = [PSCustomObject]@{
    Type = ""StatusUpdate""
    PercentComplete = 75
    StatusMessage = ""Finalizing VM creation...""
}
Write-Output $statusUpdate3

Start-Sleep -Milliseconds 100

# Return success result
$result = [PSCustomObject]@{
    Type = ""Result""
    Success = $true
    Message = ""VM '$VMName' created successfully.""
    VMName = $VMName
    CPUCount = $CPUCount
    MemoryGB = $MemoryGB
    StorageGB = $StorageGB
    OSVersion = $OSVersion
}

Write-Output $result
";
            
            File.WriteAllText(scriptPath, scriptContent);
        }
    }
} 
using System;
using System.Linq;
using System.Threading.Tasks;
using HyperVCreator.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HyperVCreator.Core.Tests
{
    [TestClass]
    public class PowerShellServiceTests
    {
        private PowerShellService _powerShellService;
        
        [TestInitialize]
        public void Setup()
        {
            _powerShellService = new PowerShellService();
        }
        
        [TestMethod]
        public async Task ExecuteScriptAsync_ReturnsExpectedOutput()
        {
            // Arrange
            string script = "Write-Output 'Test Output'";
            
            // Act
            var result = await _powerShellService.ExecuteScriptAsync(script);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Test Output", result[0]);
        }
        
        [TestMethod]
        public async Task ExecuteScriptAsync_HandlesMultipleOutputs()
        {
            // Arrange
            string script = @"
                Write-Output 'Line 1'
                Write-Output 'Line 2'
                Write-Output 'Line 3'
            ";
            
            // Act
            var result = await _powerShellService.ExecuteScriptAsync(script);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("Line 1", result[0]);
            Assert.AreEqual("Line 2", result[1]);
            Assert.AreEqual("Line 3", result[2]);
        }
        
        [TestMethod]
        public async Task ExecuteScriptAsync_HandlesErrorOutput()
        {
            // Arrange
            string script = "Write-Error 'Test Error'";
            bool errorReceived = false;
            
            _powerShellService.ErrorReceived += (sender, error) => 
            {
                if (error.Contains("Test Error"))
                {
                    errorReceived = true;
                }
            };
            
            // Act
            var result = await _powerShellService.ExecuteScriptAsync(script);
            
            // Assert
            Assert.IsTrue(result.Any(r => r.Contains("Error")));
            Assert.IsTrue(errorReceived);
        }
        
        [TestMethod]
        public async Task ExecuteScriptAsync_HandlesVariableAssignment()
        {
            // Arrange
            string script = @"
                $var = 'Test Value'
                Write-Output $var
            ";
            
            // Act
            var result = await _powerShellService.ExecuteScriptAsync(script);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Test Value", result[0]);
        }
        
        [TestMethod]
        public async Task ExecuteScriptAsync_HandlesArithmetic()
        {
            // Arrange
            string script = @"
                $a = 5
                $b = 10
                $sum = $a + $b
                Write-Output $sum
            ";
            
            // Act
            var result = await _powerShellService.ExecuteScriptAsync(script);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("15", result[0]);
        }
        
        [TestMethod]
        public async Task ExecuteScriptAsync_HandlesConditionals()
        {
            // Arrange
            string script = @"
                $value = 10
                if ($value -gt 5) {
                    Write-Output 'Greater than 5'
                } else {
                    Write-Output 'Less than or equal to 5'
                }
            ";
            
            // Act
            var result = await _powerShellService.ExecuteScriptAsync(script);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Greater than 5", result[0]);
        }
    }
} 
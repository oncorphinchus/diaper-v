using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using HyperVCreator.Core.Services;
using HyperVCreator.Core.PowerShell;
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
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Output.Count);
            Assert.AreEqual("Test Output", result.Output[0]);
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
            Assert.IsTrue(result.Success);
            Assert.AreEqual(3, result.Output.Count);
            Assert.AreEqual("Line 1", result.Output[0]);
            Assert.AreEqual("Line 2", result.Output[1]);
            Assert.AreEqual("Line 3", result.Output[2]);
        }
        
        [TestMethod]
        public async Task ExecuteScriptAsync_HandlesErrorOutput()
        {
            // Arrange
            string script = "Write-Error 'Test Error'";
            
            // Act
            var result = await _powerShellService.ExecuteScriptAsync(script);
            
            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.IsNotNull(result.Errors);
            Assert.IsTrue(result.Errors.Count > 0);
            Assert.IsTrue(result.Errors.Any(e => e.ToString().Contains("Test Error")));
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
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Output.Count);
            Assert.AreEqual("Test Value", result.Output[0]);
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
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Output.Count);
            Assert.AreEqual("15", result.Output[0]);
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
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Output.Count);
            Assert.AreEqual("Greater than 5", result.Output[0]);
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HyperVCreator.Core.Tests
{
    [TestClass]
    public class PowerShellEnvironmentTests
    {
        [TestMethod]
        public void CanCreateRunspace()
        {
            // Arrange & Act
            var runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();

            // Assert
            Assert.IsNotNull(runspace);
            Assert.AreEqual(RunspaceState.Opened, runspace.RunspaceStateInfo.State);

            // Cleanup
            runspace.Close();
            runspace.Dispose();
        }

        [TestMethod]
        public void GetPowerShellVersion()
        {
            // Arrange
            var runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            var powershell = System.Management.Automation.PowerShell.Create();
            powershell.Runspace = runspace;

            // Act
            powershell.AddScript("$PSVersionTable");
            var result = powershell.Invoke();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
            foreach (var item in result)
            {
                foreach (var property in item.Properties)
                {
                    Console.WriteLine($"{property.Name}: {property.Value}");
                }
            }

            // Cleanup
            powershell.Dispose();
            runspace.Close();
            runspace.Dispose();
        }

        [TestMethod]
        public void CanExecuteSimpleCommands()
        {
            // Arrange
            var runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            var powershell = System.Management.Automation.PowerShell.Create();
            powershell.Runspace = runspace;

            // Act
            powershell.AddScript("Get-Process | Select-Object -First 1");
            var result = powershell.Invoke();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
            
            Console.WriteLine("Process properties:");
            foreach (var property in result[0].Properties)
            {
                Console.WriteLine($"  {property.Name}: {property.Value}");
            }

            // Cleanup
            powershell.Dispose();
            runspace.Close();
            runspace.Dispose();
        }

        [TestMethod]
        public void CanGetExecutionPolicy()
        {
            // Arrange
            var runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            var powershell = System.Management.Automation.PowerShell.Create();
            powershell.Runspace = runspace;

            // Act
            powershell.AddScript("Get-ExecutionPolicy");
            var result = powershell.Invoke<string>();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
            
            Console.WriteLine($"Current execution policy: {result[0]}");

            // Cleanup
            powershell.Dispose();
            runspace.Close();
            runspace.Dispose();
        }

        [TestMethod]
        public void CanGetEnvironmentVariables()
        {
            // Arrange
            var runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            var powershell = System.Management.Automation.PowerShell.Create();
            powershell.Runspace = runspace;

            // Act
            powershell.AddScript("Get-ChildItem env: | Select-Object -First 5");
            var result = powershell.Invoke();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
            
            Console.WriteLine("Environment variables (first 5):");
            foreach (var item in result)
            {
                Console.WriteLine($"  {item.Properties["Name"].Value}: {item.Properties["Value"].Value}");
            }

            // Cleanup
            powershell.Dispose();
            runspace.Close();
            runspace.Dispose();
        }

        [TestMethod]
        public void CanCheckCommandAvailability()
        {
            // Arrange
            var runspace = RunspaceFactory.CreateRunspace();
            runspace.Open();
            var powershell = System.Management.Automation.PowerShell.Create();
            powershell.Runspace = runspace;

            // Act - Check if Hyper-V commands are available
            powershell.AddScript("Get-Command -Module Hyper-V | Measure-Object | Select-Object -ExpandProperty Count");
            var result = powershell.Invoke<int>();

            // Assert
            Assert.IsNotNull(result);
            
            if (result.Count > 0 && result[0] > 0)
            {
                Console.WriteLine($"Hyper-V commands available: {result[0]} commands found");
            }
            else
            {
                Console.WriteLine("Hyper-V commands are not available");
            }

            // Cleanup
            powershell.Dispose();
            runspace.Close();
            runspace.Dispose();
        }
    }
} 
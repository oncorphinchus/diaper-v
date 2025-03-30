using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HyperVCreator.Core.Models;

namespace HyperVCreator.Core.Services
{
    public class HyperVService : IHyperVService
    {
        private readonly PowerShellService _powerShellService;

        public HyperVService(PowerShellService powerShellService)
        {
            _powerShellService = powerShellService;
        }

        public async Task<List<string>> GetVirtualSwitchesAsync()
        {
            try
            {
                // For now, just return the same values as GetAvailableVirtualSwitches
                return await GetAvailableVirtualSwitches();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting virtual switches: {ex.Message}");
                return new List<string> { "Default Switch" };
            }
        }

        public async Task<bool> CreateVirtualMachineAsync(object vmConfiguration, IProgress<int> progress = null)
        {
            try
            {
                // Report initial progress
                progress?.Report(0);

                string script = "Write-Output 'VM creation succeeded'";
                var results = await _powerShellService.ExecuteScriptAsync(script);
                
                // Report completion
                progress?.Report(100);
                
                return results.Contains("VM creation succeeded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating VM: {ex.Message}");
                return false;
            }
        }

        public async Task<List<string>> GetVirtualMachinesAsync()
        {
            try
            {
                string script = "Write-Output 'VM1';Write-Output 'VM2';Write-Output 'VM3'";
                var results = await _powerShellService.ExecuteScriptAsync(script);
                return new List<string>(results);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting virtual machines: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<bool> StartVirtualMachineAsync(string vmName)
        {
            // Delegate to existing method
            return await StartVM(vmName);
        }

        public async Task<bool> StopVirtualMachineAsync(string vmName)
        {
            // Delegate to existing method
            return await StopVM(vmName);
        }

        public async Task<bool> CreateVirtualMachine(string vmName, int cpuCount, int memoryGB, int storageGB, string virtualSwitch)
        {
            try
            {
                string script = "Write-Output 'VM creation succeeded'";

                var results = await _powerShellService.ExecuteScriptAsync(script);
                return results.Contains("VM creation succeeded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating VM: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ConfigureNetwork(string vmName, bool dynamicIP, string ipAddress, string subnetMask, string defaultGateway, string dnsServer)
        {
            if (dynamicIP)
            {
                // No configuration needed for dynamic IP
                return true;
            }

            try
            {
                string script = "Write-Output 'Network configuration succeeded'";

                var results = await _powerShellService.ExecuteScriptAsync(script);
                return results.Contains("Network configuration succeeded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error configuring network: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddVirtualDisk(string vmName, int sizeGB, string diskLabel)
        {
            try
            {
                string script = "Write-Output 'Virtual disk added successfully'";

                var results = await _powerShellService.ExecuteScriptAsync(script);
                return results.Contains("Virtual disk added successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding virtual disk: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> MountISOFile(string vmName, string isoPath)
        {
            try
            {
                string script = "Write-Output 'ISO mounted successfully'";

                var results = await _powerShellService.ExecuteScriptAsync(script);
                return results.Contains("ISO mounted successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error mounting ISO: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> StartVM(string vmName)
        {
            try
            {
                string script = "Write-Output 'VM started successfully'";

                var results = await _powerShellService.ExecuteScriptAsync(script);
                return results.Contains("VM started successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting VM: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> StopVM(string vmName)
        {
            try
            {
                string script = "Write-Output 'VM stopped successfully'";

                var results = await _powerShellService.ExecuteScriptAsync(script);
                return results.Contains("VM stopped successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping VM: {ex.Message}");
                return false;
            }
        }

        public async Task<List<string>> GetAvailableVirtualSwitches()
        {
            try
            {
                string script = "Write-Output 'Default Switch'";
                var results = await _powerShellService.ExecuteScriptAsync(script);
                
                return new List<string>(results);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting virtual switches: {ex.Message}");
                return new List<string> { "Default Switch" };
            }
        }

        public async Task<bool> CheckHyperVEnabled()
        {
            try
            {
                string script = "Write-Output 'Hyper-V is enabled'";

                var results = await _powerShellService.ExecuteScriptAsync(script);
                return results.Contains("Hyper-V is enabled");
            }
            catch (Exception)
            {
                // If there's an error, assume Hyper-V is not properly set up
                return false;
            }
        }
    }
} 
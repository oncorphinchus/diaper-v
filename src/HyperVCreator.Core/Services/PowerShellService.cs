using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using HyperVCreator.Core.Models;
using HyperVCreator.Core.PowerShell;

namespace HyperVCreator.Core.Services
{
    /// <summary>
    /// Provides VM creation and management services using PowerShell
    /// </summary>
    public class PowerShellService
    {
        private readonly PowerShellExecutor _executor;
        private readonly string _scriptsBasePath;
        private readonly IProgressService _progressService;
        private readonly ILogService _logService;

        /// <summary>
        /// Initializes a new instance of the PowerShellService
        /// </summary>
        /// <param name="scriptsBasePath">Base path for PowerShell scripts</param>
        /// <param name="progressService">Service for reporting progress</param>
        /// <param name="logService">Service for logging</param>
        public PowerShellService(string scriptsBasePath, IProgressService progressService, ILogService logService)
        {
            _scriptsBasePath = scriptsBasePath;
            _executor = new PowerShellExecutor(scriptsBasePath);
            _progressService = progressService;
            _logService = logService;
        }

        /// <summary>
        /// Creates a new VM with the specified parameters
        /// </summary>
        /// <param name="vmParams">VM creation parameters</param>
        /// <returns>Task representing the asynchronous operation with result indicating success or failure</returns>
        public async Task<OperationResult> CreateVMAsync(VMCreationParameters vmParams)
        {
            try
            {
                _logService.LogInfo($"Starting VM creation for {vmParams.VMName}");
                _progressService.ReportProgress(0, "Starting VM creation process...");

                // Create parameters dictionary for PowerShell script
                var scriptParams = new Dictionary<string, object>
                {
                    { "VMName", vmParams.VMName },
                    { "CPUCount", vmParams.CPUCount },
                    { "MemoryGB", vmParams.MemoryGB },
                    { "StorageGB", vmParams.StorageGB },
                    { "VirtualSwitch", vmParams.VirtualSwitch },
                    { "Generation", vmParams.Generation }
                };

                // Add optional parameters if specified
                if (!string.IsNullOrEmpty(vmParams.VHDPath))
                {
                    scriptParams.Add("VHDPath", vmParams.VHDPath);
                }

                // Execute VM creation script
                _logService.LogInfo("Executing CreateVM.ps1 script");
                var result = await _executor.ExecuteScriptAsync("CreateVM", scriptParams);

                if (!result.Success)
                {
                    _logService.LogError($"VM creation failed: {result.ErrorMessage}");
                    return new OperationResult 
                    { 
                        Success = false, 
                        ErrorMessage = result.ErrorMessage 
                    };
                }

                // Process script output for status updates and results
                ProcessPowerShellOutput(result.Output);

                // Configure network if needed
                if (vmParams.NetworkConfiguration != null)
                {
                    await ConfigureNetworkAsync(vmParams.VMName, vmParams.NetworkConfiguration);
                }

                // Configure additional storage if needed
                if (vmParams.AdditionalDisks != null && vmParams.AdditionalDisks.Any())
                {
                    await ConfigureAdditionalStorageAsync(vmParams.VMName, vmParams.AdditionalDisks);
                }

                // Start tracking VM progress
                _logService.LogInfo($"Starting progress tracking for VM {vmParams.VMName}");
                await TrackVMCreationProgressAsync(vmParams.VMName);

                // Find success result in output
                var successResult = FindScriptResult(result.Output);
                if (successResult != null)
                {
                    _progressService.ReportProgress(100, "VM creation completed successfully");
                    _logService.LogInfo($"VM {vmParams.VMName} created successfully");
                    return new OperationResult { Success = true };
                }

                // Should not reach here if VM was created successfully
                _logService.LogWarning("VM creation process completed but no success result found");
                return new OperationResult 
                { 
                    Success = true, 
                    WarningMessage = "VM creation process completed but no explicit success result was received" 
                };
            }
            catch (Exception ex)
            {
                _logService.LogError($"Exception during VM creation: {ex.Message}");
                return new OperationResult 
                { 
                    Success = false, 
                    ErrorMessage = $"VM creation failed with exception: {ex.Message}" 
                };
            }
        }

        /// <summary>
        /// Configures network settings for a VM
        /// </summary>
        /// <param name="vmName">Name of the VM</param>
        /// <param name="networkConfig">Network configuration</param>
        /// <returns>Task representing the asynchronous operation</returns>
        private async Task ConfigureNetworkAsync(string vmName, NetworkConfiguration networkConfig)
        {
            _logService.LogInfo($"Configuring network for VM {vmName}");
            _progressService.ReportProgress(60, "Configuring network settings...");

            var scriptParams = new Dictionary<string, object>
            {
                { "VMName", vmName },
                { "VirtualSwitch", networkConfig.VirtualSwitch },
                { "StaticIP", networkConfig.StaticIP }
            };

            // Add static IP parameters if needed
            if (networkConfig.StaticIP)
            {
                scriptParams.Add("IPAddress", networkConfig.IPAddress);
                scriptParams.Add("SubnetMask", networkConfig.SubnetMask);
                scriptParams.Add("DefaultGateway", networkConfig.DefaultGateway);
                scriptParams.Add("DNSServers", networkConfig.DNSServers);
            }

            // Add VLAN parameters if needed
            if (networkConfig.EnableVLAN)
            {
                scriptParams.Add("EnableVLAN", true);
                scriptParams.Add("VLANId", networkConfig.VLANId);
            }

            var result = await _executor.ExecuteScriptAsync("ConfigureNetwork", scriptParams);
            
            if (!result.Success)
            {
                _logService.LogError($"Network configuration failed: {result.ErrorMessage}");
                throw new Exception($"Network configuration failed: {result.ErrorMessage}");
            }

            // Process script output
            ProcessPowerShellOutput(result.Output);
        }

        /// <summary>
        /// Configures additional storage for a VM
        /// </summary>
        /// <param name="vmName">Name of the VM</param>
        /// <param name="additionalDisks">Additional disks to add</param>
        /// <returns>Task representing the asynchronous operation</returns>
        private async Task ConfigureAdditionalStorageAsync(string vmName, List<DiskConfiguration> additionalDisks)
        {
            _logService.LogInfo($"Configuring additional storage for VM {vmName}");
            _progressService.ReportProgress(70, "Configuring additional storage...");

            var diskPaths = additionalDisks.Select(d => d.Path).ToArray();
            var diskSizes = additionalDisks.Select(d => d.SizeGB).ToArray();

            var scriptParams = new Dictionary<string, object>
            {
                { "VMName", vmName },
                { "AdditionalVHDPaths", diskPaths },
                { "AdditionalVHDSizesGB", diskSizes },
                { "UseDynamicDisks", true }
            };

            var result = await _executor.ExecuteScriptAsync("ConfigureStorage", scriptParams);
            
            if (!result.Success)
            {
                _logService.LogError($"Storage configuration failed: {result.ErrorMessage}");
                throw new Exception($"Storage configuration failed: {result.ErrorMessage}");
            }

            // Process script output
            ProcessPowerShellOutput(result.Output);
        }

        /// <summary>
        /// Tracks the progress of VM creation
        /// </summary>
        /// <param name="vmName">Name of the VM to track</param>
        /// <returns>Task representing the asynchronous operation</returns>
        private async Task TrackVMCreationProgressAsync(string vmName)
        {
            _logService.LogInfo($"Tracking progress for VM {vmName}");

            var scriptParams = new Dictionary<string, object>
            {
                { "VMName", vmName },
                { "ReportIntervalSeconds", 5 },
                { "Timeout", 1800 } // 30 minutes timeout
            };

            var result = await _executor.ExecuteScriptAsync("TrackProgress", scriptParams);
            
            if (!result.Success)
            {
                _logService.LogWarning($"VM progress tracking failed: {result.ErrorMessage}");
                // Don't throw an exception here as this is non-critical
            }

            // Process script output
            ProcessPowerShellOutput(result.Output);
        }

        /// <summary>
        /// Processes PowerShell output for status updates and results
        /// </summary>
        /// <param name="output">Collection of PowerShell output objects</param>
        private void ProcessPowerShellOutput(IEnumerable<PSObject> output)
        {
            if (output == null) return;

            foreach (var item in output)
            {
                // Skip null items
                if (item == null) continue;

                // Get object type
                var type = item.Properties["Type"]?.Value?.ToString();

                if (string.IsNullOrEmpty(type)) continue;

                switch (type)
                {
                    case "StatusUpdate":
                        // Process status update
                        var percentComplete = item.Properties["PercentComplete"]?.Value;
                        var statusMessage = item.Properties["StatusMessage"]?.Value?.ToString();
                        
                        if (percentComplete != null && statusMessage != null)
                        {
                            int percent = Convert.ToInt32(percentComplete);
                            _progressService.ReportProgress(percent, statusMessage);
                            _logService.LogInfo($"Progress: {percent}% - {statusMessage}");
                        }
                        break;

                    case "Result":
                        // Process result
                        var success = item.Properties["Success"]?.Value;
                        var message = item.Properties["Message"]?.Value?.ToString();
                        
                        if (success != null && message != null)
                        {
                            bool isSuccess = Convert.ToBoolean(success);
                            if (isSuccess)
                            {
                                _logService.LogInfo(message);
                            }
                            else
                            {
                                _logService.LogError(message);
                            }
                        }
                        break;

                    case "Error":
                        // Process error
                        var errorMessage = item.Properties["Message"]?.Value?.ToString();
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            _logService.LogError(errorMessage);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Finds a result object in PowerShell output
        /// </summary>
        /// <param name="output">Collection of PowerShell output objects</param>
        /// <returns>The result object or null if not found</returns>
        private PSObject FindScriptResult(IEnumerable<PSObject> output)
        {
            if (output == null) return null;

            return output.FirstOrDefault(item => 
                item?.Properties["Type"]?.Value?.ToString() == "Result" && 
                item.Properties["Success"]?.Value != null &&
                Convert.ToBoolean(item.Properties["Success"].Value) == true);
        }
    }
}

namespace HyperVCreator.Core.Models
{
    /// <summary>
    /// Parameters for VM creation
    /// </summary>
    public class VMCreationParameters
    {
        /// <summary>
        /// Name of the VM
        /// </summary>
        public string VMName { get; set; }
        
        /// <summary>
        /// Number of virtual CPUs
        /// </summary>
        public int CPUCount { get; set; }
        
        /// <summary>
        /// Memory in GB
        /// </summary>
        public int MemoryGB { get; set; }
        
        /// <summary>
        /// Storage in GB
        /// </summary>
        public int StorageGB { get; set; }
        
        /// <summary>
        /// Virtual switch name
        /// </summary>
        public string VirtualSwitch { get; set; }
        
        /// <summary>
        /// Path for the VHD file (optional)
        /// </summary>
        public string VHDPath { get; set; }
        
        /// <summary>
        /// VM generation (1 or 2)
        /// </summary>
        public int Generation { get; set; } = 2;
        
        /// <summary>
        /// Network configuration
        /// </summary>
        public NetworkConfiguration NetworkConfiguration { get; set; }
        
        /// <summary>
        /// Additional disks to add
        /// </summary>
        public List<DiskConfiguration> AdditionalDisks { get; set; }
    }

    /// <summary>
    /// Network configuration for a VM
    /// </summary>
    public class NetworkConfiguration
    {
        /// <summary>
        /// Virtual switch name
        /// </summary>
        public string VirtualSwitch { get; set; }
        
        /// <summary>
        /// Whether to use static IP
        /// </summary>
        public bool StaticIP { get; set; }
        
        /// <summary>
        /// IP address for static IP configuration
        /// </summary>
        public string IPAddress { get; set; }
        
        /// <summary>
        /// Subnet mask for static IP configuration
        /// </summary>
        public string SubnetMask { get; set; }
        
        /// <summary>
        /// Default gateway for static IP configuration
        /// </summary>
        public string DefaultGateway { get; set; }
        
        /// <summary>
        /// DNS servers for static IP configuration
        /// </summary>
        public string[] DNSServers { get; set; }
        
        /// <summary>
        /// Whether to enable VLAN
        /// </summary>
        public bool EnableVLAN { get; set; }
        
        /// <summary>
        /// VLAN ID if VLAN is enabled
        /// </summary>
        public int VLANId { get; set; }
    }

    /// <summary>
    /// Disk configuration for a VM
    /// </summary>
    public class DiskConfiguration
    {
        /// <summary>
        /// Path for the VHD file
        /// </summary>
        public string Path { get; set; }
        
        /// <summary>
        /// Size in GB
        /// </summary>
        public int SizeGB { get; set; }
    }

    /// <summary>
    /// Result of an operation
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Error message if the operation failed
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Warning message if there were non-critical issues
        /// </summary>
        public string WarningMessage { get; set; }
    }

    /// <summary>
    /// Interface for progress reporting
    /// </summary>
    public interface IProgressService
    {
        /// <summary>
        /// Reports progress
        /// </summary>
        /// <param name="percentComplete">Percentage of completion (0-100)</param>
        /// <param name="message">Status message</param>
        void ReportProgress(int percentComplete, string message);
    }

    /// <summary>
    /// Interface for logging
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Logs a debug message
        /// </summary>
        /// <param name="message">The message to log</param>
        void LogDebug(string message);
        
        /// <summary>
        /// Logs an info message
        /// </summary>
        /// <param name="message">The message to log</param>
        void LogInfo(string message);
        
        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message">The message to log</param>
        void LogWarning(string message);
        
        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message">The message to log</param>
        void LogError(string message);
    }
} 
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HyperVCreator.Core.PowerShell;
using HyperVCreator.Core.Models;

namespace HyperVCreator.Core.Services
{
    /// <summary>
    /// Service for creating virtual machines
    /// </summary>
    public class VMCreationService : IDisposable
    {
        private readonly PowerShell.PowerShellService _powerShellService;
        private readonly TemplateService _templateService;
        private readonly TemplateConverter _templateConverter;
        private readonly string _scriptPath;
        
        /// <summary>
        /// Initializes a new instance of the VMCreationService
        /// </summary>
        /// <param name="scriptPath">Optional path to PowerShell scripts</param>
        /// <param name="templatePath">Optional path to templates</param>
        public VMCreationService(string scriptPath = null, string templatePath = null)
        {
            _scriptPath = scriptPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts");
            _powerShellService = new PowerShell.PowerShellService(1, 5, _scriptPath);
            _templateService = new TemplateService(templatePath);
            _templateConverter = new TemplateConverter();
        }
        
        /// <summary>
        /// Creates a VM based on a template
        /// </summary>
        /// <param name="template">The template to use for VM creation</param>
        /// <param name="progress">Optional progress reporter</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>The result of the VM creation operation</returns>
        public async Task<VMCreationResult> CreateVMAsync(VMTemplate template, IProgress<VMCreationProgress> progress = null, CancellationToken cancellationToken = default)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }
            
            var result = new VMCreationResult
            {
                TemplateName = template.TemplateName,
                ServerRole = template.ServerRole,
                StartTime = DateTime.Now
            };
            
            try
            {
                // Report starting
                ReportProgress(progress, 0, "Starting VM creation...", result);
                
                // Convert template to PowerShell parameters
                var parameters = _templateConverter.ConvertToPowerShellParameters(template);
                
                // Determine which script to execute based on the server role
                string scriptPath = GetScriptPathForRole(template.ServerRole);
                
                // Create a progress handler to forward status updates
                var psProgress = new Progress<PowerShellStatusUpdate>(update =>
                {
                    ReportProgress(progress, update.PercentComplete, update.Message, result);
                });
                
                // Execute the PowerShell script
                var psResult = await _powerShellService.ExecuteScriptAsync(scriptPath, parameters, cancellationToken);
                
                // Update the result based on PowerShell execution
                result.Success = psResult.Success == true;
                result.Message = psResult.Message;
                result.HasErrors = psResult.HasErrors;
                
                // Add error details if there were errors
                if (psResult.HasErrors)
                {
                    foreach (var error in psResult.Errors)
                    {
                        result.ErrorMessages.Add(error.Exception.Message);
                    }
                }
                
                // Finalize the result
                result.CompletionTime = DateTime.Now;
                result.Duration = result.CompletionTime - result.StartTime;
                
                // Report completion
                ReportProgress(progress, 100, result.Success ? "VM creation completed successfully." : "VM creation failed.", result);
                
                return result;
            }
            catch (Exception ex)
            {
                // Handle exceptions during VM creation
                result.Success = false;
                result.HasErrors = true;
                result.Message = $"Error creating VM: {ex.Message}";
                result.ErrorMessages.Add(ex.Message);
                result.CompletionTime = DateTime.Now;
                result.Duration = result.CompletionTime - result.StartTime;
                result.Exception = ex;
                
                // Report error
                ReportProgress(progress, 100, $"VM creation failed: {ex.Message}", result);
                
                return result;
            }
        }
        
        /// <summary>
        /// Creates a VM based on a template loaded from a file
        /// </summary>
        /// <param name="templatePath">The path to the template file</param>
        /// <param name="progress">Optional progress reporter</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>The result of the VM creation operation</returns>
        public async Task<VMCreationResult> CreateVMFromTemplateFileAsync(string templatePath, IProgress<VMCreationProgress> progress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(templatePath))
            {
                throw new ArgumentException("Template path is required", nameof(templatePath));
            }
            
            // Report loading template
            var initialProgress = new VMCreationProgress { PercentComplete = 0, StatusMessage = "Loading template..." };
            progress?.Report(initialProgress);
            
            try
            {
                // Load the template
                var template = await _templateService.LoadTemplateAsync(templatePath);
                
                // Create the VM using the loaded template
                return await CreateVMAsync(template, progress, cancellationToken);
            }
            catch (Exception ex)
            {
                // Handle template loading errors
                var result = new VMCreationResult
                {
                    Success = false,
                    HasErrors = true,
                    Message = $"Error loading template: {ex.Message}",
                    StartTime = DateTime.Now,
                    CompletionTime = DateTime.Now
                };
                
                result.ErrorMessages.Add(ex.Message);
                result.Duration = result.CompletionTime - result.StartTime;
                result.Exception = ex;
                
                // Report error
                ReportProgress(progress, 100, $"Template loading failed: {ex.Message}", result);
                
                return result;
            }
        }
        
        /// <summary>
        /// Creates a VM for a specific server role using a default or provided configuration
        /// </summary>
        /// <param name="serverRole">The server role to create</param>
        /// <param name="configuration">Optional configuration parameters</param>
        /// <param name="progress">Optional progress reporter</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>The result of the VM creation operation</returns>
        public async Task<VMCreationResult> CreateVMForRoleAsync(string serverRole, Dictionary<string, object> configuration = null, IProgress<VMCreationProgress> progress = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(serverRole))
            {
                throw new ArgumentException("Server role is required", nameof(serverRole));
            }
            
            // Create a template based on the server role and configuration
            var template = _templateConverter.ConvertFromPowerShellParameters(configuration ?? new Dictionary<string, object>(), serverRole);
            
            // Create the VM using the generated template
            return await CreateVMAsync(template, progress, cancellationToken);
        }
        
        /// <summary>
        /// Gets the appropriate script path for a server role
        /// </summary>
        /// <param name="serverRole">The server role</param>
        /// <returns>The path to the PowerShell script for the role</returns>
        private string GetScriptPathForRole(string serverRole)
        {
            switch (serverRole)
            {
                case "DomainController":
                    return Path.Combine("RoleConfiguration", "DomainController.ps1");
                    
                case "FileServer":
                    return Path.Combine("RoleConfiguration", "FileServer.ps1");
                    
                case "WebServer":
                    return Path.Combine("RoleConfiguration", "WebServer.ps1");
                    
                case "SQLServer":
                    return Path.Combine("RoleConfiguration", "SQLServer.ps1");
                    
                case "DHCPServer":
                    return Path.Combine("RoleConfiguration", "DHCPServer.ps1");
                    
                case "DNSServer":
                    return Path.Combine("RoleConfiguration", "DNSServer.ps1");
                    
                case "RDSH":
                    return Path.Combine("RoleConfiguration", "RDSH.ps1");
                    
                case "CustomVM":
                default:
                    return Path.Combine("RoleConfiguration", "CustomVM.ps1");
            }
        }
        
        /// <summary>
        /// Reports progress for VM creation
        /// </summary>
        /// <param name="progress">The progress reporter</param>
        /// <param name="percentComplete">The percent complete value</param>
        /// <param name="message">The status message</param>
        /// <param name="result">The current result object</param>
        private void ReportProgress(IProgress<VMCreationProgress> progress, int percentComplete, string message, VMCreationResult result)
        {
            if (progress == null)
            {
                return;
            }
            
            var progressUpdate = new VMCreationProgress
            {
                PercentComplete = percentComplete,
                StatusMessage = message,
                CurrentOperation = result.CurrentOperation,
                ElapsedTime = DateTime.Now - result.StartTime
            };
            
            // Update the result with the current operation
            result.CurrentOperation = message;
            
            // Report the progress
            progress.Report(progressUpdate);
        }
        
        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            _powerShellService?.Dispose();
        }
    }
    
    /// <summary>
    /// Represents the result of a VM creation operation
    /// </summary>
    public class VMCreationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Gets or sets the template name used for creation
        /// </summary>
        public string TemplateName { get; set; }
        
        /// <summary>
        /// Gets or sets the server role
        /// </summary>
        public string ServerRole { get; set; }
        
        /// <summary>
        /// Gets or sets the result message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether there were errors
        /// </summary>
        public bool HasErrors { get; set; }
        
        /// <summary>
        /// Gets the collection of error messages
        /// </summary>
        public List<string> ErrorMessages { get; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the start time of the operation
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// Gets or sets the completion time of the operation
        /// </summary>
        public DateTime CompletionTime { get; set; }
        
        /// <summary>
        /// Gets or sets the duration of the operation
        /// </summary>
        public TimeSpan Duration { get; set; }
        
        /// <summary>
        /// Gets or sets the current operation being performed
        /// </summary>
        public string CurrentOperation { get; set; }
        
        /// <summary>
        /// Gets or sets the exception that occurred during the operation, if any
        /// </summary>
        public Exception Exception { get; set; }
    }
    
    /// <summary>
    /// Represents progress information for VM creation
    /// </summary>
    public class VMCreationProgress
    {
        /// <summary>
        /// Gets or sets the percent complete value (0-100)
        /// </summary>
        public int PercentComplete { get; set; }
        
        /// <summary>
        /// Gets or sets the status message
        /// </summary>
        public string StatusMessage { get; set; }
        
        /// <summary>
        /// Gets or sets the current operation being performed
        /// </summary>
        public string CurrentOperation { get; set; }
        
        /// <summary>
        /// Gets or sets the elapsed time since the operation started
        /// </summary>
        public TimeSpan ElapsedTime { get; set; }
    }
} 
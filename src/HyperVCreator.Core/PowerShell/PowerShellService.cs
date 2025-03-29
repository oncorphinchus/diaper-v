using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;
using HyperVCreator.Core.Services;

namespace HyperVCreator.Core.PowerShell
{
    /// <summary>
    /// Service for executing PowerShell scripts
    /// </summary>
    public class PowerShellService
    {
        private readonly RunspacePool _runspacePool;
        private readonly string _scriptsPath;
        
        /// <summary>
        /// Initializes a new instance of the PowerShellService
        /// </summary>
        /// <param name="minRunspaces">The minimum number of runspaces in the pool</param>
        /// <param name="maxRunspaces">The maximum number of runspaces in the pool</param>
        /// <param name="scriptsPath">The path to the scripts directory</param>
        public PowerShellService(int minRunspaces = 1, int maxRunspaces = 5, string scriptsPath = null)
        {
            // Initialize the runspace pool
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.ExecutionPolicy = ExecutionPolicy.Unrestricted;
            
            _runspacePool = RunspaceFactory.CreateRunspacePool(initialSessionState);
            _runspacePool.SetMinRunspaces(minRunspaces);
            _runspacePool.SetMaxRunspaces(maxRunspaces);
            _runspacePool.Open();
            
            // Set the scripts path
            _scriptsPath = scriptsPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts");
        }
        
        /// <summary>
        /// Executes a PowerShell script with parameters and returns the results
        /// </summary>
        /// <param name="scriptPath">The path to the script file</param>
        /// <param name="parameters">The parameters to pass to the script</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A task that represents the asynchronous operation and contains the script output</returns>
        public async Task<PowerShellResult> ExecuteScriptAsync(string scriptPath, Dictionary<string, object> parameters = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(scriptPath))
            {
                throw new ArgumentException("Script path is required", nameof(scriptPath));
            }
            
            // Resolve the script path
            string fullScriptPath = Path.IsPathRooted(scriptPath)
                ? scriptPath
                : Path.Combine(_scriptsPath, scriptPath);
                
            if (!File.Exists(fullScriptPath))
            {
                throw new FileNotFoundException($"Script file not found: {fullScriptPath}");
            }
            
            // Read the script content
            string scriptContent = await File.ReadAllTextAsync(fullScriptPath, cancellationToken);
            
            return await ExecuteScriptContentAsync(scriptContent, parameters, cancellationToken);
        }
        
        /// <summary>
        /// Executes PowerShell script content with parameters and returns the results
        /// </summary>
        /// <param name="scriptContent">The script content to execute</param>
        /// <param name="parameters">The parameters to pass to the script</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A task that represents the asynchronous operation and contains the script output</returns>
        public async Task<PowerShellResult> ExecuteScriptContentAsync(string scriptContent, Dictionary<string, object> parameters = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(scriptContent))
            {
                throw new ArgumentException("Script content is required", nameof(scriptContent));
            }
            
            // Create a PowerShell instance from the pool
            using var ps = System.Management.Automation.PowerShell.Create();
            ps.RunspacePool = _runspacePool;
            
            // Add the script content
            ps.AddScript(scriptContent);
            
            // Add parameters if provided
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    ps.AddParameter(param.Key, param.Value);
                }
            }
            
            // Create result object
            var result = new PowerShellResult();
            
            try
            {
                // Execute the script
                var asyncResult = ps.BeginInvoke();
                
                // Wait for completion with cancellation support
                while (!asyncResult.IsCompleted)
                {
                    await Task.Delay(100, cancellationToken);
                    
                    if (cancellationToken.IsCancellationRequested)
                    {
                        ps.Stop();
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                
                // Get the output
                var output = ps.EndInvoke(asyncResult);
                result.Output.AddRange(output);
                
                // Check for errors
                if (ps.Streams.Error.Count > 0)
                {
                    result.HasErrors = true;
                    foreach (var error in ps.Streams.Error)
                    {
                        result.Errors.Add(error);
                    }
                }
                
                // Add warning messages
                foreach (var warning in ps.Streams.Warning)
                {
                    result.Warnings.Add(warning);
                }
                
                // Add information messages
                foreach (var info in ps.Streams.Information)
                {
                    result.Information.Add(info);
                }
                
                // Check for specific result objects (e.g., status updates, results)
                foreach (var item in output)
                {
                    if (item is PSObject psObject)
                    {
                        // Try to extract Type property to identify result objects
                        var typeProperty = psObject.Properties["Type"];
                        if (typeProperty != null)
                        {
                            var type = typeProperty.Value as string;
                            
                            switch (type)
                            {
                                case "StatusUpdate":
                                    // Extract status update info
                                    var statusMessage = GetPropertyValue<string>(psObject, "StatusMessage", "");
                                    var percentComplete = GetPropertyValue<int>(psObject, "PercentComplete", 0);
                                    result.StatusUpdates.Add(new PowerShellStatusUpdate 
                                    { 
                                        Message = statusMessage, 
                                        PercentComplete = percentComplete 
                                    });
                                    break;
                                    
                                case "Result":
                                    // Extract operation result
                                    var success = GetPropertyValue<bool>(psObject, "Success", false);
                                    var message = GetPropertyValue<string>(psObject, "Message", "");
                                    result.Success = success;
                                    result.Message = message;
                                    break;
                            }
                        }
                    }
                }
                
                // Set default success state based on errors if not explicitly set
                if (!result.Errors.Count.Equals(0) && result.Success == null)
                {
                    result.Success = false;
                }
                else if (result.Success == null)
                {
                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                result.HasErrors = true;
                result.Success = false;
                result.Message = $"Error executing script: {ex.Message}";
                result.Exception = ex;
            }
            
            return result;
        }
        
        /// <summary>
        /// Creates a VM based on a template
        /// </summary>
        /// <param name="template">The VM template to use</param>
        /// <param name="progress">An optional progress reporter</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A task that represents the asynchronous operation and contains the creation result</returns>
        public async Task<PowerShellResult> CreateVMFromTemplateAsync(VMTemplate template, IProgress<PowerShellStatusUpdate> progress = null, CancellationToken cancellationToken = default)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }
            
            // Convert template to script parameters
            var converter = new TemplateConverter();
            var parameters = converter.ConvertToPowerShellParameters(template);
            
            // Determine which script to execute based on the server role
            string scriptPath;
            switch (template.ServerRole)
            {
                case "DomainController":
                    scriptPath = Path.Combine("RoleConfiguration", "DomainController.ps1");
                    break;
                    
                case "FileServer":
                    scriptPath = Path.Combine("RoleConfiguration", "FileServer.ps1");
                    break;
                    
                case "WebServer":
                    scriptPath = Path.Combine("RoleConfiguration", "WebServer.ps1");
                    break;
                    
                case "SQLServer":
                    scriptPath = Path.Combine("RoleConfiguration", "SQLServer.ps1");
                    break;
                    
                case "DHCPServer":
                    scriptPath = Path.Combine("RoleConfiguration", "DHCPServer.ps1");
                    break;
                    
                case "DNSServer":
                    scriptPath = Path.Combine("RoleConfiguration", "DNSServer.ps1");
                    break;
                    
                case "RDSH":
                    scriptPath = Path.Combine("RoleConfiguration", "RDSH.ps1");
                    break;
                    
                case "CustomVM":
                default:
                    scriptPath = Path.Combine("RoleConfiguration", "CustomVM.ps1");
                    break;
            }
            
            // Create a progress handler
            var progressHandler = new Progress<PowerShellStatusUpdate>(update =>
            {
                progress?.Report(update);
            });
            
            // Execute the script and track progress
            var result = await ExecuteScriptAsync(scriptPath, parameters, cancellationToken);
            
            // Report progress updates
            foreach (var update in result.StatusUpdates)
            {
                progressHandler.Report(update);
            }
            
            return result;
        }
        
        /// <summary>
        /// Gets a property value from a PSObject, with type conversion
        /// </summary>
        /// <typeparam name="T">The expected property type</typeparam>
        /// <param name="psObject">The PSObject to get the property from</param>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="defaultValue">The default value to return if property not found or conversion fails</param>
        /// <returns>The property value or default</returns>
        private T GetPropertyValue<T>(PSObject psObject, string propertyName, T defaultValue)
        {
            if (psObject == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return defaultValue;
            }
            
            var property = psObject.Properties[propertyName];
            if (property == null || property.Value == null)
            {
                return defaultValue;
            }
            
            try
            {
                if (property.Value is T typedValue)
                {
                    return typedValue;
                }
                
                return (T)Convert.ChangeType(property.Value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        
        /// <summary>
        /// Disposes resources used by the service
        /// </summary>
        public void Dispose()
        {
            _runspacePool?.Dispose();
        }
    }
    
    /// <summary>
    /// Represents the result of a PowerShell script execution
    /// </summary>
    public class PowerShellResult
    {
        /// <summary>
        /// Gets the collection of output objects
        /// </summary>
        public List<PSObject> Output { get; } = new List<PSObject>();
        
        /// <summary>
        /// Gets the collection of error records
        /// </summary>
        public List<ErrorRecord> Errors { get; } = new List<ErrorRecord>();
        
        /// <summary>
        /// Gets the collection of warning records
        /// </summary>
        public List<WarningRecord> Warnings { get; } = new List<WarningRecord>();
        
        /// <summary>
        /// Gets the collection of information records
        /// </summary>
        public List<InformationRecord> Information { get; } = new List<InformationRecord>();
        
        /// <summary>
        /// Gets the collection of status updates
        /// </summary>
        public List<PowerShellStatusUpdate> StatusUpdates { get; } = new List<PowerShellStatusUpdate>();
        
        /// <summary>
        /// Gets or sets a value indicating whether the script execution has errors
        /// </summary>
        public bool HasErrors { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful
        /// </summary>
        public bool? Success { get; set; }
        
        /// <summary>
        /// Gets or sets the result message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Gets or sets the exception that occurred during execution, if any
        /// </summary>
        public Exception Exception { get; set; }
        
        /// <summary>
        /// Gets a value indicating whether the result includes any status updates
        /// </summary>
        public bool HasStatusUpdates => StatusUpdates.Count > 0;
        
        /// <summary>
        /// Gets the first status update message, or null if none
        /// </summary>
        public string FirstStatusMessage => HasStatusUpdates ? StatusUpdates[0].Message : null;
        
        /// <summary>
        /// Gets the last status update message, or null if none
        /// </summary>
        public string LastStatusMessage => HasStatusUpdates ? StatusUpdates[StatusUpdates.Count - 1].Message : null;
        
        /// <summary>
        /// Gets the last percent complete value, or 0 if none
        /// </summary>
        public int LastPercentComplete => HasStatusUpdates ? StatusUpdates[StatusUpdates.Count - 1].PercentComplete : 0;
    }
    
    /// <summary>
    /// Represents a status update from a PowerShell script
    /// </summary>
    public class PowerShellStatusUpdate
    {
        /// <summary>
        /// Gets or sets the status message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Gets or sets the percent complete value (0-100)
        /// </summary>
        public int PercentComplete { get; set; }
    }
} 
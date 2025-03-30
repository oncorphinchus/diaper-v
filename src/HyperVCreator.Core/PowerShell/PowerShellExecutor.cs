using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell.Commands;
using System.Text;
using System.Threading.Tasks;

namespace HyperVCreator.Core.PowerShell
{
    /// <summary>
    /// Result of PowerShell script execution
    /// </summary>
    public class PowerShellExecutorResult
    {
        /// <summary>
        /// Whether the execution was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Error message if any
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Collection of PowerShell output objects
        /// </summary>
        public Collection<PSObject> Output { get; set; }
        
        /// <summary>
        /// Collection of error records
        /// </summary>
        public Collection<ErrorRecord> Errors { get; set; }
        
        /// <summary>
        /// The exception if any
        /// </summary>
        public Exception Exception { get; set; }
    }

    /// <summary>
    /// Handles execution of PowerShell scripts
    /// </summary>
    public class PowerShellExecutor
    {
        private readonly string _scriptsBasePath;
        private readonly Dictionary<string, string> _scriptPaths;
        private readonly InitialSessionState _initialSessionState;

        /// <summary>
        /// Initializes a new instance of the PowerShellExecutor
        /// </summary>
        /// <param name="scriptsBasePath">Base path for PowerShell scripts</param>
        public PowerShellExecutor(string scriptsBasePath)
        {
            _scriptsBasePath = scriptsBasePath;
            _scriptPaths = new Dictionary<string, string>();
            _initialSessionState = InitialSessionState.CreateDefault();
            
            // Allow script execution
            _initialSessionState.AuthorizationManager = new System.Management.Automation.AuthorizationManager("Microsoft.PowerShell");
            
            // Cache script paths
            CacheScriptPaths();
        }

        /// <summary>
        /// Caches paths to all PowerShell scripts
        /// </summary>
        private void CacheScriptPaths()
        {
            if (!Directory.Exists(_scriptsBasePath))
            {
                throw new DirectoryNotFoundException($"Scripts directory not found: {_scriptsBasePath}");
            }

            // Find all PS1 files in the scripts directory and subdirectories
            var scriptFiles = Directory.GetFiles(_scriptsBasePath, "*.ps1", SearchOption.AllDirectories);
            
            foreach (var file in scriptFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                _scriptPaths[fileName] = file;
            }
        }

        /// <summary>
        /// Executes a PowerShell script by name
        /// </summary>
        /// <param name="scriptName">Name of the script (without .ps1 extension)</param>
        /// <param name="parameters">Script parameters</param>
        /// <returns>Result of the script execution</returns>
        public PowerShellExecutorResult ExecuteScript(string scriptName, Dictionary<string, object> parameters)
        {
            // Resolve script path
            if (!_scriptPaths.TryGetValue(scriptName, out var scriptPath))
            {
                return new PowerShellExecutorResult
                {
                    Success = false,
                    ErrorMessage = $"Script not found: {scriptName}"
                };
            }

            return ExecuteScriptFromPath(scriptPath, parameters);
        }

        /// <summary>
        /// Executes a PowerShell script from a specific path
        /// </summary>
        /// <param name="scriptPath">Full path to the script</param>
        /// <param name="parameters">Script parameters</param>
        /// <returns>Result of the script execution</returns>
        public PowerShellExecutorResult ExecuteScriptFromPath(string scriptPath, Dictionary<string, object> parameters)
        {
            var result = new PowerShellExecutorResult
            {
                Success = false,
                Output = new Collection<PSObject>(),
                Errors = new Collection<ErrorRecord>()
            };
            
            try
            {
                // Verify script exists
                if (!File.Exists(scriptPath))
                {
                    result.ErrorMessage = $"Script file not found: {scriptPath}";
                    return result;
                }
                
                // Create a runspace
                using (var runspace = RunspaceFactory.CreateRunspace(_initialSessionState))
                {
                    runspace.Open();
                    
                    // Create a PowerShell instance
                    using (var powerShell = System.Management.Automation.PowerShell.Create())
                    {
                        powerShell.Runspace = runspace;
                        
                        // Add script content
                        powerShell.AddScript(File.ReadAllText(scriptPath));
                        
                        // Add parameters
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                powerShell.AddParameter(param.Key, param.Value);
                            }
                        }
                        
                        // Execute the script
                        result.Output = powerShell.Invoke();
                        
                        // Convert PSDataCollection to Collection for Errors
                        result.Errors = new Collection<ErrorRecord>();
                        foreach (var error in powerShell.Streams.Error)
                        {
                            result.Errors.Add(error);
                        }
                        
                        // Check for errors
                        if (powerShell.HadErrors)
                        {
                            var errorBuilder = new StringBuilder();
                            foreach (var error in powerShell.Streams.Error)
                            {
                                errorBuilder.AppendLine(error.Exception.Message);
                            }
                            result.ErrorMessage = errorBuilder.ToString();
                        }
                        else
                        {
                            result.Success = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Exception executing script: {ex.Message}";
                result.Exception = ex;
            }
            
            return result;
        }

        /// <summary>
        /// Executes a PowerShell script asynchronously
        /// </summary>
        /// <param name="scriptName">Name of the script (without .ps1 extension)</param>
        /// <param name="parameters">Script parameters</param>
        /// <returns>Task with the result of script execution</returns>
        public Task<PowerShellExecutorResult> ExecuteScriptAsync(string scriptName, Dictionary<string, object> parameters)
        {
            return Task.Run(() => ExecuteScript(scriptName, parameters));
        }

        /// <summary>
        /// Executes a PowerShell script from a path asynchronously
        /// </summary>
        /// <param name="scriptPath">Full path to the script</param>
        /// <param name="parameters">Script parameters</param>
        /// <returns>Task with the result of script execution</returns>
        public Task<PowerShellExecutorResult> ExecuteScriptFromPathAsync(string scriptPath, Dictionary<string, object> parameters)
        {
            return Task.Run(() => ExecuteScriptFromPath(scriptPath, parameters));
        }
    }
} 
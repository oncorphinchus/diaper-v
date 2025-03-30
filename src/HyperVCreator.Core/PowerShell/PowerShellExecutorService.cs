using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace HyperVCreator.Core.PowerShell
{
    /// <summary>
    /// Service that wraps the PowerShellExecutor to provide progress reporting and standardized result handling
    /// </summary>
    public class PowerShellExecutorService
    {
        private readonly PowerShellExecutor _executor;
        
        /// <summary>
        /// Initializes a new instance of the PowerShellExecutorService
        /// </summary>
        /// <param name="scriptsBasePath">Base path for PowerShell scripts</param>
        public PowerShellExecutorService(string scriptsBasePath)
        {
            _executor = new PowerShellExecutor(scriptsBasePath);
        }
        
        /// <summary>
        /// Executes a PowerShell script with progress reporting
        /// </summary>
        /// <param name="scriptName">Name of the script to execute</param>
        /// <param name="parameters">Parameters for the script</param>
        /// <param name="progress">Optional progress reporter</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A standardized PowerShell result</returns>
        public async Task<PowerShellResult> ExecuteScriptAsync(
            string scriptName, 
            Dictionary<string, object> parameters = null, 
            IProgress<PowerShellStatusUpdate> progress = null, 
            CancellationToken cancellationToken = default)
        {
            // Report initial progress
            progress?.Report(new PowerShellStatusUpdate { Message = $"Starting script: {scriptName}", PercentComplete = 0 });
            
            try
            {
                // Execute the script using the executor
                var executorResult = await _executor.ExecuteScriptAsync(scriptName, parameters);
                
                // Convert the result
                var result = ConvertToStandardResult(executorResult);
                
                // Report final progress
                progress?.Report(new PowerShellStatusUpdate 
                { 
                    Message = result.Success == true ? "Script completed successfully" : "Script failed", 
                    PercentComplete = 100 
                });
                
                return result;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                var result = new PowerShellResult
                {
                    HasErrors = true,
                    Success = false,
                    Message = $"Error executing script {scriptName}: {ex.Message}",
                    Exception = ex
                };
                
                // Report error progress
                progress?.Report(new PowerShellStatusUpdate { Message = $"Error: {ex.Message}", PercentComplete = 100 });
                
                return result;
            }
        }
        
        /// <summary>
        /// Executes a PowerShell script from a specific path with progress reporting
        /// </summary>
        /// <param name="scriptPath">Full path to the script</param>
        /// <param name="parameters">Parameters for the script</param>
        /// <param name="progress">Optional progress reporter</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A standardized PowerShell result</returns>
        public async Task<PowerShellResult> ExecuteScriptFromPathAsync(
            string scriptPath, 
            Dictionary<string, object> parameters = null, 
            IProgress<PowerShellStatusUpdate> progress = null, 
            CancellationToken cancellationToken = default)
        {
            // Report initial progress
            progress?.Report(new PowerShellStatusUpdate { Message = $"Starting script from path: {scriptPath}", PercentComplete = 0 });
            
            try
            {
                // Execute the script using the executor
                var executorResult = await _executor.ExecuteScriptFromPathAsync(scriptPath, parameters);
                
                // Convert the result
                var result = ConvertToStandardResult(executorResult);
                
                // Report final progress
                progress?.Report(new PowerShellStatusUpdate 
                { 
                    Message = result.Success == true ? "Script completed successfully" : "Script failed", 
                    PercentComplete = 100 
                });
                
                return result;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                var result = new PowerShellResult
                {
                    HasErrors = true,
                    Success = false,
                    Message = $"Error executing script from path {scriptPath}: {ex.Message}",
                    Exception = ex
                };
                
                // Report error progress
                progress?.Report(new PowerShellStatusUpdate { Message = $"Error: {ex.Message}", PercentComplete = 100 });
                
                return result;
            }
        }
        
        /// <summary>
        /// Converts PowerShellExecutorResult to PowerShellResult
        /// </summary>
        private PowerShellResult ConvertToStandardResult(PowerShellExecutorResult executorResult)
        {
            var result = new PowerShellResult
            {
                Success = executorResult.Success,
                Message = executorResult.ErrorMessage,
                Exception = executorResult.Exception,
                HasErrors = executorResult.Errors?.Count > 0
            };
            
            // Copy output
            if (executorResult.Output != null)
            {
                foreach (var item in executorResult.Output)
                {
                    result.Output.Add(item);
                }
            }
            
            // Copy errors
            if (executorResult.Errors != null)
            {
                foreach (var error in executorResult.Errors)
                {
                    result.Errors.Add(error);
                }
            }
            
            return result;
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;

namespace HyperVCreator.Core.Services
{
    /// <summary>
    /// Provides VM creation and management services using PowerShell
    /// </summary>
    public class PowerShellService : IDisposable
    {
        private Runspace _runspace;
        private bool _disposed;
        
        public event EventHandler<string> OutputReceived;
        public event EventHandler<string> ErrorReceived;
        
        /// <summary>
        /// Initializes a new instance of the PowerShellService
        /// </summary>
        public PowerShellService()
        {
            InitializeRunspace();
        }
        
        private void InitializeRunspace()
        {
            try
            {
                // Create and open a single runspace
                var initialSessionState = InitialSessionState.CreateDefault();
                _runspace = RunspaceFactory.CreateRunspace(initialSessionState);
                _runspace.Open();
            }
            catch (Exception ex)
            {
                OnErrorReceived($"Error initializing PowerShell runspace: {ex.Message}");
            }
        }
        
        public async Task<List<string>> ExecuteScriptAsync(string script, CancellationToken cancellationToken = default)
        {
            var results = new List<string>();
            
            try
            {
                using (var powershell = System.Management.Automation.PowerShell.Create())
                {
                    // Configure PowerShell instance
                    powershell.Runspace = _runspace;
                    
                    // Register event handlers
                    powershell.Streams.Error.DataAdded += (sender, e) =>
                    {
                        ErrorRecord error = ((PSDataCollection<ErrorRecord>)sender)[e.Index];
                        string errorMessage = error.Exception.Message;
                        OnErrorReceived(errorMessage);
                        results.Add($"Error: {errorMessage}");
                    };
                    
                    powershell.Streams.Warning.DataAdded += (sender, e) =>
                    {
                        var warning = ((PSDataCollection<WarningRecord>)sender)[e.Index];
                        OnOutputReceived($"Warning: {warning.Message}");
                        results.Add($"Warning: {warning.Message}");
                    };
                    
                    powershell.Streams.Information.DataAdded += (sender, e) =>
                    {
                        var info = ((PSDataCollection<InformationRecord>)sender)[e.Index];
                        OnOutputReceived($"Info: {info.MessageData}");
                        results.Add($"Info: {info.MessageData}");
                    };
                    
                    // Add script to PowerShell instance
                    powershell.AddScript(script);
                    
                    // Execute script
                    using var cancelRegistration = cancellationToken.Register(() => 
                    {
                        OnOutputReceived("Script execution cancelled.");
                        powershell.Stop();
                    });
                    
                    var output = await Task.Run(() => powershell.Invoke(), cancellationToken);
                    
                    // Process results
                    foreach (var item in output)
                    {
                        string result = item?.ToString() ?? "null";
                        OnOutputReceived(result);
                        results.Add(result);
                    }
                    
                    // Clear streams to avoid memory leaks
                    powershell.Streams.ClearStreams();
                }
            }
            catch (OperationCanceledException)
            {
                OnOutputReceived("Script execution cancelled.");
                results.Add("Script execution cancelled.");
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error executing PowerShell script: {ex.Message}";
                OnErrorReceived(errorMessage);
                results.Add($"Error: {errorMessage}");
                
                if (ex.InnerException != null)
                {
                    string innerErrorMessage = $"Inner exception: {ex.InnerException.Message}";
                    OnErrorReceived(innerErrorMessage);
                    results.Add($"Error: {innerErrorMessage}");
                }
            }
            
            return results;
        }
        
        protected virtual void OnOutputReceived(string output)
        {
            OutputReceived?.Invoke(this, output);
        }
        
        protected virtual void OnErrorReceived(string error)
        {
            ErrorReceived?.Invoke(this, error);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            
            if (disposing)
            {
                // Dispose managed resources
                if (_runspace != null)
                {
                    if (_runspace.RunspaceStateInfo.State == RunspaceState.Opened)
                    {
                        _runspace.Close();
                    }
                    _runspace.Dispose();
                    _runspace = null;
                }
            }
            
            _disposed = true;
        }
        
        ~PowerShellService()
        {
            Dispose(false);
        }
    }
} 
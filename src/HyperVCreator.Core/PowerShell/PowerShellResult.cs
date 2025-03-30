using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace HyperVCreator.Core.PowerShell
{
    /// <summary>
    /// Represents the result of a PowerShell script execution
    /// </summary>
    public class PowerShellResult
    {
        /// <summary>
        /// Gets the output of the PowerShell script
        /// </summary>
        public List<PSObject> Output { get; } = new List<PSObject>();
        
        /// <summary>
        /// Gets the errors that occurred during script execution
        /// </summary>
        public List<ErrorRecord> Errors { get; } = new List<ErrorRecord>();
        
        /// <summary>
        /// Gets the warning messages that occurred during script execution
        /// </summary>
        public List<WarningRecord> Warnings { get; } = new List<WarningRecord>();
        
        /// <summary>
        /// Gets the information messages that occurred during script execution
        /// </summary>
        public List<InformationRecord> Information { get; } = new List<InformationRecord>();
        
        /// <summary>
        /// Gets the status updates from the script execution
        /// </summary>
        public List<PowerShellStatusUpdate> StatusUpdates { get; } = new List<PowerShellStatusUpdate>();
        
        /// <summary>
        /// Gets or sets a value indicating whether the script execution had errors
        /// </summary>
        public bool HasErrors { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the script execution was successful
        /// </summary>
        public bool? Success { get; set; }
        
        /// <summary>
        /// Gets or sets the message from the script execution
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Gets or sets the exception that occurred during script execution
        /// </summary>
        public Exception Exception { get; set; }
        
        /// <summary>
        /// Gets a value indicating whether the script execution had status updates
        /// </summary>
        public bool HasStatusUpdates => StatusUpdates.Count > 0;
        
        /// <summary>
        /// Gets the first status message
        /// </summary>
        public string FirstStatusMessage => HasStatusUpdates ? StatusUpdates[0].Message : null;
        
        /// <summary>
        /// Gets the last status message
        /// </summary>
        public string LastStatusMessage => HasStatusUpdates ? StatusUpdates[StatusUpdates.Count - 1].Message : null;
        
        /// <summary>
        /// Gets the last percentage complete value
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
        /// Gets or sets the percentage complete value
        /// </summary>
        public int PercentComplete { get; set; }
    }
} 
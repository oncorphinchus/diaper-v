using System;

namespace HyperVCreator.Core.Models
{
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
} 
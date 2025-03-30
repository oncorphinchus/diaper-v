using System;

namespace HyperVCreator.Core.Services
{
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
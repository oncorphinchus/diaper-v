using System;

namespace HyperVCreator.Core.Services
{
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
} 
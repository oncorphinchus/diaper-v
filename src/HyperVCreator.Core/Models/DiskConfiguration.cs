using System;

namespace HyperVCreator.Core.Models
{
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
} 
using System;
using System.Collections.Generic;

namespace HyperVCreator.Core.Models
{
    /// <summary>
    /// Parameters for VM creation
    /// </summary>
    public class VMCreationParameters
    {
        /// <summary>
        /// Name of the VM
        /// </summary>
        public string VMName { get; set; }
        
        /// <summary>
        /// Number of virtual CPUs
        /// </summary>
        public int CPUCount { get; set; }
        
        /// <summary>
        /// Memory in GB
        /// </summary>
        public int MemoryGB { get; set; }
        
        /// <summary>
        /// Storage in GB
        /// </summary>
        public int StorageGB { get; set; }
        
        /// <summary>
        /// Virtual switch name
        /// </summary>
        public string VirtualSwitch { get; set; }
        
        /// <summary>
        /// Path for the VHD file (optional)
        /// </summary>
        public string VHDPath { get; set; }
        
        /// <summary>
        /// VM generation (1 or 2)
        /// </summary>
        public int Generation { get; set; } = 2;
        
        /// <summary>
        /// Network configuration
        /// </summary>
        public NetworkConfiguration NetworkConfiguration { get; set; }
        
        /// <summary>
        /// Additional disks to add
        /// </summary>
        public List<DiskConfiguration> AdditionalDisks { get; set; }
    }
} 
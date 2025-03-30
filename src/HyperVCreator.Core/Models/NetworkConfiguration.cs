using System;
using System.Collections.Generic;

namespace HyperVCreator.Core.Models
{
    /// <summary>
    /// Network configuration for a VM
    /// </summary>
    public class NetworkConfiguration
    {
        /// <summary>
        /// Virtual switch name
        /// </summary>
        public string VirtualSwitch { get; set; }
        
        /// <summary>
        /// Whether to use static IP
        /// </summary>
        public bool StaticIP { get; set; }
        
        /// <summary>
        /// IP address for static IP configuration
        /// </summary>
        public string IPAddress { get; set; }
        
        /// <summary>
        /// Subnet mask for static IP configuration
        /// </summary>
        public string SubnetMask { get; set; }
        
        /// <summary>
        /// Default gateway for static IP configuration
        /// </summary>
        public string DefaultGateway { get; set; }
        
        /// <summary>
        /// DNS servers for static IP configuration
        /// </summary>
        public string[] DNSServers { get; set; }
        
        /// <summary>
        /// Whether to enable VLAN
        /// </summary>
        public bool EnableVLAN { get; set; }
        
        /// <summary>
        /// VLAN ID if VLAN is enabled
        /// </summary>
        public int VLANId { get; set; }
    }
} 
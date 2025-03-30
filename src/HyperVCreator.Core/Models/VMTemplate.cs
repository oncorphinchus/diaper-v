using System;
using System.Collections.Generic;

namespace HyperVCreator.Core.Models
{
    /// <summary>
    /// Represents a virtual machine template
    /// </summary>
    public class VMTemplate
    {
        /// <summary>
        /// Gets or sets the template name
        /// </summary>
        public string TemplateName { get; set; }
        
        /// <summary>
        /// Gets or sets the server role
        /// </summary>
        public string ServerRole { get; set; }
        
        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the hardware configuration
        /// </summary>
        public HardwareConfig HardwareConfiguration { get; set; }
        
        /// <summary>
        /// Gets or sets the network configuration
        /// </summary>
        public NetworkConfig NetworkConfiguration { get; set; }
        
        /// <summary>
        /// Gets or sets the OS configuration
        /// </summary>
        public OSConfig OSConfiguration { get; set; }
        
        /// <summary>
        /// Gets or sets the additional configuration
        /// </summary>
        public AdditionalConfig AdditionalConfiguration { get; set; }
        
        /// <summary>
        /// Gets or sets the metadata
        /// </summary>
        public TemplateMetadata Metadata { get; set; }

        /// <summary>
        /// Initializes a new instance of the VMTemplate class
        /// </summary>
        public VMTemplate()
        {
            HardwareConfiguration = new HardwareConfig();
            NetworkConfiguration = new NetworkConfig();
            OSConfiguration = new OSConfig();
            AdditionalConfiguration = new AdditionalConfig();
            Metadata = new TemplateMetadata();
        }
    }
} 
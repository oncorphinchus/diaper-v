using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HyperVCreator.Core.Services;
using HyperVCreator.Core.Models;

namespace HyperVCreator.App.ViewModels
{
    public class TemplateEditorViewModel : ViewModelBase
    {
        private readonly TemplateService _templateService;
        private VMTemplate _template;
        private string _originalName;
        private bool _isNew;
        private bool _isSaving;
        private string _errorMessage;
        private List<string> _availableServerRoles;
        private List<string> _availableOSVersions;

        public TemplateEditorViewModel(TemplateService templateService, VMTemplate template = null)
        {
            _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            
            // Create a new template if null or use the provided one
            IsNew = template == null;
            Template = template ?? CreateNewTemplate();
            _originalName = Template.TemplateName;
            
            // Initialize available values
            AvailableServerRoles = new List<string>
            {
                "DomainController",
                "FileServer",
                "WebServer",
                "SQLServer",
                "RDSH",
                "DHCPServer",
                "DNSServer",
                "CustomVM"
            };
            
            AvailableOSVersions = new List<string>
            {
                "Windows Server 2016 Standard",
                "Windows Server 2016 Datacenter",
                "Windows Server 2019 Standard",
                "Windows Server 2019 Datacenter",
                "Windows Server 2022 Standard",
                "Windows Server 2022 Datacenter"
            };
            
            // Initialize commands
            SaveCommand = new RelayCommand(async _ => await SaveTemplateAsync(), _ => CanSave);
            CancelCommand = new RelayCommand(_ => Cancel());
            AddDiskCommand = new RelayCommand(_ => AddAdditionalDisk());
            RemoveDiskCommand = new RelayCommand(_ => RemoveAdditionalDisk(), _ => SelectedDiskIndex >= 0);
            AddTagCommand = new RelayCommand(_ => AddTag());
            RemoveTagCommand = new RelayCommand(_ => RemoveTag(), _ => SelectedTagIndex >= 0);
        }
        
        #region Properties
        
        public VMTemplate Template
        {
            get => _template;
            set
            {
                _template = value;
                OnPropertyChanged(nameof(Template));
                
                // Also notify changes to all template properties
                OnPropertyChanged(nameof(TemplateName));
                OnPropertyChanged(nameof(ServerRole));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(ProcessorCount));
                OnPropertyChanged(nameof(MemoryGB));
                OnPropertyChanged(nameof(StorageGB));
                OnPropertyChanged(nameof(Generation));
                OnPropertyChanged(nameof(EnableSecureBoot));
                OnPropertyChanged(nameof(AdditionalDisks));
                OnPropertyChanged(nameof(VirtualSwitch));
                OnPropertyChanged(nameof(DynamicIP));
                OnPropertyChanged(nameof(IPAddress));
                OnPropertyChanged(nameof(SubnetMask));
                OnPropertyChanged(nameof(DefaultGateway));
                OnPropertyChanged(nameof(DNSServers));
                OnPropertyChanged(nameof(OSVersion));
                OnPropertyChanged(nameof(ProductKey));
                OnPropertyChanged(nameof(AdminPassword));
                OnPropertyChanged(nameof(ComputerName));
                OnPropertyChanged(nameof(AutoStartVM));
                OnPropertyChanged(nameof(UseUnattendXML));
                OnPropertyChanged(nameof(EnableRDP));
                OnPropertyChanged(nameof(EnablePSRemoting));
                OnPropertyChanged(nameof(Tags));
            }
        }
        
        public bool IsNew
        {
            get => _isNew;
            set => SetProperty(ref _isNew, value);
        }
        
        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }
        
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        
        // List properties for dropdowns and list selections
        public List<string> AvailableServerRoles
        {
            get => _availableServerRoles;
            set => SetProperty(ref _availableServerRoles, value);
        }
        
        public List<string> AvailableOSVersions
        {
            get => _availableOSVersions;
            set => SetProperty(ref _availableOSVersions, value);
        }
        
        // Template properties
        public string TemplateName
        {
            get => Template.TemplateName;
            set
            {
                Template.TemplateName = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }
        
        public string ServerRole
        {
            get => Template.ServerRole;
            set
            {
                Template.ServerRole = value;
                OnPropertyChanged();
            }
        }
        
        public string Description
        {
            get => Template.Description;
            set
            {
                Template.Description = value;
                OnPropertyChanged();
            }
        }
        
        // Hardware configuration
        public int ProcessorCount
        {
            get => Template.HardwareConfiguration.ProcessorCount;
            set
            {
                Template.HardwareConfiguration.ProcessorCount = value;
                OnPropertyChanged();
            }
        }
        
        public int MemoryGB
        {
            get => Template.HardwareConfiguration.MemoryGB;
            set
            {
                Template.HardwareConfiguration.MemoryGB = value;
                OnPropertyChanged();
            }
        }
        
        public int StorageGB
        {
            get => Template.HardwareConfiguration.StorageGB;
            set
            {
                Template.HardwareConfiguration.StorageGB = value;
                OnPropertyChanged();
            }
        }
        
        public int Generation
        {
            get => Template.HardwareConfiguration.Generation;
            set
            {
                Template.HardwareConfiguration.Generation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EnableSecureBoot)); // Generation affects secure boot
            }
        }
        
        public bool EnableSecureBoot
        {
            get => Template.HardwareConfiguration.EnableSecureBoot;
            set
            {
                Template.HardwareConfiguration.EnableSecureBoot = value;
                OnPropertyChanged();
            }
        }
        
        public List<DiskConfig> AdditionalDisks
        {
            get => Template.HardwareConfiguration.AdditionalDisks;
            set
            {
                Template.HardwareConfiguration.AdditionalDisks = value;
                OnPropertyChanged();
            }
        }
        
        private int _selectedDiskIndex = -1;
        public int SelectedDiskIndex
        {
            get => _selectedDiskIndex;
            set
            {
                SetProperty(ref _selectedDiskIndex, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }
        
        // Network configuration
        public string VirtualSwitch
        {
            get => Template.NetworkConfiguration.VirtualSwitch;
            set
            {
                Template.NetworkConfiguration.VirtualSwitch = value;
                OnPropertyChanged();
            }
        }
        
        public bool DynamicIP
        {
            get => Template.NetworkConfiguration.DynamicIP;
            set
            {
                Template.NetworkConfiguration.DynamicIP = value;
                OnPropertyChanged();
                // Update related static IP fields
                OnPropertyChanged(nameof(IPAddress));
                OnPropertyChanged(nameof(SubnetMask));
                OnPropertyChanged(nameof(DefaultGateway));
                OnPropertyChanged(nameof(DNSServers));
            }
        }
        
        public string IPAddress
        {
            get => Template.NetworkConfiguration.StaticIPConfiguration.IPAddress;
            set
            {
                Template.NetworkConfiguration.StaticIPConfiguration.IPAddress = value;
                OnPropertyChanged();
            }
        }
        
        public string SubnetMask
        {
            get => Template.NetworkConfiguration.StaticIPConfiguration.SubnetMask;
            set
            {
                Template.NetworkConfiguration.StaticIPConfiguration.SubnetMask = value;
                OnPropertyChanged();
            }
        }
        
        public string DefaultGateway
        {
            get => Template.NetworkConfiguration.StaticIPConfiguration.DefaultGateway;
            set
            {
                Template.NetworkConfiguration.StaticIPConfiguration.DefaultGateway = value;
                OnPropertyChanged();
            }
        }
        
        public List<string> DNSServers
        {
            get => Template.NetworkConfiguration.StaticIPConfiguration.DNSServers;
            set
            {
                Template.NetworkConfiguration.StaticIPConfiguration.DNSServers = value;
                OnPropertyChanged();
            }
        }
        
        // OS configuration
        public string OSVersion
        {
            get => Template.OSConfiguration.OSVersion;
            set
            {
                Template.OSConfiguration.OSVersion = value;
                OnPropertyChanged();
            }
        }
        
        public string ProductKey
        {
            get => Template.OSConfiguration.ProductKey;
            set
            {
                Template.OSConfiguration.ProductKey = value;
                OnPropertyChanged();
            }
        }
        
        public string AdminPassword
        {
            get => Template.OSConfiguration.AdminPassword;
            set
            {
                Template.OSConfiguration.AdminPassword = value;
                OnPropertyChanged();
            }
        }
        
        public string ComputerName
        {
            get => Template.OSConfiguration.ComputerName;
            set
            {
                Template.OSConfiguration.ComputerName = value;
                OnPropertyChanged();
            }
        }
        
        // Additional configuration
        public bool AutoStartVM
        {
            get => Template.AdditionalConfiguration.AutoStartVM;
            set
            {
                Template.AdditionalConfiguration.AutoStartVM = value;
                OnPropertyChanged();
            }
        }
        
        public bool UseUnattendXML
        {
            get => Template.OSConfiguration.UseUnattendXML;
            set
            {
                Template.OSConfiguration.UseUnattendXML = value;
                OnPropertyChanged();
            }
        }
        
        public bool EnableRDP
        {
            get => Template.AdditionalConfiguration.EnableRDP;
            set
            {
                Template.AdditionalConfiguration.EnableRDP = value;
                OnPropertyChanged();
            }
        }
        
        public bool EnablePSRemoting
        {
            get => Template.AdditionalConfiguration.EnablePSRemoting;
            set
            {
                Template.AdditionalConfiguration.EnablePSRemoting = value;
                OnPropertyChanged();
            }
        }
        
        // Metadata
        public List<string> Tags
        {
            get => Template.Metadata.Tags;
            set
            {
                Template.Metadata.Tags = value;
                OnPropertyChanged();
            }
        }
        
        private int _selectedTagIndex = -1;
        public int SelectedTagIndex
        {
            get => _selectedTagIndex;
            set
            {
                SetProperty(ref _selectedTagIndex, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }
        
        private string _newTagText;
        public string NewTagText
        {
            get => _newTagText;
            set => SetProperty(ref _newTagText, value);
        }
        
        #endregion
        
        #region Commands
        
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddDiskCommand { get; }
        public ICommand RemoveDiskCommand { get; }
        public ICommand AddTagCommand { get; }
        public ICommand RemoveTagCommand { get; }
        
        #endregion
        
        #region Command Implementations
        
        private async Task SaveTemplateAsync()
        {
            if (!ValidateTemplate())
                return;
            
            IsSaving = true;
            ErrorMessage = string.Empty;
            
            try
            {
                // Update metadata
                Template.Metadata.LastModifiedDate = DateTime.Now;
                if (IsNew)
                {
                    Template.Metadata.CreatedDate = DateTime.Now;
                    Template.Metadata.Author = Environment.UserName;
                }
                
                // Save the template
                await _templateService.SaveTemplateAsync(Template);
                
                // Navigate back to template list
                // TODO: Implement navigation service to navigate between views
                // This will be implemented with the navigation service in the Base Application phase
                // MessengerInstance.Send(new NavigateToTemplateListMessage());
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving template: {ex.Message}";
            }
            finally
            {
                IsSaving = false;
            }
        }
        
        private void Cancel()
        {
            // Navigate back to template list without saving
            // MessengerInstance.Send(new NavigateToTemplateListMessage());
        }
        
        private void AddAdditionalDisk()
        {
            var newDisk = new DiskConfig
            {
                SizeGB = 50,
                Letter = "E",
                Label = "Data"
            };
            
            Template.HardwareConfiguration.AdditionalDisks.Add(newDisk);
            OnPropertyChanged(nameof(AdditionalDisks));
        }
        
        private void RemoveAdditionalDisk()
        {
            if (SelectedDiskIndex >= 0 && SelectedDiskIndex < Template.HardwareConfiguration.AdditionalDisks.Count)
            {
                Template.HardwareConfiguration.AdditionalDisks.RemoveAt(SelectedDiskIndex);
                OnPropertyChanged(nameof(AdditionalDisks));
                SelectedDiskIndex = -1;
            }
        }
        
        private void AddTag()
        {
            if (!string.IsNullOrWhiteSpace(NewTagText) && !Template.Metadata.Tags.Contains(NewTagText))
            {
                Template.Metadata.Tags.Add(NewTagText);
                OnPropertyChanged(nameof(Tags));
                NewTagText = string.Empty;
            }
        }
        
        private void RemoveTag()
        {
            if (SelectedTagIndex >= 0 && SelectedTagIndex < Template.Metadata.Tags.Count)
            {
                Template.Metadata.Tags.RemoveAt(SelectedTagIndex);
                OnPropertyChanged(nameof(Tags));
                SelectedTagIndex = -1;
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private VMTemplate CreateNewTemplate()
        {
            return new VMTemplate
            {
                TemplateName = "New Template",
                ServerRole = "CustomVM",
                Description = "Custom VM Template",
                
                HardwareConfiguration = new HardwareConfig
                {
                    ProcessorCount = 2,
                    MemoryGB = 4,
                    StorageGB = 80,
                    Generation = 2,
                    EnableSecureBoot = true,
                    AdditionalDisks = new List<DiskConfig>()
                },
                
                NetworkConfiguration = new NetworkConfig
                {
                    VirtualSwitch = "Default Switch",
                    DynamicIP = true
                },
                
                OSConfiguration = new OSConfig
                {
                    OSVersion = "Windows Server 2022 Standard",
                    UseUnattendXML = true
                },
                
                AdditionalConfiguration = new AdditionalConfig
                {
                    AutoStartVM = false,
                    EnableRDP = true,
                    EnablePSRemoting = true
                },
                
                Metadata = new TemplateMetadata
                {
                    Author = Environment.UserName,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    Tags = new List<string>()
                }
            };
        }
        
        private bool ValidateTemplate()
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(Template.TemplateName))
            {
                ErrorMessage = "Template name is required";
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(Template.ServerRole))
            {
                ErrorMessage = "Server role is required";
                return false;
            }
            
            // Validate hardware configuration
            if (Template.HardwareConfiguration.ProcessorCount <= 0)
            {
                ErrorMessage = "Processor count must be greater than 0";
                return false;
            }
            
            if (Template.HardwareConfiguration.MemoryGB <= 0)
            {
                ErrorMessage = "Memory must be greater than 0 GB";
                return false;
            }
            
            if (Template.HardwareConfiguration.StorageGB < 20)
            {
                ErrorMessage = "Primary storage must be at least 20 GB";
                return false;
            }
            
            // Validate static IP configuration
            if (!Template.NetworkConfiguration.DynamicIP)
            {
                if (string.IsNullOrWhiteSpace(Template.NetworkConfiguration.StaticIPConfiguration.IPAddress))
                {
                    ErrorMessage = "IP Address is required when using static IP";
                    return false;
                }
                
                if (string.IsNullOrWhiteSpace(Template.NetworkConfiguration.StaticIPConfiguration.SubnetMask))
                {
                    ErrorMessage = "Subnet Mask is required when using static IP";
                    return false;
                }
                
                if (string.IsNullOrWhiteSpace(Template.NetworkConfiguration.StaticIPConfiguration.DefaultGateway))
                {
                    ErrorMessage = "Default Gateway is required when using static IP";
                    return false;
                }
            }
            
            return true;
        }
        
        public bool CanSave => !string.IsNullOrWhiteSpace(Template.TemplateName) && 
                               !string.IsNullOrWhiteSpace(Template.ServerRole) && 
                               !IsSaving;
                               
        #endregion
    }
} 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HyperVCreator.App.Commands;
using HyperVCreator.App.Services;
using HyperVCreator.Core.Services;

namespace HyperVCreator.App.ViewModels.ServerRoles.RDSH
{
    public class RDSHConfigViewModel : INotifyPropertyChanged
    {
        private readonly IHyperVService _hyperVService;
        private readonly INavigationService _navigationService;
        private readonly IConfigurationService _configurationService;

        #region Properties

        // Basic VM Settings
        private string _vmName = "RDSH01";
        public string VMName
        {
            get => _vmName;
            set
            {
                if (_vmName != value)
                {
                    _vmName = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<int> CPUOptions { get; } = new ObservableCollection<int> { 1, 2, 4, 8 };
        
        private int _selectedCPU = 4;
        public int SelectedCPU
        {
            get => _selectedCPU;
            set
            {
                if (_selectedCPU != value)
                {
                    _selectedCPU = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<int> MemoryOptions { get; } = new ObservableCollection<int> { 2, 4, 8, 16, 32 };
        
        private int _selectedMemory = 8;
        public int SelectedMemory
        {
            get => _selectedMemory;
            set
            {
                if (_selectedMemory != value)
                {
                    _selectedMemory = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private bool _dynamicMemory = true;
        public bool DynamicMemory
        {
            get => _dynamicMemory;
            set
            {
                if (_dynamicMemory != value)
                {
                    _dynamicMemory = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> VirtualSwitches { get; } = new ObservableCollection<string>();
        
        private string _selectedVirtualSwitch;
        public string SelectedVirtualSwitch
        {
            get => _selectedVirtualSwitch;
            set
            {
                if (_selectedVirtualSwitch != value)
                {
                    _selectedVirtualSwitch = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private int _diskSizeGB = 100;
        public int DiskSizeGB
        {
            get => _diskSizeGB;
            set
            {
                if (_diskSizeGB != value)
                {
                    _diskSizeGB = value;
                    OnPropertyChanged();
                }
            }
        }
        
        // RDSH Specific Settings
        private string _connectionBroker = "";
        public string ConnectionBroker
        {
            get => _connectionBroker;
            set
            {
                if (_connectionBroker != value)
                {
                    _connectionBroker = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _collectionName = "Desktop Collection";
        public string CollectionName
        {
            get => _collectionName;
            set
            {
                if (_collectionName != value)
                {
                    _collectionName = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private bool _useHA = false;
        public bool UseHA
        {
            get => _useHA;
            set
            {
                if (_useHA != value)
                {
                    _useHA = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _licenseServer = "";
        public string LicenseServer
        {
            get => _licenseServer;
            set
            {
                if (_licenseServer != value)
                {
                    _licenseServer = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public ObservableCollection<string> LicenseModes { get; } = new ObservableCollection<string> { "PerUser", "PerDevice" };
        
        private string _selectedLicenseMode = "PerUser";
        public string SelectedLicenseMode
        {
            get => _selectedLicenseMode;
            set
            {
                if (_selectedLicenseMode != value)
                {
                    _selectedLicenseMode = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private int _maxConnections = 50;
        public int MaxConnections
        {
            get => _maxConnections;
            set
            {
                if (_maxConnections != value)
                {
                    _maxConnections = value;
                    OnPropertyChanged();
                }
            }
        }
        
        // Network Configuration
        private bool _isDHCP = true;
        public bool IsDHCP
        {
            get => _isDHCP;
            set
            {
                if (_isDHCP != value)
                {
                    _isDHCP = value;
                    OnPropertyChanged();
                    
                    if (value)
                    {
                        IsStaticIP = false;
                    }
                }
            }
        }
        
        private bool _isStaticIP = false;
        public bool IsStaticIP
        {
            get => _isStaticIP;
            set
            {
                if (_isStaticIP != value)
                {
                    _isStaticIP = value;
                    OnPropertyChanged();
                    
                    if (value)
                    {
                        IsDHCP = false;
                    }
                }
            }
        }
        
        private string _ipAddress = "192.168.1.20";
        public string IPAddress
        {
            get => _ipAddress;
            set
            {
                if (_ipAddress != value)
                {
                    _ipAddress = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _subnetMask = "255.255.255.0";
        public string SubnetMask
        {
            get => _subnetMask;
            set
            {
                if (_subnetMask != value)
                {
                    _subnetMask = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _defaultGateway = "192.168.1.1";
        public string DefaultGateway
        {
            get => _defaultGateway;
            set
            {
                if (_defaultGateway != value)
                {
                    _defaultGateway = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _preferredDNS = "192.168.1.10";
        public string PreferredDNS
        {
            get => _preferredDNS;
            set
            {
                if (_preferredDNS != value)
                {
                    _preferredDNS = value;
                    OnPropertyChanged();
                }
            }
        }
        
        // Advanced Options
        private bool _installOffice = false;
        public bool InstallOffice
        {
            get => _installOffice;
            set
            {
                if (_installOffice != value)
                {
                    _installOffice = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private bool _installRSAT = true;
        public bool InstallRSAT
        {
            get => _installRSAT;
            set
            {
                if (_installRSAT != value)
                {
                    _installRSAT = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private bool _enableRemoteFX = false;
        public bool EnableRemoteFX
        {
            get => _enableRemoteFX;
            set
            {
                if (_enableRemoteFX != value)
                {
                    _enableRemoteFX = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private bool _enableUserProfileDisks = true;
        public bool EnableUserProfileDisks
        {
            get => _enableUserProfileDisks;
            set
            {
                if (_enableUserProfileDisks != value)
                {
                    _enableUserProfileDisks = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _userProfileDiskPath = @"\\FileServer\UserProfiles";
        public string UserProfileDiskPath
        {
            get => _userProfileDiskPath;
            set
            {
                if (_userProfileDiskPath != value)
                {
                    _userProfileDiskPath = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private int _userProfileDiskMaxSizeGB = 10;
        public int UserProfileDiskMaxSizeGB
        {
            get => _userProfileDiskMaxSizeGB;
            set
            {
                if (_userProfileDiskMaxSizeGB != value)
                {
                    _userProfileDiskMaxSizeGB = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Commands

        private ICommand _createVMCommand;
        public ICommand CreateVMCommand => _createVMCommand ??= new RelayCommand(
            execute: _ => ExecuteCreateVMCommand(),
            canExecute: _ => CanExecuteCreateVMCommand()
        );

        private ICommand _cancelCommand;
        public ICommand CancelCommand => _cancelCommand ??= new RelayCommand(
            execute: _ => ExecuteCancelCommand(),
            canExecute: _ => true
        );

        #endregion

        public RDSHConfigViewModel()
        {
            // Use dependency injection in actual implementation
            _hyperVService = null;
            _navigationService = null;
            _configurationService = null;

            // In a real implementation, fetch this from Hyper-V
            VirtualSwitches.Add("Default Switch");
            VirtualSwitches.Add("Internal Network");
            VirtualSwitches.Add("External Network");
            SelectedVirtualSwitch = VirtualSwitches.FirstOrDefault();

            // Implement constructor logic here
            LoadSettings();
        }

        private void LoadSettings()
        {
            // In a real implementation, this would load saved settings
            // from the configuration service
        }

        private bool CanExecuteCreateVMCommand()
        {
            // Validate VM Name
            if (string.IsNullOrWhiteSpace(VMName))
                return false;
                
            // Validate Hardware Configuration
            if (SelectedCPU <= 0 || SelectedMemory <= 0 || DiskSizeGB <= 0)
                return false;
                
            // Validate Virtual Switch
            if (string.IsNullOrWhiteSpace(SelectedVirtualSwitch))
                return false;
                
            // Validate License Mode if License Server is specified
            if (!string.IsNullOrWhiteSpace(LicenseServer) && string.IsNullOrWhiteSpace(SelectedLicenseMode))
                return false;
                
            // Validate Static IP Configuration
            if (IsStaticIP)
            {
                if (string.IsNullOrWhiteSpace(IPAddress) || string.IsNullOrWhiteSpace(SubnetMask) || 
                    string.IsNullOrWhiteSpace(DefaultGateway) || string.IsNullOrWhiteSpace(PreferredDNS))
                    return false;
            }
            
            // Validate User Profile Disk settings
            if (EnableUserProfileDisks)
            {
                if (string.IsNullOrWhiteSpace(UserProfileDiskPath) || UserProfileDiskMaxSizeGB <= 0)
                    return false;
            }
            
            return true;
        }

        private void ExecuteCreateVMCommand()
        {
            // Here you would implement the logic to create the VM
            // 1. Build the VM configuration
            var config = BuildVMConfiguration();
            
            // 2. Call the service to create the VM
            // Example: _hyperVService.CreateVM(config);
            
            // 3. Navigate to a progress or confirmation page
            // Example: _navigationService.NavigateToProgressPage(vm);
        }
        
        private object BuildVMConfiguration()
        {
            // In a real implementation, this would create a proper VM configuration object
            // based on the user's selections
            
            // This is just a placeholder - in a real app you would return a proper VM configuration
            var config = new
            {
                VMName = VMName,
                CPUCount = SelectedCPU,
                MemoryGB = SelectedMemory,
                DynamicMemory = DynamicMemory,
                VirtualSwitch = SelectedVirtualSwitch,
                DiskSizeGB = DiskSizeGB,
                
                ServerRole = "RDSH",
                RDSHConfig = new
                {
                    ConnectionBroker = ConnectionBroker,
                    CollectionName = CollectionName,
                    UseHA = UseHA,
                    LicenseServer = LicenseServer,
                    LicenseMode = SelectedLicenseMode,
                    MaxConnections = MaxConnections
                },
                
                NetworkConfig = new
                {
                    UseDHCP = IsDHCP,
                    IPAddress = IPAddress,
                    SubnetMask = SubnetMask,
                    DefaultGateway = DefaultGateway,
                    PreferredDNS = PreferredDNS
                },
                
                AdvancedOptions = new
                {
                    InstallOffice = InstallOffice,
                    InstallRSAT = InstallRSAT,
                    EnableRemoteFX = EnableRemoteFX,
                    EnableUserProfileDisks = EnableUserProfileDisks,
                    UserProfileDiskPath = UserProfileDiskPath,
                    UserProfileDiskMaxSizeGB = UserProfileDiskMaxSizeGB
                }
            };

            return config;
        }

        private void ExecuteCancelCommand()
        {
            // Navigate back to main page
            if (_navigationService != null)
            {
                _navigationService.NavigateToMainPage();
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
} 
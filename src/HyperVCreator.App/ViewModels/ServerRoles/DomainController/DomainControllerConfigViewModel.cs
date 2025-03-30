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

namespace HyperVCreator.App.ViewModels.ServerRoles.DomainController
{
    public class DomainControllerConfigViewModel : INotifyPropertyChanged
    {
        private readonly IHyperVService _hyperVService;
        private readonly INavigationService _navigationService;
        private readonly IConfigurationService _configurationService;

        #region Properties

        // Basic VM Settings
        private string _vmName = "DC01";
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
        
        private int _selectedCPU = 2;
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

        public ObservableCollection<int> MemoryOptions { get; } = new ObservableCollection<int> { 2, 4, 8, 16 };
        
        private int _selectedMemory = 4;
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

        public ObservableCollection<string> VirtualSwitches { get; private set; } = new ObservableCollection<string>();
        
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

        // Domain Settings
        private string _domainName = "contoso.local";
        public string DomainName
        {
            get => _domainName;
            set
            {
                if (_domainName != value)
                {
                    _domainName = value;
                    
                    // Auto-populate NetBIOS name based on first part of domain
                    if (!string.IsNullOrEmpty(value) && value.Contains('.'))
                    {
                        NetBIOSName = value.Split('.')[0].ToUpper();
                    }
                    
                    OnPropertyChanged();
                }
            }
        }

        private string _netBIOSName = "CONTOSO";
        public string NetBIOSName
        {
            get => _netBIOSName;
            set
            {
                if (_netBIOSName != value)
                {
                    _netBIOSName = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> ForestFunctionalLevels { get; } = new ObservableCollection<string>
        {
            "Windows Server 2016",
            "Windows Server 2019",
            "Windows Server 2022"
        };
        
        private string _selectedForestLevel = "Windows Server 2022";
        public string SelectedForestLevel
        {
            get => _selectedForestLevel;
            set
            {
                if (_selectedForestLevel != value)
                {
                    _selectedForestLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> DomainFunctionalLevels { get; } = new ObservableCollection<string>
        {
            "Windows Server 2016",
            "Windows Server 2019",
            "Windows Server 2022"
        };
        
        private string _selectedDomainLevel = "Windows Server 2022";
        public string SelectedDomainLevel
        {
            get => _selectedDomainLevel;
            set
            {
                if (_selectedDomainLevel != value)
                {
                    _selectedDomainLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        // DNS Settings
        private bool _configureDNS = true;
        public bool ConfigureDNS
        {
            get => _configureDNS;
            set
            {
                if (_configureDNS != value)
                {
                    _configureDNS = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _dnsForwarders = "8.8.8.8, 8.8.4.4";
        public string DNSForwarders
        {
            get => _dnsForwarders;
            set
            {
                if (_dnsForwarders != value)
                {
                    _dnsForwarders = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _createReverseLookupZone = true;
        public bool CreateReverseLookupZone
        {
            get => _createReverseLookupZone;
            set
            {
                if (_createReverseLookupZone != value)
                {
                    _createReverseLookupZone = value;
                    OnPropertyChanged();
                }
            }
        }

        // Network Configuration
        private bool _isDHCP;
        public bool IsDHCP
        {
            get => _isDHCP;
            set
            {
                if (_isDHCP != value)
                {
                    _isDHCP = value;
                    IsStaticIP = !value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsStaticIP));
                }
            }
        }

        private bool _isStaticIP = true;
        public bool IsStaticIP
        {
            get => _isStaticIP;
            set
            {
                if (_isStaticIP != value)
                {
                    _isStaticIP = value;
                    IsDHCP = !value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsDHCP));
                }
            }
        }

        private string _ipAddress = "192.168.1.10";
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

        private string _preferredDNS = "127.0.0.1";
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
        private bool _createOUStructure = true;
        public bool CreateOUStructure
        {
            get => _createOUStructure;
            set
            {
                if (_createOUStructure != value)
                {
                    _createOUStructure = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _configureGPOs = true;
        public bool ConfigureGPOs
        {
            get => _configureGPOs;
            set
            {
                if (_configureGPOs != value)
                {
                    _configureGPOs = value;
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

        public DomainControllerConfigViewModel()
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

        public DomainControllerConfigViewModel(
            IHyperVService hyperVService,
            INavigationService navigationService,
            IConfigurationService configurationService)
        {
            _hyperVService = hyperVService ?? throw new ArgumentNullException(nameof(hyperVService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));

            // Initialize virtual switches
            LoadVirtualSwitches();
            
            // Load saved settings
            LoadSettings();
        }

        private void LoadVirtualSwitches()
        {
            // In a real implementation, this would fetch switches from Hyper-V
            if (_hyperVService != null)
            {
                var switches = _hyperVService.GetVirtualSwitches();
                VirtualSwitches = new ObservableCollection<string>(switches);
                SelectedVirtualSwitch = VirtualSwitches.FirstOrDefault();
            }
            else
            {
                // Demo data
                VirtualSwitches.Add("Default Switch");
                VirtualSwitches.Add("Internal Network");
                VirtualSwitches.Add("External Network");
                SelectedVirtualSwitch = VirtualSwitches.FirstOrDefault();
            }
        }

        private void LoadSettings()
        {
            // Load settings from configuration if available
            if (_configurationService != null)
            {
                // Example implementation
                // var defaultDomain = _configurationService.GetSetting("DomainController.DefaultDomain", "contoso.local");
                // DomainName = defaultDomain;
            }
        }

        private bool CanExecuteCreateVMCommand()
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(VMName))
                return false;

            if (string.IsNullOrWhiteSpace(DomainName) || !DomainName.Contains('.'))
                return false;

            if (string.IsNullOrWhiteSpace(NetBIOSName))
                return false;

            if (IsStaticIP)
            {
                if (string.IsNullOrWhiteSpace(IPAddress) || string.IsNullOrWhiteSpace(SubnetMask))
                    return false;
            }

            return true;
        }

        private void ExecuteCreateVMCommand()
        {
            // Build VM configuration
            var vmConfig = BuildVMConfiguration();

            // Create the VM
            if (_hyperVService != null)
            {
                _hyperVService.CreateVM(vmConfig);
                
                // Navigate back or to a results page
                if (_navigationService != null)
                {
                    _navigationService.NavigateToMainPage();
                }
            }
            else
            {
                // Demo/debug mode - just log the config
                Console.WriteLine($"Creating Domain Controller: {VMName}");
                Console.WriteLine($"Domain: {DomainName}, NetBIOS: {NetBIOSName}");
            }
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
                VirtualSwitch = SelectedVirtualSwitch,
                
                ServerRole = "DomainController",
                DomainName = DomainName,
                NetBIOSName = NetBIOSName,
                ForestLevel = SelectedForestLevel,
                DomainLevel = SelectedDomainLevel,
                
                NetworkConfig = new
                {
                    UseDHCP = IsDHCP,
                    IPAddress = IPAddress,
                    SubnetMask = SubnetMask,
                    DefaultGateway = DefaultGateway,
                    PreferredDNS = PreferredDNS
                },
                
                DNSConfig = new
                {
                    ConfigureDNS = ConfigureDNS,
                    Forwarders = DNSForwarders,
                    CreateReverseLookup = CreateReverseLookupZone
                },
                
                AdvancedOptions = new
                {
                    CreateOUStructure = CreateOUStructure,
                    ConfigureGPOs = ConfigureGPOs,
                    InstallRSAT = InstallRSAT
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
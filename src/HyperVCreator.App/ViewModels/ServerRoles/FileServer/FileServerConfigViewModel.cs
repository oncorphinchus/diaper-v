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

namespace HyperVCreator.App.ViewModels.ServerRoles.FileServer
{
    public class FileServerConfigViewModel : INotifyPropertyChanged
    {
        private readonly IHyperVService _hyperVService;
        private readonly INavigationService _navigationService;
        private readonly IConfigurationService _configurationService;

        #region Properties

        // Basic VM Settings
        private string _vmName = "FS01";
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

        public ObservableCollection<int> MemoryOptions { get; } = new ObservableCollection<int> { 2, 4, 8, 16, 32 };
        
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
        
        private int _systemDiskSizeGB = 120;
        public int SystemDiskSizeGB
        {
            get => _systemDiskSizeGB;
            set
            {
                if (_systemDiskSizeGB != value)
                {
                    _systemDiskSizeGB = value;
                    OnPropertyChanged();
                }
            }
        }
        
        // Data Disk Settings
        public ObservableCollection<DataDiskViewModel> DataDisks { get; } = new ObservableCollection<DataDiskViewModel>();
        
        private DataDiskViewModel _selectedDataDisk;
        public DataDiskViewModel SelectedDataDisk
        {
            get => _selectedDataDisk;
            set
            {
                if (_selectedDataDisk != value)
                {
                    _selectedDataDisk = value;
                    OnPropertyChanged();
                }
            }
        }
        
        // Share Settings
        public ObservableCollection<ShareViewModel> Shares { get; } = new ObservableCollection<ShareViewModel>();
        
        private ShareViewModel _selectedShare;
        public ShareViewModel SelectedShare
        {
            get => _selectedShare;
            set
            {
                if (_selectedShare != value)
                {
                    _selectedShare = value;
                    OnPropertyChanged();
                }
            }
        }
        
        // File Server Features
        private bool _enableFSRM = false;
        public bool EnableFSRM
        {
            get => _enableFSRM;
            set
            {
                if (_enableFSRM != value)
                {
                    _enableFSRM = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private bool _enableQuotas = true;
        public bool EnableQuotas
        {
            get => _enableQuotas;
            set
            {
                if (_enableQuotas != value)
                {
                    _enableQuotas = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private bool _enableFileScreens = false;
        public bool EnableFileScreens
        {
            get => _enableFileScreens;
            set
            {
                if (_enableFileScreens != value)
                {
                    _enableFileScreens = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private bool _enableDedup = true;
        public bool EnableDedup
        {
            get => _enableDedup;
            set
            {
                if (_enableDedup != value)
                {
                    _enableDedup = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private bool _enableShadowCopies = true;
        public bool EnableShadowCopies
        {
            get => _enableShadowCopies;
            set
            {
                if (_enableShadowCopies != value)
                {
                    _enableShadowCopies = value;
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
        
        private string _ipAddress = "192.168.1.30";
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
        
        // Domain Join
        private bool _joinDomain = true;
        public bool JoinDomain
        {
            get => _joinDomain;
            set
            {
                if (_joinDomain != value)
                {
                    _joinDomain = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _domainName = "contoso.local";
        public string DomainName
        {
            get => _domainName;
            set
            {
                if (_domainName != value)
                {
                    _domainName = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _domainOU = "OU=Servers,DC=contoso,DC=local";
        public string DomainOU
        {
            get => _domainOU;
            set
            {
                if (_domainOU != value)
                {
                    _domainOU = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _domainUsername = "";
        public string DomainUsername
        {
            get => _domainUsername;
            set
            {
                if (_domainUsername != value)
                {
                    _domainUsername = value;
                    OnPropertyChanged();
                }
            }
        }
        
        // Advanced Settings
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
        
        private bool _installBackupAgent = false;
        public bool InstallBackupAgent
        {
            get => _installBackupAgent;
            set
            {
                if (_installBackupAgent != value)
                {
                    _installBackupAgent = value;
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
        
        private ICommand _addDataDiskCommand;
        public ICommand AddDataDiskCommand => _addDataDiskCommand ??= new RelayCommand(
            execute: _ => AddDataDisk(),
            canExecute: _ => true
        );
        
        private ICommand _removeDataDiskCommand;
        public ICommand RemoveDataDiskCommand => _removeDataDiskCommand ??= new RelayCommand(
            execute: param => RemoveDataDisk(param as DataDiskViewModel),
            canExecute: param => param is DataDiskViewModel
        );
        
        private ICommand _addShareCommand;
        public ICommand AddShareCommand => _addShareCommand ??= new RelayCommand(
            execute: _ => AddShare(),
            canExecute: _ => DataDisks.Count > 0
        );
        
        private ICommand _removeShareCommand;
        public ICommand RemoveShareCommand => _removeShareCommand ??= new RelayCommand(
            execute: param => RemoveShare(param as ShareViewModel),
            canExecute: param => param is ShareViewModel
        );

        #endregion

        public FileServerConfigViewModel()
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

            // Add default data disk and share
            AddDataDisk();
            AddShare();
            
            // Implement constructor logic here
            LoadSettings();
        }

        private void LoadSettings()
        {
            // In a real implementation, this would load saved settings
            // from the configuration service
        }
        
        private void AddDataDisk()
        {
            var newDisk = new DataDiskViewModel
            {
                Size = 500,
                Letter = GetNextAvailableDriveLetter(),
                Label = $"Data{DataDisks.Count + 1}",
                FileSystem = "NTFS"
            };
            
            DataDisks.Add(newDisk);
            SelectedDataDisk = newDisk;
        }
        
        private void RemoveDataDisk(DataDiskViewModel disk)
        {
            if (disk == null) return;
            
            // Check if any shares are using this disk
            var sharesUsingDisk = Shares.Where(s => s.Path.StartsWith(disk.Letter + ":")).ToList();
            if (sharesUsingDisk.Any())
            {
                // In a real implementation, show a warning message
                // For now, just remove the shares that use this disk
                foreach (var share in sharesUsingDisk.ToList())
                {
                    Shares.Remove(share);
                }
            }
            
            DataDisks.Remove(disk);
            
            if (DataDisks.Count > 0)
            {
                SelectedDataDisk = DataDisks[0];
            }
            else
            {
                SelectedDataDisk = null;
            }
        }
        
        private string GetNextAvailableDriveLetter()
        {
            // Start with D as C is usually the system drive
            char letter = 'D';
            while (DataDisks.Any(d => d.Letter == letter.ToString()) && letter <= 'Z')
            {
                letter++;
            }
            
            return letter.ToString();
        }
        
        private void AddShare()
        {
            if (DataDisks.Count == 0) return;
            
            var diskToUse = SelectedDataDisk ?? DataDisks[0];
            var newShare = new ShareViewModel
            {
                Name = $"Share{Shares.Count + 1}",
                Path = $"{diskToUse.Letter}:\\Shares\\Share{Shares.Count + 1}",
                Description = $"Share {Shares.Count + 1}"
            };
            
            // Add default permissions
            newShare.Permissions.Add(new SharePermissionViewModel
            {
                Identity = "Everyone",
                AccessRights = "Read"
            });
            
            newShare.Permissions.Add(new SharePermissionViewModel
            {
                Identity = "Administrators",
                AccessRights = "FullControl"
            });
            
            Shares.Add(newShare);
            SelectedShare = newShare;
        }
        
        private void RemoveShare(ShareViewModel share)
        {
            if (share == null) return;
            
            Shares.Remove(share);
            
            if (Shares.Count > 0)
            {
                SelectedShare = Shares[0];
            }
            else
            {
                SelectedShare = null;
            }
        }

        private bool CanExecuteCreateVMCommand()
        {
            // Validate VM Name
            if (string.IsNullOrWhiteSpace(VMName))
                return false;
                
            // Validate Hardware Configuration
            if (SelectedCPU <= 0 || SelectedMemory <= 0 || SystemDiskSizeGB <= 0)
                return false;
                
            // Validate Virtual Switch
            if (string.IsNullOrWhiteSpace(SelectedVirtualSwitch))
                return false;
                
            // Validate Data Disks
            if (DataDisks.Count == 0)
                return false;
                
            // Validate Static IP Configuration
            if (IsStaticIP)
            {
                if (string.IsNullOrWhiteSpace(IPAddress) || string.IsNullOrWhiteSpace(SubnetMask) || 
                    string.IsNullOrWhiteSpace(DefaultGateway) || string.IsNullOrWhiteSpace(PreferredDNS))
                    return false;
            }
            
            // Validate Domain Join
            if (JoinDomain)
            {
                if (string.IsNullOrWhiteSpace(DomainName) || string.IsNullOrWhiteSpace(DomainUsername))
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
                SystemDiskSizeGB = SystemDiskSizeGB,
                
                ServerRole = "FileServer",
                FileServerConfig = new
                {
                    DataDisks = DataDisks.Select(d => new {
                        Size = d.Size,
                        Letter = d.Letter,
                        Label = d.Label,
                        FileSystem = d.FileSystem
                    }).ToList(),
                    
                    Shares = Shares.Select(s => new {
                        Name = s.Name,
                        Path = s.Path,
                        Description = s.Description,
                        Permissions = s.Permissions.Select(p => new {
                            Identity = p.Identity,
                            AccessRights = p.AccessRights
                        }).ToList()
                    }).ToList(),
                    
                    EnableFSRM = EnableFSRM,
                    EnableQuotas = EnableQuotas,
                    EnableFileScreens = EnableFileScreens,
                    EnableDedup = EnableDedup,
                    EnableShadowCopies = EnableShadowCopies
                },
                
                NetworkConfig = new
                {
                    UseDHCP = IsDHCP,
                    IPAddress = IPAddress,
                    SubnetMask = SubnetMask,
                    DefaultGateway = DefaultGateway,
                    PreferredDNS = PreferredDNS
                },
                
                DomainConfig = new
                {
                    JoinDomain = JoinDomain,
                    DomainName = DomainName,
                    DomainOU = DomainOU,
                    DomainUsername = DomainUsername
                },
                
                AdvancedOptions = new
                {
                    InstallRSAT = InstallRSAT,
                    InstallBackupAgent = InstallBackupAgent
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
    
    public class DataDiskViewModel : INotifyPropertyChanged
    {
        private int _size = 500;
        public int Size
        {
            get => _size;
            set
            {
                if (_size != value)
                {
                    _size = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _letter = "D";
        public string Letter
        {
            get => _letter;
            set
            {
                if (_letter != value)
                {
                    _letter = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _label = "Data";
        public string Label
        {
            get => _label;
            set
            {
                if (_label != value)
                {
                    _label = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _fileSystem = "NTFS";
        public string FileSystem
        {
            get => _fileSystem;
            set
            {
                if (_fileSystem != value)
                {
                    _fileSystem = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    public class ShareViewModel : INotifyPropertyChanged
    {
        private string _name = "SharedData";
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _path = "D:\\SharedData";
        public string Path
        {
            get => _path;
            set
            {
                if (_path != value)
                {
                    _path = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _description = "General shared data folder";
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public ObservableCollection<SharePermissionViewModel> Permissions { get; } = new ObservableCollection<SharePermissionViewModel>();
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    public class SharePermissionViewModel : INotifyPropertyChanged
    {
        private string _identity = "Everyone";
        public string Identity
        {
            get => _identity;
            set
            {
                if (_identity != value)
                {
                    _identity = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _accessRights = "Read";
        public string AccessRights
        {
            get => _accessRights;
            set
            {
                if (_accessRights != value)
                {
                    _accessRights = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 
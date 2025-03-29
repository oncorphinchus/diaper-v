using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Input;

namespace HyperVCreator.App.ViewModels.ServerRoles.SQLServer
{
    /// <summary>
    /// ViewModel for SQL Server configuration
    /// </summary>
    public class SQLServerConfigViewModel : INotifyPropertyChanged
    {
        #region Private fields
        private string _vmName = "SQL-Server";
        private string _instanceName = "MSSQLSERVER";
        private bool _isWindowsAuth = true;
        private bool _isMixedMode;
        private bool _isDHCP = true;
        private bool _isStaticIP;
        private string _ipAddress;
        private string _subnetMask = "255.255.255.0";
        private string _defaultGateway;
        private string _dnsServers;
        private bool _joinDomain;
        private string _domainName;
        private bool _createMaintenanceJobs = true;
        private bool _includeDatabaseEngine = true;
        private bool _includeSSMS = true;
        private bool _includeRS;
        private bool _includeAS;
        private bool _includeIS;
        
        private int _selectedCPU = 4;
        private int _selectedMemory = 8;
        private string _selectedVirtualSwitch;
        private string _selectedSQLEdition = "Developer";
        private int _selectedSystemDisk = 80;
        private int _selectedDataDisk = 100;
        private int _selectedLogDisk = 50;
        private int _selectedTempDBDisk = 50;
        private int _selectedBackupDisk = 100;
        #endregion
        
        #region Observable Collections
        public ObservableCollection<int> CPUOptions { get; } = new ObservableCollection<int> { 1, 2, 4, 6, 8, 12, 16 };
        public ObservableCollection<int> MemoryOptions { get; } = new ObservableCollection<int> { 4, 8, 16, 32, 64, 128 };
        public ObservableCollection<string> VirtualSwitches { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> SQLEditions { get; } = new ObservableCollection<string> 
        { 
            "Express", 
            "Standard", 
            "Enterprise", 
            "Developer" 
        };
        public ObservableCollection<int> SystemDiskOptions { get; } = new ObservableCollection<int> { 40, 60, 80, 100, 120, 150, 200 };
        public ObservableCollection<int> DataDiskOptions { get; } = new ObservableCollection<int> { 50, 100, 200, 300, 500, 1000 };
        public ObservableCollection<int> LogDiskOptions { get; } = new ObservableCollection<int> { 20, 50, 100, 200, 300 };
        public ObservableCollection<int> TempDBDiskOptions { get; } = new ObservableCollection<int> { 20, 50, 100, 200 };
        public ObservableCollection<int> BackupDiskOptions { get; } = new ObservableCollection<int> { 50, 100, 200, 500, 1000 };
        #endregion
        
        #region Properties
        public string VMName
        {
            get => _vmName;
            set => SetProperty(ref _vmName, value);
        }
        
        public int SelectedCPU
        {
            get => _selectedCPU;
            set => SetProperty(ref _selectedCPU, value);
        }
        
        public int SelectedMemory
        {
            get => _selectedMemory;
            set => SetProperty(ref _selectedMemory, value);
        }
        
        public string SelectedVirtualSwitch
        {
            get => _selectedVirtualSwitch;
            set => SetProperty(ref _selectedVirtualSwitch, value);
        }
        
        public string SelectedSQLEdition
        {
            get => _selectedSQLEdition;
            set => SetProperty(ref _selectedSQLEdition, value);
        }
        
        public string InstanceName
        {
            get => _instanceName;
            set => SetProperty(ref _instanceName, value);
        }
        
        public bool IsWindowsAuth
        {
            get => _isWindowsAuth;
            set
            {
                if (SetProperty(ref _isWindowsAuth, value) && value)
                {
                    IsMixedMode = false;
                }
            }
        }
        
        public bool IsMixedMode
        {
            get => _isMixedMode;
            set
            {
                if (SetProperty(ref _isMixedMode, value) && value)
                {
                    IsWindowsAuth = false;
                }
            }
        }
        
        public bool IncludeDatabaseEngine
        {
            get => _includeDatabaseEngine;
            set => SetProperty(ref _includeDatabaseEngine, value);
        }
        
        public bool IncludeSSMS
        {
            get => _includeSSMS;
            set => SetProperty(ref _includeSSMS, value);
        }
        
        public bool IncludeRS
        {
            get => _includeRS;
            set => SetProperty(ref _includeRS, value);
        }
        
        public bool IncludeAS
        {
            get => _includeAS;
            set => SetProperty(ref _includeAS, value);
        }
        
        public bool IncludeIS
        {
            get => _includeIS;
            set => SetProperty(ref _includeIS, value);
        }
        
        public int SelectedSystemDisk
        {
            get => _selectedSystemDisk;
            set => SetProperty(ref _selectedSystemDisk, value);
        }
        
        public int SelectedDataDisk
        {
            get => _selectedDataDisk;
            set => SetProperty(ref _selectedDataDisk, value);
        }
        
        public int SelectedLogDisk
        {
            get => _selectedLogDisk;
            set => SetProperty(ref _selectedLogDisk, value);
        }
        
        public int SelectedTempDBDisk
        {
            get => _selectedTempDBDisk;
            set => SetProperty(ref _selectedTempDBDisk, value);
        }
        
        public int SelectedBackupDisk
        {
            get => _selectedBackupDisk;
            set => SetProperty(ref _selectedBackupDisk, value);
        }
        
        public bool IsDHCP
        {
            get => _isDHCP;
            set
            {
                if (SetProperty(ref _isDHCP, value) && value)
                {
                    IsStaticIP = false;
                }
            }
        }
        
        public bool IsStaticIP
        {
            get => _isStaticIP;
            set
            {
                if (SetProperty(ref _isStaticIP, value) && value)
                {
                    IsDHCP = false;
                }
            }
        }
        
        public string IPAddress
        {
            get => _ipAddress;
            set => SetProperty(ref _ipAddress, value);
        }
        
        public string SubnetMask
        {
            get => _subnetMask;
            set => SetProperty(ref _subnetMask, value);
        }
        
        public string DefaultGateway
        {
            get => _defaultGateway;
            set => SetProperty(ref _defaultGateway, value);
        }
        
        public string DNSServers
        {
            get => _dnsServers;
            set => SetProperty(ref _dnsServers, value);
        }
        
        public bool JoinDomain
        {
            get => _joinDomain;
            set => SetProperty(ref _joinDomain, value);
        }
        
        public string DomainName
        {
            get => _domainName;
            set => SetProperty(ref _domainName, value);
        }
        
        public bool CreateMaintenanceJobs
        {
            get => _createMaintenanceJobs;
            set => SetProperty(ref _createMaintenanceJobs, value);
        }
        #endregion
        
        #region Commands
        public ICommand CreateVMCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion
        
        public SQLServerConfigViewModel()
        {
            // Initialize commands
            CreateVMCommand = new RelayCommand(ExecuteCreateVM, CanExecuteCreateVM);
            CancelCommand = new RelayCommand(ExecuteCancel);
            
            // Initialize virtual switches
            LoadVirtualSwitches();
        }
        
        private void LoadVirtualSwitches()
        {
            VirtualSwitches.Clear();
            
            // In a real implementation, this would load from Hyper-V
            VirtualSwitches.Add("Default Switch");
            VirtualSwitches.Add("Internal Network");
            VirtualSwitches.Add("External Network");
            
            // Set default selection
            SelectedVirtualSwitch = "Default Switch";
        }
        
        private bool CanExecuteCreateVM(object parameter)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(VMName)) return false;
            if (string.IsNullOrWhiteSpace(SelectedVirtualSwitch)) return false;
            if (SelectedCPU <= 0) return false;
            if (SelectedMemory <= 0) return false;
            if (SelectedSystemDisk <= 0) return false;
            
            // Validate SQL Server specific settings
            if (!IncludeDatabaseEngine) return false; // At least Database Engine is required
            
            // Additional SQL Server validation
            if (IsMixedMode)
            {
                // Would validate SA password here
            }
            
            // Network validation
            if (IsStaticIP)
            {
                if (string.IsNullOrWhiteSpace(IPAddress)) return false;
                if (string.IsNullOrWhiteSpace(SubnetMask)) return false;
            }
            
            // Domain validation
            if (JoinDomain && string.IsNullOrWhiteSpace(DomainName)) return false;
            
            return true;
        }
        
        private void ExecuteCreateVM(object parameter)
        {
            // Build SQL Server features string
            List<string> features = new List<string>();
            if (IncludeDatabaseEngine) features.Add("SQLENGINE");
            if (IncludeSSMS) features.Add("SSMS");
            if (IncludeRS) features.Add("RS");
            if (IncludeAS) features.Add("AS");
            if (IncludeIS) features.Add("IS");
            
            // In a real implementation, we would:
            // 1. Get the SA password from the PasswordBox
            // 2. Create a VM template
            // 3. Call the VM creation service
            
            // Example:
            // var vmTemplate = new VMTemplate
            // {
            //     TemplateName = "SQL Server",
            //     ServerRole = "SQLServer",
            //     Description = "SQL Server VM",
            //     HardwareConfiguration = new HardwareConfig
            //     {
            //         ProcessorCount = SelectedCPU,
            //         MemoryGB = SelectedMemory,
            //         StorageGB = SelectedSystemDisk,
            //         Generation = 2,
            //         EnableSecureBoot = true
            //     },
            //     NetworkConfiguration = new NetworkConfig
            //     {
            //         VirtualSwitch = SelectedVirtualSwitch,
            //         DynamicIP = IsDHCP,
            //         IPAddress = IPAddress,
            //         SubnetMask = SubnetMask,
            //         DefaultGateway = DefaultGateway,
            //         DNSServers = DNSServers
            //     },
            //     ...
            // };
            
            // var vmService = new VMCreationService();
            // await vmService.CreateVMAsync(vmTemplate, progress, cancellationToken);
        }
        
        private void ExecuteCancel(object parameter)
        {
            // In a real implementation, this would navigate back or close the form
        }
        
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
    
    /// <summary>
    /// Simple implementation of ICommand for the ViewModel
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;
        
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }
        
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }
        
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
} 
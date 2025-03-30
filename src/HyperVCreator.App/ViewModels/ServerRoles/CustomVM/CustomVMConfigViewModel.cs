using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HyperVCreator.App.Commands;
using HyperVCreator.App.Services;
using HyperVCreator.Core.Models;
using HyperVCreator.Core.Services;

namespace HyperVCreator.App.ViewModels.ServerRoles.CustomVM
{
    public class CustomVMConfigViewModel : ViewModelBase
    {
        private readonly PowerShellService _powerShellService;
        private readonly TemplateService _templateService;
        private CancellationTokenSource _cancellationTokenSource;
        
        #region Properties
        
        private string _vmName;
        public string VMName
        {
            get => _vmName;
            set
            {
                if (_vmName != value)
                {
                    _vmName = value;
                    OnPropertyChanged();
                    CreateVMCommand.RaiseCanExecuteChanged();
                }
            }
        }
        
        private string _computerName;
        public string ComputerName
        {
            get => _computerName;
            set
            {
                if (_computerName != value)
                {
                    _computerName = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _description;
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
        
        private int _cpuCount = 2;
        public int CPUCount
        {
            get => _cpuCount;
            set
            {
                if (_cpuCount != value && value >= 1)
                {
                    _cpuCount = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private int _memoryGB = 4;
        public int MemoryGB
        {
            get => _memoryGB;
            set
            {
                if (_memoryGB != value && value >= 1)
                {
                    _memoryGB = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private int _storageGB = 80;
        public int StorageGB
        {
            get => _storageGB;
            set
            {
                if (_storageGB != value && value >= 20)
                {
                    _storageGB = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _virtualSwitch = "Default Switch";
        public string VirtualSwitch
        {
            get => _virtualSwitch;
            set
            {
                if (_virtualSwitch != value)
                {
                    _virtualSwitch = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _osVersion = "Windows Server 2022 Standard";
        public string OSVersion
        {
            get => _osVersion;
            set
            {
                if (_osVersion != value)
                {
                    _osVersion = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private bool _dynamicIP = true;
        public bool DynamicIP
        {
            get => _dynamicIP;
            set
            {
                if (_dynamicIP != value)
                {
                    _dynamicIP = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _ipAddress;
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
        
        private string _defaultGateway;
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
        
        private string _dnsServer;
        public string DNSServer
        {
            get => _dnsServer;
            set
            {
                if (_dnsServer != value)
                {
                    _dnsServer = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private int _additionalDiskSizeGB;
        public int AdditionalDiskSizeGB
        {
            get => _additionalDiskSizeGB;
            set
            {
                if (_additionalDiskSizeGB != value)
                {
                    _additionalDiskSizeGB = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private bool _useUnattendXML = true;
        public bool UseUnattendXML
        {
            get => _useUnattendXML;
            set
            {
                if (_useUnattendXML != value)
                {
                    _useUnattendXML = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _adminPassword = "P@ssw0rd";
        public string AdminPassword
        {
            get => _adminPassword;
            set
            {
                if (_adminPassword != value)
                {
                    _adminPassword = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private int _timeZone = 85;
        public int TimeZone
        {
            get => _timeZone;
            set
            {
                if (_timeZone != value)
                {
                    _timeZone = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private bool _autoStart;
        public bool AutoStart
        {
            get => _autoStart;
            set
            {
                if (_autoStart != value)
                {
                    _autoStart = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private int _generation = 2;
        public int Generation
        {
            get => _generation;
            set
            {
                if (_generation != value && (value == 1 || value == 2))
                {
                    _generation = value;
                    OnPropertyChanged();
                    EnableSecureBoot = value == 2 && EnableSecureBoot;
                }
            }
        }
        
        private bool _enableSecureBoot = true;
        public bool EnableSecureBoot
        {
            get => _enableSecureBoot;
            set
            {
                if (_enableSecureBoot != value)
                {
                    _enableSecureBoot = value && Generation == 2;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _isoPath;
        public string ISOPath
        {
            get => _isoPath;
            set
            {
                if (_isoPath != value)
                {
                    _isoPath = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _productKey;
        public string ProductKey
        {
            get => _productKey;
            set
            {
                if (_productKey != value)
                {
                    _productKey = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _statusMessage = "Ready to create Custom VM";
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private int _progressValue;
        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                if (_progressValue != value)
                {
                    _progressValue = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private bool _isCreating;
        public bool IsCreating
        {
            get => _isCreating;
            set
            {
                if (_isCreating != value)
                {
                    _isCreating = value;
                    OnPropertyChanged();
                    CreateVMCommand.RaiseCanExecuteChanged();
                    CancelCommand.RaiseCanExecuteChanged();
                }
            }
        }
        
        private bool _showProgressDetails;
        public bool ShowProgressDetails
        {
            get => _showProgressDetails;
            set
            {
                if (_showProgressDetails != value)
                {
                    _showProgressDetails = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private ObservableCollection<string> _creationLogs = new ObservableCollection<string>();
        public ObservableCollection<string> CreationLogs
        {
            get => _creationLogs;
            set
            {
                if (_creationLogs != value)
                {
                    _creationLogs = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private ObservableCollection<string> _availableOSVersions = new ObservableCollection<string>
        {
            "Windows Server 2022 Standard",
            "Windows Server 2022 Datacenter",
            "Windows Server 2019 Standard",
            "Windows Server 2019 Datacenter",
            "Windows Server 2016 Standard",
            "Windows Server 2016 Datacenter",
            "Windows 10 Pro",
            "Windows 10 Enterprise",
            "Windows 11 Pro",
            "Windows 11 Enterprise"
        };
        public ObservableCollection<string> AvailableOSVersions
        {
            get => _availableOSVersions;
            set
            {
                if (_availableOSVersions != value)
                {
                    _availableOSVersions = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private ObservableCollection<string> _availableVirtualSwitches = new ObservableCollection<string> { "Default Switch" };
        public ObservableCollection<string> AvailableVirtualSwitches
        {
            get => _availableVirtualSwitches;
            set
            {
                if (_availableVirtualSwitches != value)
                {
                    _availableVirtualSwitches = value;
                    OnPropertyChanged();
                }
            }
        }
        
        #endregion
        
        #region Commands
        
        private RelayCommand _createVMCommand;
        public RelayCommand CreateVMCommand => _createVMCommand ??= new RelayCommand(
            execute: async _ => await CreateVMAsync(),
            canExecute: _ => !IsCreating && !string.IsNullOrWhiteSpace(VMName)
        );
        
        private RelayCommand _cancelCommand;
        public RelayCommand CancelCommand => _cancelCommand ??= new RelayCommand(
            execute: _ => CancelCreation(),
            canExecute: _ => IsCreating
        );
        
        private RelayCommand _loadTemplateCommand;
        public RelayCommand LoadTemplateCommand => _loadTemplateCommand ??= new RelayCommand(
            execute: _ => LoadTemplate(),
            canExecute: _ => !IsCreating
        );
        
        private RelayCommand _browseISOCommand;
        public RelayCommand BrowseISOCommand => _browseISOCommand ??= new RelayCommand(
            execute: _ => BrowseForISO(),
            canExecute: _ => !IsCreating
        );
        
        #endregion
        
        public CustomVMConfigViewModel()
        {
            _powerShellService = App.Current.Resources["PowerShellService"] as PowerShellService;
            _templateService = new TemplateService();
            
            // Load default template
            InitializeFromTemplate(_templateService.GetTemplateByName("Default Custom VM"));
            
            // Add detailed logging for creation progress
            _powerShellService.OutputReceived += (sender, output) =>
            {
                if (output.Contains("StatusUpdate") || output.Contains("PercentComplete"))
                {
                    try
                    {
                        // Extract progress information
                        if (output.Contains("PercentComplete"))
                        {
                            var percentIndex = output.IndexOf("PercentComplete") + "PercentComplete".Length;
                            var colonIndex = output.IndexOf(":", percentIndex);
                            var commaIndex = output.IndexOf(",", colonIndex);
                            
                            if (colonIndex > 0 && commaIndex > 0)
                            {
                                var percentString = output.Substring(colonIndex + 1, commaIndex - colonIndex - 1).Trim();
                                if (int.TryParse(percentString, out int percent))
                                {
                                    ProgressValue = percent;
                                }
                            }
                        }
                        
                        // Extract status message
                        if (output.Contains("StatusMessage"))
                        {
                            var statusIndex = output.IndexOf("StatusMessage") + "StatusMessage".Length;
                            var colonIndex = output.IndexOf(":", statusIndex);
                            var endIndex = output.IndexOf("}", colonIndex);
                            
                            if (colonIndex > 0 && endIndex > 0)
                            {
                                var statusString = output.Substring(colonIndex + 1, endIndex - colonIndex - 1).Trim();
                                if (statusString.StartsWith("\"") && statusString.EndsWith("\""))
                                {
                                    statusString = statusString.Substring(1, statusString.Length - 2);
                                }
                                
                                StatusMessage = statusString;
                                Application.Current.Dispatcher.Invoke(() => CreationLogs.Add(statusString));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() => CreationLogs.Add($"Error parsing progress: {ex.Message}"));
                    }
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() => CreationLogs.Add(output));
                }
            };
            
            // Load available virtual switches
            LoadVirtualSwitches();
        }
        
        private async void LoadVirtualSwitches()
        {
            try
            {
                var switches = await _powerShellService.ExecuteScriptAsync("Get-VMSwitch | Select-Object -ExpandProperty Name");
                if (switches != null && switches.Any())
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AvailableVirtualSwitches.Clear();
                        foreach (var switchName in switches)
                        {
                            AvailableVirtualSwitches.Add(switchName.Trim());
                        }
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AvailableVirtualSwitches.Clear();
                        AvailableVirtualSwitches.Add("Default Switch");
                    });
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AvailableVirtualSwitches.Clear();
                    AvailableVirtualSwitches.Add("Default Switch");
                    CreationLogs.Add($"Error loading virtual switches: {ex.Message}");
                });
            }
        }
        
        private void InitializeFromTemplate(VMTemplate template)
        {
            if (template == null) return;
            
            // Set hardware configuration
            CPUCount = template.HardwareConfiguration.ProcessorCount;
            MemoryGB = template.HardwareConfiguration.MemoryGB;
            StorageGB = template.HardwareConfiguration.StorageGB;
            Generation = template.HardwareConfiguration.Generation;
            EnableSecureBoot = template.HardwareConfiguration.EnableSecureBoot;
            
            // Set network configuration
            DynamicIP = template.NetworkConfiguration.DynamicIP;
            if (!template.NetworkConfiguration.DynamicIP && template.NetworkConfiguration.StaticIPConfiguration != null)
            {
                IPAddress = template.NetworkConfiguration.StaticIPConfiguration.IPAddress;
                SubnetMask = template.NetworkConfiguration.StaticIPConfiguration.SubnetMask;
                DefaultGateway = template.NetworkConfiguration.StaticIPConfiguration.DefaultGateway;
                DNSServer = template.NetworkConfiguration.StaticIPConfiguration.DNSServers.FirstOrDefault() ?? "";
            }
            
            // Set OS configuration
            OSVersion = template.OSConfiguration.OSVersion;
            AdminPassword = template.OSConfiguration.AdminPassword;
            TimeZone = template.OSConfiguration.TimeZone;
            ProductKey = template.OSConfiguration.ProductKey;
            
            // Set additional configuration
            UseUnattendXML = template.AdditionalConfiguration.UseUnattendXML;
            AutoStart = template.AdditionalConfiguration.AutoStartVM;
            
            // Set VM description
            Description = template.Description;
        }
        
        private void LoadTemplate()
        {
            try
            {
                var templateSelector = new Views.TemplateListView();
                var templateSelectorViewModel = templateSelector.DataContext as TemplateListViewModel;
                
                if (templateSelectorViewModel != null)
                {
                    templateSelectorViewModel.SelectionMode = true;
                    
                    if (templateSelector.ShowDialog() == true && templateSelectorViewModel.SelectedTemplate != null)
                    {
                        var template = _templateService.GetTemplateByName(templateSelectorViewModel.SelectedTemplate.Name);
                        if (template != null)
                        {
                            InitializeFromTemplate(template);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading template: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void BrowseForISO()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".iso",
                Filter = "ISO Images|*.iso"
            };
            
            if (dialog.ShowDialog() == true)
            {
                ISOPath = dialog.FileName;
            }
        }
        
        private async Task CreateVMAsync()
        {
            try
            {
                IsCreating = true;
                ProgressValue = 0;
                StatusMessage = "Preparing to create Custom VM...";
                CreationLogs.Clear();
                ShowProgressDetails = true;
                
                _cancellationTokenSource = new CancellationTokenSource();
                
                // Build parameter hashtable
                var parameters = new Dictionary<string, object>
                {
                    { "VMName", VMName },
                    { "OSVersion", OSVersion },
                    { "VMDescription", Description ?? $"Custom VM created on {DateTime.Now}" },
                    { "CPUCount", CPUCount },
                    { "MemoryGB", MemoryGB },
                    { "StorageGB", StorageGB },
                    { "VirtualSwitch", VirtualSwitch },
                    { "Generation", Generation },
                    { "EnableSecureBoot", EnableSecureBoot },
                    { "UseUnattendXML", UseUnattendXML },
                    { "AutoStart", AutoStart },
                    { "AdminPassword", AdminPassword },
                    { "TimeZone", TimeZone }
                };
                
                // Add optional parameters
                if (!string.IsNullOrWhiteSpace(ComputerName))
                {
                    parameters.Add("ComputerName", ComputerName);
                }
                
                if (!string.IsNullOrWhiteSpace(ISOPath))
                {
                    parameters.Add("ISOPath", ISOPath);
                }
                
                if (!string.IsNullOrWhiteSpace(ProductKey))
                {
                    parameters.Add("ProductKey", ProductKey);
                }
                
                if (!DynamicIP)
                {
                    if (!string.IsNullOrWhiteSpace(IPAddress))
                    {
                        parameters.Add("IPAddress", IPAddress);
                        parameters.Add("SubnetMask", SubnetMask);
                        
                        if (!string.IsNullOrWhiteSpace(DefaultGateway))
                        {
                            parameters.Add("DefaultGateway", DefaultGateway);
                        }
                        
                        if (!string.IsNullOrWhiteSpace(DNSServer))
                        {
                            parameters.Add("DNSServer", DNSServer);
                        }
                    }
                }
                
                if (AdditionalDiskSizeGB > 0)
                {
                    parameters.Add("AdditionalDiskSizeGB", AdditionalDiskSizeGB);
                }
                
                // Execute the script
                var script = $@"
# Import CustomVM.ps1 from the RoleConfiguration directory
$scriptPath = Split-Path -Parent (Get-Module -ListAvailable HyperVCreator.Scripts).Path
$customVMScript = Join-Path $scriptPath 'RoleConfiguration\CustomVM.ps1'

# Create parameter hashtable
$params = @{{
    VMName = '{VMName}'
    OSVersion = '{OSVersion}'
    VMDescription = '{Description ?? $"Custom VM created on {DateTime.Now}"}'
    CPUCount = {CPUCount}
    MemoryGB = {MemoryGB}
    StorageGB = {StorageGB}
    VirtualSwitch = '{VirtualSwitch}'
    Generation = {Generation}
    EnableSecureBoot = ${EnableSecureBoot.ToString().ToLower()}
    UseUnattendXML = ${UseUnattendXML.ToString().ToLower()}
    AutoStart = ${AutoStart.ToString().ToLower()}
    AdminPassword = '{AdminPassword}'
    TimeZone = {TimeZone}
";
                
                if (!string.IsNullOrWhiteSpace(ComputerName))
                {
                    script += $"    ComputerName = '{ComputerName}'\r\n";
                }
                
                if (!string.IsNullOrWhiteSpace(ISOPath))
                {
                    script += $"    ISOPath = '{ISOPath.Replace("\\", "\\\\")}'\r\n";
                }
                
                if (!string.IsNullOrWhiteSpace(ProductKey))
                {
                    script += $"    ProductKey = '{ProductKey}'\r\n";
                }
                
                if (!DynamicIP && !string.IsNullOrWhiteSpace(IPAddress))
                {
                    script += $"    IPAddress = '{IPAddress}'\r\n";
                    script += $"    SubnetMask = '{SubnetMask}'\r\n";
                    
                    if (!string.IsNullOrWhiteSpace(DefaultGateway))
                    {
                        script += $"    DefaultGateway = '{DefaultGateway}'\r\n";
                    }
                    
                    if (!string.IsNullOrWhiteSpace(DNSServer))
                    {
                        script += $"    DNSServer = '{DNSServer}'\r\n";
                    }
                }
                
                if (AdditionalDiskSizeGB > 0)
                {
                    script += $"    AdditionalDiskSizeGB = {AdditionalDiskSizeGB}\r\n";
                }
                
                script += @"}

# Execute the script
if (Test-Path $customVMScript) {
    . $customVMScript @params
}
else {
    Write-Error ""CustomVM.ps1 script not found at: $customVMScript""
}
";
                
                CreationLogs.Add($"Creating Custom VM '{VMName}'...");
                CreationLogs.Add($"CPU Count: {CPUCount}, Memory: {MemoryGB}GB, Storage: {StorageGB}GB");
                CreationLogs.Add($"OS Version: {OSVersion}");
                CreationLogs.Add($"Virtual Switch: {VirtualSwitch}");
                CreationLogs.Add($"Generation: {Generation}, Secure Boot: {EnableSecureBoot}");
                
                var result = await _powerShellService.ExecuteScriptAsync(script, _cancellationTokenSource.Token);
                
                if (result != null && result.Any(r => r.Contains("Success") && r.Contains("true")))
                {
                    StatusMessage = $"Custom VM '{VMName}' created successfully!";
                    ProgressValue = 100;
                    CreationLogs.Add($"Custom VM '{VMName}' created successfully!");
                }
                else
                {
                    StatusMessage = "Failed to create Custom VM. Check logs for details.";
                    CreationLogs.Add("Failed to create Custom VM. See error details above.");
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Operation cancelled.";
                CreationLogs.Add("Custom VM creation was cancelled by user.");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                CreationLogs.Add($"Error creating Custom VM: {ex.Message}");
                if (ex.InnerException != null)
                {
                    CreationLogs.Add($"Inner exception: {ex.InnerException.Message}");
                }
            }
            finally
            {
                IsCreating = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }
        
        private void CancelCreation()
        {
            _cancellationTokenSource?.Cancel();
            StatusMessage = "Cancelling operation...";
        }
    }
} 
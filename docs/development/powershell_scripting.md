# PowerShell Scripting Guide

This document outlines the standards, best practices, and implementation details for PowerShell scripts used in the Hyper-V VM Creation Application.

## Overview

PowerShell is the primary automation technology used in this application for:
1. Creating and configuring Hyper-V virtual machines
2. Automated OS installation via unattended setup
3. Post-installation configuration of server roles
4. Network and storage configuration

## PowerShell Script Organization

### Directory Structure

```
Scripts/
├── Common/                      # Shared utility functions
│   ├── ErrorHandling.ps1        # Error handling functions
│   ├── Logging.ps1              # Logging functions
│   └── Validation.ps1           # Input validation functions
├── HyperV/                      # VM creation and management
│   ├── CreateVM.ps1             # VM creation functions
│   ├── ConfigureNetwork.ps1     # Network configuration
│   └── ConfigureStorage.ps1     # Storage configuration
├── UnattendXML/                 # Unattend.xml generation
│   ├── GenerateUnattend.ps1     # XML generation functions
│   └── Templates/               # Template XML files
├── RoleConfiguration/           # Server role configuration
│   ├── DomainController.ps1     # DC configuration
│   ├── FileServer.ps1           # File Server configuration
│   ├── WebServer.ps1            # Web Server configuration
│   ├── SQLServer.ps1            # SQL Server configuration
│   ├── RDSHServer.ps1           # RDSH configuration
│   ├── DHCPServer.ps1           # DHCP Server configuration
│   └── DNSServer.ps1            # DNS Server configuration
└── Monitoring/                  # Progress monitoring
    ├── TrackProgress.ps1        # Progress tracking functions
    └── ReportStatus.ps1         # Status reporting functions
```

## Script Design Principles

### Modularity

- Each script should focus on a single responsibility
- Functions should be small and focused on specific tasks
- Utilize parameter blocks to make scripts configurable
- Use dot-sourcing to import common functions

### Error Handling

- Use try/catch blocks for operations that may fail
- Implement proper error reporting and logging
- Return standardized error objects
- Ensure errors are captured and reported to the C# application

### Logging

- Implement verbose logging for debugging
- Use Write-Verbose, Write-Warning, and Write-Error appropriately
- Include timestamps in log entries
- Log both to console and file when appropriate

### Security

- Scripts should validate input parameters
- Avoid using plain-text credentials
- Use appropriate PowerShell execution policy
- Follow the principle of least privilege

## PowerShell Execution from C#

### Runspace Management

The application uses PowerShell runspaces for script execution:

```csharp
public class PowerShellExecutor
{
    public PowerShellResult ExecuteScript(string scriptPath, Dictionary<string, object> parameters)
    {
        var result = new PowerShellResult();
        
        try
        {
            // Create a runspace
            using (var runspace = RunspaceFactory.CreateRunspace())
            {
                runspace.Open();
                
                // Create a pipeline
                using (var pipeline = runspace.CreatePipeline())
                {
                    // Create the script command
                    var scriptCommand = new Command(scriptPath, true);
                    
                    // Add parameters
                    foreach (var param in parameters)
                    {
                        scriptCommand.Parameters.Add(param.Key, param.Value);
                    }
                    
                    // Add the command to the pipeline
                    pipeline.Commands.Add(scriptCommand);
                    
                    // Execute the script
                    var output = pipeline.Invoke();
                    
                    // Process the output
                    result.Output = output;
                    result.Success = true;
                }
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Exception = ex;
        }
        
        return result;
    }
}
```

### Script Parameter Passing

Parameters are passed to PowerShell scripts as key-value pairs:

```csharp
var parameters = new Dictionary<string, object>
{
    { "VMName", "DC01" },
    { "CPUCount", 2 },
    { "MemoryGB", 4 },
    { "StorageGB", 80 },
    { "VirtualSwitch", "Default Switch" },
    { "DomainName", "contoso.local" }
};

var result = _powerShellExecutor.ExecuteScript("Scripts/RoleConfiguration/DomainController.ps1", parameters);
```

### Output Processing

PowerShell script output is captured and parsed for status updates and results:

```csharp
// Process PowerShell output objects
foreach (var outputObject in result.Output)
{
    // Check if it's a status update
    if (outputObject.Properties["Type"]?.Value?.ToString() == "StatusUpdate")
    {
        var percent = Convert.ToInt32(outputObject.Properties["PercentComplete"]?.Value);
        var status = outputObject.Properties["StatusMessage"]?.Value?.ToString();
        
        // Update progress
        _progressViewModel.UpdateProgress(percent, status);
    }
    
    // Check if it's a result object
    if (outputObject.Properties["Type"]?.Value?.ToString() == "Result")
    {
        var success = Convert.ToBoolean(outputObject.Properties["Success"]?.Value);
        var message = outputObject.Properties["Message"]?.Value?.ToString();
        
        // Handle the result
        if (success)
        {
            // Operation succeeded
        }
        else
        {
            // Operation failed
        }
    }
}
```

## Script Templates and Examples

### Common Function Template

```powershell
function Invoke-Operation {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$Parameter1,
        
        [Parameter(Mandatory=$false)]
        [int]$Parameter2 = 1
    )
    
    begin {
        Write-Verbose "Starting operation with $Parameter1"
    }
    
    process {
        try {
            # Perform the operation
            Write-Verbose "Performing operation..."
            
            # Create a result object
            $result = [PSCustomObject]@{
                Type = "StatusUpdate"
                PercentComplete = 50
                StatusMessage = "Operation in progress..."
            }
            
            # Output status update
            Write-Output $result
            
            # Complete the operation
            
            # Output final result
            $result = [PSCustomObject]@{
                Type = "Result"
                Success = $true
                Message = "Operation completed successfully"
            }
            
            Write-Output $result
        }
        catch {
            Write-Error "Operation failed: $_"
            
            # Output error result
            $result = [PSCustomObject]@{
                Type = "Result"
                Success = $false
                Message = "Operation failed: $_"
                ErrorDetails = $_
            }
            
            Write-Output $result
        }
    }
    
    end {
        Write-Verbose "Operation completed"
    }
}
```

### VM Creation Script Example

```powershell
# CreateVM.ps1
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$VMName,
    
    [Parameter(Mandatory=$false)]
    [int]$CPUCount = 2,
    
    [Parameter(Mandatory=$false)]
    [int]$MemoryGB = 4,
    
    [Parameter(Mandatory=$false)]
    [int]$StorageGB = 80,
    
    [Parameter(Mandatory=$false)]
    [string]$VirtualSwitch = "Default Switch",
    
    [Parameter(Mandatory=$false)]
    [string]$VHDPath = $null
)

# Import common functions
. "$PSScriptRoot\..\Common\ErrorHandling.ps1"
. "$PSScriptRoot\..\Common\Logging.ps1"

function New-HyperVVM {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$VMName,
        
        [Parameter(Mandatory=$true)]
        [int]$CPUCount,
        
        [Parameter(Mandatory=$true)]
        [int]$MemoryGB,
        
        [Parameter(Mandatory=$true)]
        [int]$StorageGB,
        
        [Parameter(Mandatory=$true)]
        [string]$VirtualSwitch,
        
        [Parameter(Mandatory=$false)]
        [string]$VHDPath
    )
    
    try {
        # Status update - Starting
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 0
            StatusMessage = "Creating virtual machine..."
        }
        Write-Output $statusUpdate
        
        # Validate parameters
        if ([string]::IsNullOrEmpty($VHDPath)) {
            $VHDPath = "C:\Users\Public\Documents\Hyper-V\Virtual Hard Disks\$VMName.vhdx"
        }
        
        # Check if VM exists
        $existingVM = Get-VM -Name $VMName -ErrorAction SilentlyContinue
        if ($existingVM) {
            throw "A VM with the name '$VMName' already exists."
        }
        
        # Status update - Creating VHD
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 10
            StatusMessage = "Creating virtual hard disk..."
        }
        Write-Output $statusUpdate
        
        # Create VHD
        $null = New-VHD -Path $VHDPath -SizeBytes ($StorageGB * 1GB) -Dynamic
        
        # Status update - Creating VM
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 30
            StatusMessage = "Creating virtual machine configuration..."
        }
        Write-Output $statusUpdate
        
        # Create VM
        $vm = New-VM -Name $VMName -MemoryStartupBytes ($MemoryGB * 1GB) -VHDPath $VHDPath -Generation 2 -SwitchName $VirtualSwitch
        
        # Status update - Configuring VM
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 50
            StatusMessage = "Configuring virtual machine properties..."
        }
        Write-Output $statusUpdate
        
        # Configure VM settings
        $vm | Set-VMProcessor -Count $CPUCount
        $vm | Set-VMMemory -DynamicMemoryEnabled $true -MinimumBytes (1GB) -MaximumBytes ($MemoryGB * 1GB) -StartupBytes ($MemoryGB * 1GB)
        
        # Enable Secure Boot for Generation 2 VMs
        $vm | Set-VMFirmware -EnableSecureBoot On
        
        # Status update - Completing
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 90
            StatusMessage = "Finalizing virtual machine creation..."
        }
        Write-Output $statusUpdate
        
        # Final VM check
        $createdVM = Get-VM -Name $VMName
        
        # Status update - Complete
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 100
            StatusMessage = "Virtual machine created successfully."
        }
        Write-Output $statusUpdate
        
        # Return success result
        $result = [PSCustomObject]@{
            Type = "Result"
            Success = $true
            Message = "VM '$VMName' created successfully."
            VMName = $VMName
            VHDPath = $VHDPath
        }
        
        Write-Output $result
    }
    catch {
        Write-Error "Failed to create VM: $_"
        
        # Return error result
        $result = [PSCustomObject]@{
            Type = "Result"
            Success = $false
            Message = "Failed to create VM: $_"
            ErrorDetails = $_
        }
        
        Write-Output $result
    }
}

# Execute the VM creation
New-HyperVVM -VMName $VMName -CPUCount $CPUCount -MemoryGB $MemoryGB -StorageGB $StorageGB -VirtualSwitch $VirtualSwitch -VHDPath $VHDPath
```

### Domain Controller Configuration Example

```powershell
# DomainController.ps1
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$VMName,
    
    [Parameter(Mandatory=$true)]
    [string]$DomainName,
    
    [Parameter(Mandatory=$false)]
    [string]$NetBIOSName = $null,
    
    [Parameter(Mandatory=$false)]
    [string]$ForestFunctionalLevel = "WinThreshold",
    
    [Parameter(Mandatory=$false)]
    [string]$DomainFunctionalLevel = "WinThreshold",
    
    [Parameter(Mandatory=$true)]
    [string]$SafeModeAdministratorPassword
)

# Import common functions
. "$PSScriptRoot\..\Common\ErrorHandling.ps1"
. "$PSScriptRoot\..\Common\Logging.ps1"

function Install-ActiveDirectory {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$VMName,
        
        [Parameter(Mandatory=$true)]
        [string]$DomainName,
        
        [Parameter(Mandatory=$false)]
        [string]$NetBIOSName,
        
        [Parameter(Mandatory=$false)]
        [string]$ForestFunctionalLevel,
        
        [Parameter(Mandatory=$false)]
        [string]$DomainFunctionalLevel,
        
        [Parameter(Mandatory=$true)]
        [string]$SafeModeAdministratorPassword
    )
    
    try {
        # Status update - Starting
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 0
            StatusMessage = "Starting Active Directory installation..."
        }
        Write-Output $statusUpdate
        
        # Generate NetBIOS name if not provided
        if ([string]::IsNullOrEmpty($NetBIOSName)) {
            $NetBIOSName = $DomainName.Split('.')[0].ToUpper()
        }
        
        # Generate DSC configuration script
        $dscScriptPath = "$env:TEMP\DCConfig_$VMName.ps1"
        
        # Create the DSC configuration script
        $dscScript = @"
configuration DCConfig
{
    param
    (
        [Parameter(Mandatory=`$true)]
        [string]`$DomainName,
        
        [Parameter(Mandatory=`$true)]
        [string]`$NetBIOSName,
        
        [Parameter(Mandatory=`$true)]
        [string]`$ForestFunctionalLevel,
        
        [Parameter(Mandatory=`$true)]
        [string]`$DomainFunctionalLevel,
        
        [Parameter(Mandatory=`$true)]
        [System.Management.Automation.PSCredential]`$SafeModeAdministratorCred
    )
    
    Import-DscResource -ModuleName PSDesiredStateConfiguration
    Import-DscResource -ModuleName ActiveDirectoryDsc
    
    Node localhost
    {
        WindowsFeature ADDSInstall
        {
            Ensure = "Present"
            Name = "AD-Domain-Services"
        }
        
        WindowsFeature ADDSTools
        {
            Ensure = "Present"
            Name = "RSAT-ADDS"
            DependsOn = "[WindowsFeature]ADDSInstall"
        }
        
        ADDomain NewDomain
        {
            DomainName = `$DomainName
            DomainNetbiosName = `$NetBIOSName
            DomainAdministratorCredential = `$SafeModeAdministratorCred
            SafemodeAdministratorPassword = `$SafeModeAdministratorCred
            ForestMode = `$ForestFunctionalLevel
            DomainMode = `$DomainFunctionalLevel
            DatabasePath = "C:\\Windows\\NTDS"
            LogPath = "C:\\Windows\\NTDS"
            SysvolPath = "C:\\Windows\\SYSVOL"
            DependsOn = "[WindowsFeature]ADDSInstall"
        }
    }
}
"@
        
        # Write the DSC script to a file
        Set-Content -Path $dscScriptPath -Value $dscScript
        
        # Status update - DSC Script Created
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 20
            StatusMessage = "DSC configuration script created..."
        }
        Write-Output $statusUpdate
        
        # Convert password to secure string
        $securePassword = ConvertTo-SecureString $SafeModeAdministratorPassword -AsPlainText -Force
        $credential = New-Object System.Management.Automation.PSCredential("Administrator", $securePassword)
        
        # Create MOF file
        $mofPath = "$env:TEMP\DCConfig_$VMName"
        & $dscScriptPath -DomainName $DomainName -NetBIOSName $NetBIOSName -ForestFunctionalLevel $ForestFunctionalLevel -DomainFunctionalLevel $DomainFunctionalLevel -SafeModeAdministratorCred $credential -OutputPath $mofPath
        
        # Status update - MOF Created
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 40
            StatusMessage = "MOF configuration file created..."
        }
        Write-Output $statusUpdate
        
        # Generate VM session script
        $sessionScriptPath = "$env:TEMP\DCSession_$VMName.ps1"
        $sessionScript = @"
# Install required DSC modules
Install-PackageProvider -Name NuGet -Force
Install-Module -Name ActiveDirectoryDsc -Force

# Start DSC configuration
Start-DscConfiguration -Path '$mofPath' -Wait -Verbose -Force
"@
        
        Set-Content -Path $sessionScriptPath -Value $sessionScript
        
        # Status update - Ready for VM Session
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 60
            StatusMessage = "Ready to configure DC in VM session..."
        }
        Write-Output $statusUpdate
        
        # Here we would need to copy the script to the VM and execute it
        # In a real implementation, this would involve VM session management
        # This is a placeholder for that logic
        
        # Status update - Complete
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 100
            StatusMessage = "Active Directory installation configured successfully."
        }
        Write-Output $statusUpdate
        
        # Return success result
        $result = [PSCustomObject]@{
            Type = "Result"
            Success = $true
            Message = "Active Directory installation configured for VM '$VMName'."
            DomainName = $DomainName
            NetBIOSName = $NetBIOSName
        }
        
        Write-Output $result
    }
    catch {
        Write-Error "Failed to configure Active Directory: $_"
        
        # Return error result
        $result = [PSCustomObject]@{
            Type = "Result"
            Success = $false
            Message = "Failed to configure Active Directory: $_"
            ErrorDetails = $_
        }
        
        Write-Output $result
    }
}

# Execute the Active Directory installation
$securePassword = ConvertTo-SecureString $SafeModeAdministratorPassword -AsPlainText -Force
Install-ActiveDirectory -VMName $VMName -DomainName $DomainName -NetBIOSName $NetBIOSName -ForestFunctionalLevel $ForestFunctionalLevel -DomainFunctionalLevel $DomainFunctionalLevel -SafeModeAdministratorPassword $SafeModeAdministratorPassword
```

## PowerShell Script Testing

### Unit Testing with Pester

PowerShell scripts should be tested using the Pester framework:

```powershell
# CreateVM.Tests.ps1
Describe "VM Creation Script" {
    BeforeAll {
        # Mock functions that interact with Hyper-V
        Mock New-VHD { return @{ Path = $Path } }
        Mock New-VM { return @{ Name = $Name } }
        Mock Set-VMProcessor { return $true }
        Mock Set-VMMemory { return $true }
        Mock Set-VMFirmware { return $true }
        Mock Get-VM { return @{ Name = $VMName } }
    }
    
    Context "Basic VM Creation" {
        It "Creates a VM with default parameters" {
            # Arrange
            $params = @{
                VMName = "TestVM"
                CPUCount = 2
                MemoryGB = 4
                StorageGB = 80
                VirtualSwitch = "Default Switch"
            }
            
            # Act
            $result = & "$PSScriptRoot\..\HyperV\CreateVM.ps1" @params
            
            # Assert
            $result | Where-Object { $_.Type -eq "Result" } | Should -Not -BeNullOrEmpty
            $result | Where-Object { $_.Type -eq "Result" } | Select-Object -ExpandProperty Success | Should -Be $true
            Should -Invoke New-VHD -Times 1
            Should -Invoke New-VM -Times 1
        }
        
        It "Handles errors when VM already exists" {
            # Arrange
            $params = @{
                VMName = "ExistingVM"
                CPUCount = 2
                MemoryGB = 4
                StorageGB = 80
                VirtualSwitch = "Default Switch"
            }
            
            # Mock to simulate VM already exists
            Mock Get-VM { return @{ Name = "ExistingVM" } } -ParameterFilter { $Name -eq "ExistingVM" }
            
            # Act
            $result = & "$PSScriptRoot\..\HyperV\CreateVM.ps1" @params
            
            # Assert
            $result | Where-Object { $_.Type -eq "Result" } | Should -Not -BeNullOrEmpty
            $result | Where-Object { $_.Type -eq "Result" } | Select-Object -ExpandProperty Success | Should -Be $false
            $result | Where-Object { $_.Type -eq "Result" } | Select-Object -ExpandProperty Message | Should -Match "already exists"
        }
    }
}
```

## Best Practices Checklist

- [ ] All scripts have a consistent header with description and parameter documentation
- [ ] Scripts use [CmdletBinding()] attribute for advanced function features
- [ ] All parameters have appropriate validation attributes
- [ ] Scripts follow a consistent error handling pattern
- [ ] Scripts provide detailed progress updates
- [ ] Complex scripts are broken down into smaller, reusable functions
- [ ] Scripts are tested with Pester unit tests
- [ ] Scripts use secure parameter handling for sensitive data
- [ ] Scripts follow consistent output format for status updates and results
- [ ] Scripts have proper logging for diagnostic purposes
- [ ] Scripts are tested with Pester unit tests
- [ ] Scripts use secure parameter handling for sensitive data
- [ ] Scripts follow consistent output format for status updates and results
- [ ] Scripts have proper logging for diagnostic purposes 
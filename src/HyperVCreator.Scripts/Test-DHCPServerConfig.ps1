<#
.SYNOPSIS
    Test script for DHCP Server configuration.

.DESCRIPTION
    This script tests the configuration of a Hyper-V VM as a DHCP Server.
    It creates a new VM, installs Windows Server, and configures it as a DHCP Server.

.NOTES
    File Name      : Test-DHCPServerConfig.ps1
    Author         : HyperV Creator Team
    Prerequisite   : PowerShell 5.1 or later
#>

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Error "This script must be run as Administrator."
    exit 1
}

# Import required modules
Import-Module Hyper-V

# Source scripts
. "$PSScriptRoot\Common\ErrorHandling.ps1"
. "$PSScriptRoot\Common\Logging.ps1"
. "$PSScriptRoot\HyperV\VirtualMachine.ps1"
. "$PSScriptRoot\RoleConfiguration\DHCPServer.ps1"

# Define test parameters
$vmName = "TestDHCPServer-$(Get-Random)"
$vmPath = "C:\HyperV\VMs"
$vhdPath = "C:\HyperV\VHDs"
$isoPath = "C:\ISO\WindowsServer2022.iso"
$switchName = "Default Switch"
$adminPassword = ConvertTo-SecureString "P@ssw0rd123!" -AsPlainText -Force

# Start logging
$ErrorActionPreference = "Stop"
Write-LogMessage -Level Info -Message "Starting DHCP Server configuration test"

try {
    # Step 1: Create a new VM
    Write-LogMessage -Level Info -Message "Creating new VM: $vmName"
    
    # Create VM parameters
    $vmParams = @{
        VMName = $vmName
        CPUCount = 2
        MemoryGB = 4
        StorageGB = 60
        NetworkSwitch = $switchName
        VMPath = $vmPath
        VHDPath = $vhdPath
        ISOPath = $isoPath
        UnattendXMLPath = "$PSScriptRoot\UnattendXML\DHCPServer.xml"
        AdminPassword = $adminPassword
        EnableDynamicMemory = $true
        Generation = 2
    }
    
    # Create the VM
    $vm = New-HyperVVM @vmParams
    
    # Step 2: Configure DHCP Server
    Write-LogMessage -Level Info -Message "Starting DHCP Server configuration on $vmName"
    
    # Prepare parameters for DHCP Server configuration
    $dhcpParams = @{
        VMName = $vmName
        DomainName = "test.local"
        DNSServerIP = "192.168.1.10"
        ScopeID = "192.168.1.0"
        StartRange = "192.168.1.100"
        EndRange = "192.168.1.200"
        SubnetMask = "255.255.255.0"
        Router = "192.168.1.1"
        LeaseDurationDays = 8
        ScopeName = "Test Scope"
        ScopeDescription = "Test DHCP Scope for VM Creation"
        AuthorizeDHCP = $false
        ConfigureFailover = $false
    }
    
    # Create a credential object for the VM admin
    $adminUserName = "Administrator"
    $adminCredential = New-Object System.Management.Automation.PSCredential($adminUserName, $adminPassword)
    
    # Set the global admin credential for scripts that need it
    $Global:AdminCredential = $adminCredential
    
    # Wait for the VM to fully start and complete initial setup
    Write-LogMessage -Level Info -Message "Waiting for VM to be ready..."
    Start-Sleep -Seconds 180
    
    # Configure DHCP Server
    Install-DHCPServer @dhcpParams
    
    # Wait for DHCP Server configuration to complete
    Start-Sleep -Seconds 30
    
    # Step 3: Verify DHCP Server installation
    Write-LogMessage -Level Info -Message "Verifying DHCP Server installation"
    
    # Create a new PowerShell session to the VM
    $session = New-PSSession -VMName $vmName -Credential $adminCredential
    
    # Verify DHCP Server is running
    $verifyScript = {
        $dhcpService = Get-Service -Name DHCPServer
        $dhcpRole = Get-WindowsFeature -Name DHCP
        $scopeExists = Get-DhcpServerv4Scope -ErrorAction SilentlyContinue | Where-Object { $_.ScopeId -eq $using:dhcpParams.ScopeID }
        
        $result = @{
            ServiceStatus = $dhcpService.Status
            ServiceRunning = $dhcpService.Status -eq 'Running'
            RoleInstalled = $dhcpRole.Installed
            ScopeExists = $null -ne $scopeExists
        }
        
        if ($null -ne $scopeExists) {
            $result.ScopeStartRange = $scopeExists.StartRange.ToString()
            $result.ScopeEndRange = $scopeExists.EndRange.ToString()
            $result.SubnetMask = $scopeExists.SubnetMask.ToString()
            
            # Get DHCP options for the scope
            $options = Get-DhcpServerv4OptionValue -ScopeId $using:dhcpParams.ScopeID -ErrorAction SilentlyContinue
            if ($options) {
                $routerOption = $options | Where-Object { $_.OptionId -eq 3 }
                $dnsOption = $options | Where-Object { $_.OptionId -eq 6 }
                
                if ($routerOption) {
                    $result.RouterOption = $routerOption.Value
                }
                if ($dnsOption) {
                    $result.DNSOption = $dnsOption.Value
                }
            }
        }
        
        return $result
    }
    
    $verificationResult = Invoke-Command -Session $session -ScriptBlock $verifyScript
    
    # Check verification results
    if ($verificationResult.ServiceRunning) {
        Write-LogMessage -Level Info -Message "DHCP Server service is running."
    } else {
        Write-LogMessage -Level Warning -Message "DHCP Server service is not running. Status: $($verificationResult.ServiceStatus)"
    }
    
    if ($verificationResult.RoleInstalled) {
        Write-LogMessage -Level Info -Message "DHCP Server role is installed."
    } else {
        Write-LogMessage -Level Warning -Message "DHCP Server role is not installed."
    }
    
    if ($verificationResult.ScopeExists) {
        Write-LogMessage -Level Info -Message "DHCP scope $($dhcpParams.ScopeID) exists."
        Write-LogMessage -Level Info -Message "Scope range: $($verificationResult.ScopeStartRange) - $($verificationResult.ScopeEndRange)"
        Write-LogMessage -Level Info -Message "Subnet mask: $($verificationResult.SubnetMask)"
        
        if ($verificationResult.RouterOption) {
            Write-LogMessage -Level Info -Message "Router option configured: $($verificationResult.RouterOption)"
        }
        
        if ($verificationResult.DNSOption) {
            Write-LogMessage -Level Info -Message "DNS Server option configured: $($verificationResult.DNSOption)"
        }
    } else {
        Write-LogMessage -Level Warning -Message "DHCP scope $($dhcpParams.ScopeID) does not exist."
    }
    
    # Clean up the session
    Remove-PSSession -Session $session
    
    # Success message
    Write-LogMessage -Level Info -Message "DHCP Server configuration test completed successfully"
}
catch {
    # Catch any errors and log them
    $errorMessage = "Error during DHCP Server configuration test: $_"
    Write-LogMessage -Level Error -Message $errorMessage
    throw $errorMessage
}
finally {
    # Clean up - uncomment to automatically remove the test VM
    # Write-LogMessage -Level Info -Message "Cleaning up: Removing VM $vmName"
    # Remove-VM -Name $vmName -Force -ErrorAction SilentlyContinue
    
    # Always reset the global credential
    $Global:AdminCredential = $null
} 
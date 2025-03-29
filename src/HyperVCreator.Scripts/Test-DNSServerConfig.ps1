<#
.SYNOPSIS
    Test script for DNS Server configuration.

.DESCRIPTION
    This script tests the configuration of a Hyper-V VM as a DNS Server.
    It creates a new VM, installs Windows Server, and configures it as a DNS Server.

.NOTES
    File Name      : Test-DNSServerConfig.ps1
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
. "$PSScriptRoot\RoleConfiguration\DNSServer.ps1"

# Define test parameters
$vmName = "TestDNSServer-$(Get-Random)"
$vmPath = "C:\HyperV\VMs"
$vhdPath = "C:\HyperV\VHDs"
$isoPath = "C:\ISO\WindowsServer2022.iso"
$switchName = "Default Switch"
$adminPassword = ConvertTo-SecureString "P@ssw0rd123!" -AsPlainText -Force

# Start logging
$ErrorActionPreference = "Stop"
Write-LogMessage -Level Info -Message "Starting DNS Server configuration test"

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
        UnattendXMLPath = "$PSScriptRoot\UnattendXML\DNSServer.xml"
        AdminPassword = $adminPassword
        EnableDynamicMemory = $true
        Generation = 2
    }
    
    # Create the VM
    $vm = New-HyperVVM @vmParams
    
    # Step 2: Configure DNS Server
    Write-LogMessage -Level Info -Message "Starting DNS Server configuration on $vmName"
    
    # Prepare parameters for DNS Server configuration
    $dnsParams = @{
        VMName = $vmName
        PrimaryZone = "test.local"
        NetworkID = "192.168.1.0/24"
        DNSForwarders = @("8.8.8.8", "8.8.4.4")
        ZoneType = "Primary"
        StorageType = "File"
        EnableRecursion = $true
        AllowZoneTransfer = "NoTransfer"
        EnableDNSSEC = $false
        EnableGlobalQueryBlockList = $true
        CreateDefaultRecords = $true
        DefaultRecords = @{
            "server1" = "192.168.1.10"
            "server2" = "192.168.1.11"
            "www" = "192.168.1.20"
            "mail" = "192.168.1.25"
        }
    }
    
    # Create a credential object for the VM admin
    $adminUserName = "Administrator"
    $adminCredential = New-Object System.Management.Automation.PSCredential($adminUserName, $adminPassword)
    
    # Set the global admin credential for scripts that need it
    $Global:AdminCredential = $adminCredential
    
    # Wait for the VM to fully start and complete initial setup
    Write-LogMessage -Level Info -Message "Waiting for VM to be ready..."
    Start-Sleep -Seconds 180
    
    # Configure DNS Server
    Install-DNSServer @dnsParams
    
    # Wait for DNS Server configuration to complete
    Start-Sleep -Seconds 30
    
    # Step 3: Verify DNS Server installation
    Write-LogMessage -Level Info -Message "Verifying DNS Server installation"
    
    # Create a new PowerShell session to the VM
    $session = New-PSSession -VMName $vmName -Credential $adminCredential
    
    # Store primary zone for use in script block
    $primaryZone = $dnsParams.PrimaryZone
    
    # Verify DNS Server is running
    $verifyScript = {
        param (
            [string]$PrimaryZoneName
        )
        
        $dnsService = Get-Service -Name DNS
        $dnsRole = Get-WindowsFeature -Name DNS-Server
        $zones = Get-DnsServerZone -ErrorAction SilentlyContinue
        
        $result = @{
            ServiceStatus = $dnsService.Status
            ServiceRunning = $dnsService.Status -eq 'Running'
            RoleInstalled = $dnsRole.Installed
            ZonesCount = $zones.Count
            Zones = @()
        }
        
        if ($zones) {
            foreach ($zone in $zones) {
                # Skip built-in zones
                if ($zone.ZoneName -notin @("TrustAnchors", ".", "0.in-addr.arpa", "127.in-addr.arpa", "255.in-addr.arpa")) {
                    $zoneInfo = @{
                        Name = $zone.ZoneName
                        Type = $zone.ZoneType
                        Records = @()
                    }
                    
                    # Get records for the zone
                    if ($zone.ZoneName -eq $PrimaryZoneName) {
                        $records = Get-DnsServerResourceRecord -ZoneName $zone.ZoneName -ErrorAction SilentlyContinue | 
                            Where-Object { $_.RecordType -eq "A" -and $_.HostName -ne "@" }
                        
                        if ($records) {
                            foreach ($record in $records) {
                                $recordInfo = @{
                                    HostName = $record.HostName
                                    RecordType = $record.RecordType
                                    Data = if ($record.RecordData.IPv4Address) { $record.RecordData.IPv4Address.ToString() } else { "" }
                                }
                                $zoneInfo.Records += $recordInfo
                            }
                        }
                    }
                    
                    $result.Zones += $zoneInfo
                }
            }
        }
        
        # Check DNS settings
        $recursion = Get-DnsServerRecursion
        $forwarders = Get-DnsServerForwarder
        
        $result.RecursionEnabled = $recursion.Enable
        $result.Forwarders = @($forwarders.IPAddress | ForEach-Object { $_.IPAddressToString })
        
        return $result
    }
    
    $verificationResult = Invoke-Command -Session $session -ScriptBlock $verifyScript -ArgumentList $primaryZone
    
    # Check verification results
    if ($verificationResult.ServiceRunning) {
        Write-LogMessage -Level Info -Message "DNS Server service is running."
    } else {
        Write-LogMessage -Level Warning -Message "DNS Server service is not running. Status: $($verificationResult.ServiceStatus)"
    }
    
    if ($verificationResult.RoleInstalled) {
        Write-LogMessage -Level Info -Message "DNS Server role is installed."
    } else {
        Write-LogMessage -Level Warning -Message "DNS Server role is not installed."
    }
    
    # Check zones
    Write-LogMessage -Level Info -Message "Found $($verificationResult.ZonesCount) total DNS zones."
    
    foreach ($zone in $verificationResult.Zones) {
        Write-LogMessage -Level Info -Message "Zone: $($zone.Name) (Type: $($zone.Type))"
        
        if ($zone.Records.Count -gt 0) {
            Write-LogMessage -Level Info -Message "Records in zone $($zone.Name):"
            foreach ($record in $zone.Records) {
                Write-LogMessage -Level Info -Message "  $($record.HostName) ($($record.RecordType)): $($record.Data)"
            }
        } else {
            Write-LogMessage -Level Info -Message "No records found in zone $($zone.Name)."
        }
    }
    
    # Check DNS settings
    if ($verificationResult.RecursionEnabled) {
        Write-LogMessage -Level Info -Message "DNS recursion is enabled."
    } else {
        Write-LogMessage -Level Warning -Message "DNS recursion is disabled."
    }
    
    if ($verificationResult.Forwarders.Count -gt 0) {
        Write-LogMessage -Level Info -Message "DNS forwarders configured: $($verificationResult.Forwarders -join ', ')"
    } else {
        Write-LogMessage -Level Warning -Message "No DNS forwarders configured."
    }
    
    # Test DNS resolution
    $defaultRecords = $dnsParams.DefaultRecords

    $testDNSScript = {
        param (
            [string]$DnsZone,
            [hashtable]$RecordsToTest
        )
        try {
            # Test resolution of records we created
            $testResults = @()
            
            foreach ($record in $RecordsToTest.GetEnumerator()) {
                $hostName = "$($record.Key).$DnsZone"
                try {
                    $resolved = Resolve-DnsName -Name $hostName -Type A -ErrorAction Stop
                    $testResults += @{
                        HostName = $hostName
                        ExpectedIP = $record.Value
                        ResolvedIP = if ($resolved.IPAddress) { $resolved.IPAddress } else { "Not resolved" }
                        Success = $resolved.IPAddress -eq $record.Value
                    }
                }
                catch {
                    $testResults += @{
                        HostName = $hostName
                        ExpectedIP = $record.Value
                        ResolvedIP = "Failed to resolve"
                        Success = $false
                    }
                }
            }
            
            # Also test external name resolution to verify forwarders
            try {
                $externalTest = Resolve-DnsName -Name "www.microsoft.com" -Type A -ErrorAction Stop
                $testResults += @{
                    HostName = "www.microsoft.com"
                    ExpectedIP = "External IP"
                    ResolvedIP = if ($externalTest.IPAddress) { "Resolved successfully" } else { "Not resolved" }
                    Success = $null -ne $externalTest.IPAddress
                }
            }
            catch {
                $testResults += @{
                    HostName = "www.microsoft.com"
                    ExpectedIP = "External IP"
                    ResolvedIP = "Failed to resolve"
                    Success = $false
                }
            }
            
            return $testResults
        }
        catch {
            return @{
                Error = $_
            }
        }
    }

    $dnsTestResults = Invoke-Command -Session $session -ScriptBlock $testDNSScript -ArgumentList $primaryZone, $defaultRecords
    
    if ($dnsTestResults -and $dnsTestResults.GetType().Name -ne "String") {
        Write-LogMessage -Level Info -Message "DNS resolution test results:"
        foreach ($result in $dnsTestResults) {
            $status = if ($result.Success) { "Success" } else { "Failed" }
            Write-LogMessage -Level Info -Message "  $($result.HostName): $($result.ResolvedIP) - $status"
        }
    } else {
        Write-LogMessage -Level Warning -Message "DNS resolution test failed: $dnsTestResults"
    }
    
    # Clean up the session
    Remove-PSSession -Session $session
    
    # Success message
    Write-LogMessage -Level Info -Message "DNS Server configuration test completed successfully"
}
catch {
    # Catch any errors and log them
    $errorMessage = "Error during DNS Server configuration test: $_"
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
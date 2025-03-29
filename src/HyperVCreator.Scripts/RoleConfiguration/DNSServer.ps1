<#
.SYNOPSIS
    Configures a Hyper-V VM as a DNS Server.

.DESCRIPTION
    This script installs and configures the DNS Server role on a Windows Server VM.
    It handles the installation of prerequisites, DNS Server role, and post-installation
    configuration including DNS zones, records, and forwarders.

.PARAMETER VMName
    Name of the VM to configure.

.PARAMETER PrimaryZone
    Primary forward lookup zone name to create (e.g., contoso.local).

.PARAMETER ReverseZone
    Reverse lookup zone to create (e.g., 1.168.192.in-addr.arpa).

.PARAMETER NetworkID
    Network ID for the reverse lookup zone (e.g., 192.168.1.0/24).

.PARAMETER DNSForwarders
    Array of DNS forwarders (e.g., @("8.8.8.8", "8.8.4.4")).

.PARAMETER ZoneType
    Type of zone to create. Default is "Primary". Options are "Primary", "Secondary", or "Stub".

.PARAMETER StorageType
    Storage type for DNS zones. Default is "File". Options are "File" or "AD".

.PARAMETER EnableRecursion
    Whether to enable recursion. Default is $true.

.PARAMETER AllowZoneTransfer
    Zone transfer setting. Default is "NoTransfer". Options are "NoTransfer", "ToAnyServer", "ToZoneNameServers", or "ToSecondaryServers".

.PARAMETER EnableDNSSEC
    Whether to enable DNSSEC. Default is $false.

.PARAMETER EnableGlobalQueryBlockList
    Whether to enable the global query block list. Default is $true.

.PARAMETER CreateDefaultRecords
    Whether to create default DNS records. Default is $true.

.PARAMETER DefaultRecords
    Hashtable of record name:IP address pairs to create in the primary zone.

.NOTES
    File Name      : DNSServer.ps1
    Author         : HyperV Creator Team
    Prerequisite   : PowerShell 5.1 or later
#>

# Source common functions
. "$PSScriptRoot\..\Common\ErrorHandling.ps1"
. "$PSScriptRoot\..\Common\Logging.ps1"
. "$PSScriptRoot\..\Common\Validation.ps1"
. "$PSScriptRoot\..\Monitoring\TrackProgress.ps1"

function Install-DNSServer {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$VMName,
        
        [Parameter(Mandatory=$true)]
        [string]$PrimaryZone,
        
        [Parameter(Mandatory=$false)]
        [string]$ReverseZone,
        
        [Parameter(Mandatory=$false)]
        [string]$NetworkID,
        
        [Parameter(Mandatory=$false)]
        [string[]]$DNSForwarders = @("8.8.8.8", "8.8.4.4"),
        
        [Parameter(Mandatory=$false)]
        [ValidateSet("Primary", "Secondary", "Stub")]
        [string]$ZoneType = "Primary",
        
        [Parameter(Mandatory=$false)]
        [ValidateSet("File", "AD")]
        [string]$StorageType = "File",
        
        [Parameter(Mandatory=$false)]
        [bool]$EnableRecursion = $true,
        
        [Parameter(Mandatory=$false)]
        [ValidateSet("NoTransfer", "ToAnyServer", "ToZoneNameServers", "ToSecondaryServers")]
        [string]$AllowZoneTransfer = "NoTransfer",
        
        [Parameter(Mandatory=$false)]
        [bool]$EnableDNSSEC = $false,
        
        [Parameter(Mandatory=$false)]
        [bool]$EnableGlobalQueryBlockList = $true,
        
        [Parameter(Mandatory=$false)]
        [bool]$CreateDefaultRecords = $true,
        
        [Parameter(Mandatory=$false)]
        [hashtable]$DefaultRecords = @{}
    )
    
    begin {
        Write-LogMessage -Level Info -Message "Starting DNS Server configuration for VM: $VMName"
        Start-TrackingOperation -OperationName "ConfigureDNSServer" -TotalSteps 5
    }
    
    process {
        try {
            # Validate parameters
            $domainValid = Test-DomainName -DomainName $PrimaryZone
            if (-not $domainValid.IsValid) {
                throw "Invalid primary zone: $($domainValid.Message)"
            }
            
            if (-not [string]::IsNullOrEmpty($ReverseZone)) {
                # Basic validation for reverse zone format
                if (-not $ReverseZone.EndsWith('.in-addr.arpa')) {
                    throw "Reverse zone should end with .in-addr.arpa"
                }
            }
            
            if (-not [string]::IsNullOrEmpty($NetworkID)) {
                # Basic validation for network ID format (e.g., 192.168.1.0/24)
                if (-not $NetworkID.Contains('/')) {
                    throw "Network ID should be in CIDR notation (e.g., 192.168.1.0/24)"
                }
                
                $parts = $NetworkID.Split('/')
                $ipPart = $parts[0]
                $maskPart = $parts[1]
                
                $ipValid = Test-IPAddress -IPAddress $ipPart
                if (-not $ipValid.IsValid) {
                    throw "Invalid network address in NetworkID: $ipPart"
                }
                
                if (-not [int]::TryParse($maskPart, [ref]$null) -or [int]$maskPart -lt 0 -or [int]$maskPart -gt 32) {
                    throw "Invalid subnet mask in NetworkID: $maskPart"
                }
            }
            
            foreach ($forwarder in $DNSForwarders) {
                $ipValid = Test-IPAddress -IPAddress $forwarder
                if (-not $ipValid.IsValid) {
                    throw "Invalid DNS forwarder IP address: $forwarder"
                }
            }
            
            # Step 1: Ensure VM is running
            Update-OperationProgress -StepNumber 1 -StepDescription "Verifying VM is running"
            $vm = Get-VM -Name $VMName -ErrorAction Stop
            if ($vm.State -ne 'Running') {
                Write-LogMessage -Level Warning -Message "VM $VMName is not running. Starting VM..."
                Start-VM -Name $VMName -ErrorAction Stop
                # Wait for VM to start
                $timeout = 300
                $timer = [Diagnostics.Stopwatch]::StartNew()
                while ($vm.State -ne 'Running' -and $timer.Elapsed.TotalSeconds -lt $timeout) {
                    Start-Sleep -Seconds 5
                    $vm = Get-VM -Name $VMName -ErrorAction Stop
                }
                if ($vm.State -ne 'Running') {
                    throw "Failed to start VM $VMName within timeout period."
                }
            }
            
            # Establish a PowerShell session to the VM
            $session = New-PSSession -VMName $VMName -Credential $AdminCredential
            
            # Step 2: Install DNS Server role
            Update-OperationProgress -StepNumber 2 -StepDescription "Installing DNS Server role"
            
            $installDNSRoleScript = {
                Install-WindowsFeature -Name DNS -IncludeManagementTools
            }
            
            $installResult = Invoke-Command -Session $session -ScriptBlock $installDNSRoleScript
            
            if (-not $installResult.Success) {
                throw "Failed to install DNS Server role: $($installResult.ExitCode)"
            }
            
            # Step 3: Configure DNS Server
            Update-OperationProgress -StepNumber 3 -StepDescription "Configuring DNS Server settings"
            
            $configureDNSScript = {
                param (
                    [bool]$EnableRecursion,
                    [string[]]$DNSForwarders,
                    [bool]$EnableDNSSEC,
                    [bool]$EnableGlobalQueryBlockList
                )
                
                try {
                    # Configure DNS Server settings
                    Set-DnsServerRecursion -Enable $EnableRecursion
                    
                    # Set forwarders
                    if ($DNSForwarders.Count -gt 0) {
                        $forwarders = $DNSForwarders | ForEach-Object { [System.Net.IPAddress]$_ }
                        Set-DnsServerForwarder -IPAddress $forwarders -UseRootHint $false
                    }
                    
                    # Configure DNSSEC settings
                    if ($EnableDNSSEC) {
                        Set-DnsServerDnsSecZoneSetting -EnableDnsSec $true -ZoneName "." -SigningStateSetting DoNotSign
                    }
                    
                    # Configure global query block list
                    if ($EnableGlobalQueryBlockList) {
                        Set-DnsServerGlobalQueryBlockList -Enable $true
                    } else {
                        Set-DnsServerGlobalQueryBlockList -Enable $false
                    }
                    
                    return @{
                        Success = $true
                    }
                }
                catch {
                    return @{
                        Success = $false
                        Error = $_
                    }
                }
            }
            
            $dnsConfigParams = @{
                EnableRecursion = $EnableRecursion
                DNSForwarders = $DNSForwarders
                EnableDNSSEC = $EnableDNSSEC
                EnableGlobalQueryBlockList = $EnableGlobalQueryBlockList
            }
            
            $configResult = Invoke-Command -Session $session -ScriptBlock $configureDNSScript -ArgumentList $dnsConfigParams
            
            if (-not $configResult.Success) {
                throw "Failed to configure DNS Server: $($configResult.Error)"
            }
            
            # Step 4: Create DNS zones
            Update-OperationProgress -StepNumber 4 -StepDescription "Creating DNS zones"
            
            # Create primary forward lookup zone
            $createPrimaryZoneScript = {
                param (
                    [string]$PrimaryZone,
                    [string]$ZoneType,
                    [string]$StorageType,
                    [string]$AllowZoneTransfer
                )
                
                try {
                    # Check if zone already exists
                    $existingZone = Get-DnsServerZone -Name $PrimaryZone -ErrorAction SilentlyContinue
                    
                    if ($null -eq $existingZone) {
                        # Create the zone based on ZoneType and StorageType
                        if ($ZoneType -eq "Primary") {
                            if ($StorageType -eq "File") {
                                Add-DnsServerPrimaryZone -Name $PrimaryZone -ZoneFile "$PrimaryZone.dns" -DynamicUpdate None
                            } else {
                                # Active Directory integrated
                                $ADIntegration = "Forest"
                                Add-DnsServerPrimaryZone -Name $PrimaryZone -ReplicationScope $ADIntegration -DynamicUpdate Secure
                            }
                        } elseif ($ZoneType -eq "Secondary") {
                            # For secondary zones, we would need to specify master servers
                            # This is a placeholder for secondary zone creation
                            throw "Secondary zone type requires master server addresses"
                        } elseif ($ZoneType -eq "Stub") {
                            # For stub zones, we would need to specify master servers
                            # This is a placeholder for stub zone creation
                            throw "Stub zone type requires master server addresses"
                        }
                        
                        # Configure zone transfer settings
                        $zoneTransferType = switch ($AllowZoneTransfer) {
                            "NoTransfer" { "None" }
                            "ToAnyServer" { "Any" }
                            "ToZoneNameServers" { "NameServer" }
                            "ToSecondaryServers" { "Server" }
                            default { "None" }
                        }
                        
                        Set-DnsServerPrimaryZone -Name $PrimaryZone -ZoneFile "$PrimaryZone.dns" -Notify $zoneTransferType -NotifyServers @() -SecureSecondaries $zoneTransferType
                        
                        return @{
                            Success = $true
                            Message = "Created primary zone $PrimaryZone"
                        }
                    } else {
                        return @{
                            Success = $true
                            Message = "Zone $PrimaryZone already exists"
                        }
                    }
                }
                catch {
                    return @{
                        Success = $false
                        Error = $_
                    }
                }
            }
            
            $primaryZoneParams = @{
                PrimaryZone = $PrimaryZone
                ZoneType = $ZoneType
                StorageType = $StorageType
                AllowZoneTransfer = $AllowZoneTransfer
            }
            
            $primaryZoneResult = Invoke-Command -Session $session -ScriptBlock $createPrimaryZoneScript -ArgumentList $primaryZoneParams
            
            if (-not $primaryZoneResult.Success) {
                throw "Failed to create primary zone: $($primaryZoneResult.Error)"
            }
            
            Write-LogMessage -Level Info -Message $primaryZoneResult.Message
            
            # Create reverse lookup zone if specified
            if (-not [string]::IsNullOrEmpty($NetworkID)) {
                $createReverseZoneScript = {
                    param (
                        [string]$NetworkID,
                        [string]$ReverseZone,
                        [string]$StorageType
                    )
                    
                    try {
                        # Check if zone already exists
                        $existingZone = Get-DnsServerZone -Name $ReverseZone -ErrorAction SilentlyContinue
                        
                        if ($null -eq $existingZone) {
                            # Convert network ID to reverse zone format if not provided
                            if ([string]::IsNullOrEmpty($ReverseZone)) {
                                $parts = $NetworkID.Split('/')
                                $ipPart = $parts[0]
                                $prefixLength = [int]$parts[1]
                                
                                $ipBytes = $ipPart.Split('.')
                                
                                # Determine which octets to include based on subnet mask
                                $octetCount = [Math]::Ceiling($prefixLength / 8)
                                
                                # Build reverse zone name
                                $reverseOctets = [System.Collections.ArrayList]@()
                                for ($i = 0; $i -lt $octetCount; $i++) {
                                    $reverseOctets.Add($ipBytes[$i]) | Out-Null
                                }
                                
                                $reverseOctets.Reverse()
                                $ReverseZone = "$($reverseOctets -join '.').in-addr.arpa"
                            }
                            
                            # Create the reverse zone
                            if ($StorageType -eq "File") {
                                Add-DnsServerPrimaryZone -NetworkID $NetworkID -ZoneFile "$ReverseZone.dns" -DynamicUpdate None
                            } else {
                                # Active Directory integrated
                                $ADIntegration = "Forest"
                                Add-DnsServerPrimaryZone -NetworkID $NetworkID -ReplicationScope $ADIntegration -DynamicUpdate Secure
                            }
                            
                            return @{
                                Success = $true
                                Message = "Created reverse zone $ReverseZone"
                                ReversZoneName = $ReverseZone
                            }
                        } else {
                            return @{
                                Success = $true
                                Message = "Reverse zone $ReverseZone already exists"
                                ReversZoneName = $ReverseZone
                            }
                        }
                    }
                    catch {
                        return @{
                            Success = $false
                            Error = $_
                        }
                    }
                }
                
                $reverseZoneParams = @{
                    NetworkID = $NetworkID
                    ReverseZone = $ReverseZone
                    StorageType = $StorageType
                }
                
                $reverseZoneResult = Invoke-Command -Session $session -ScriptBlock $createReverseZoneScript -ArgumentList $reverseZoneParams
                
                if (-not $reverseZoneResult.Success) {
                    throw "Failed to create reverse zone: $($reverseZoneResult.Error)"
                }
                
                Write-LogMessage -Level Info -Message $reverseZoneResult.Message
                
                # Update ReverseZone variable if it was automatically generated
                if ([string]::IsNullOrEmpty($ReverseZone)) {
                    $ReverseZone = $reverseZoneResult.ReversZoneName
                }
            }
            
            # Step 5: Create default DNS records
            Update-OperationProgress -StepNumber 5 -StepDescription "Creating DNS records"
            
            if ($CreateDefaultRecords) {
                $createRecordsScript = {
                    param (
                        [string]$PrimaryZone,
                        [hashtable]$DefaultRecords
                    )
                    
                    try {
                        $results = @()
                        
                        # Create A records from the hashtable
                        foreach ($record in $DefaultRecords.GetEnumerator()) {
                            $name = $record.Key
                            $ipAddress = $record.Value
                            
                            # Attempt to create the A record
                            try {
                                Add-DnsServerResourceRecordA -ZoneName $PrimaryZone -Name $name -IPv4Address $ipAddress -ErrorAction Stop
                                $results += @{
                                    Name = $name
                                    IPAddress = $ipAddress
                                    Result = "Created"
                                }
                            }
                            catch {
                                $results += @{
                                    Name = $name
                                    IPAddress = $ipAddress
                                    Result = "Failed: $_"
                                }
                            }
                        }
                        
                        # Get the server's IP address
                        $serverIP = (Get-NetIPAddress -AddressFamily IPv4 -InterfaceAlias "Ethernet*").IPAddress
                        
                        # Add the DNS server's own record if it doesn't exist in the default records
                        if (-not $DefaultRecords.ContainsKey("dns") -and $serverIP) {
                            try {
                                Add-DnsServerResourceRecordA -ZoneName $PrimaryZone -Name "dns" -IPv4Address $serverIP -ErrorAction Stop
                                $results += @{
                                    Name = "dns"
                                    IPAddress = $serverIP
                                    Result = "Created"
                                }
                            }
                            catch {
                                $results += @{
                                    Name = "dns"
                                    IPAddress = $serverIP
                                    Result = "Failed: $_"
                                }
                            }
                        }
                        
                        return @{
                            Success = $true
                            Results = $results
                        }
                    }
                    catch {
                        return @{
                            Success = $false
                            Error = $_
                        }
                    }
                }
                
                $recordsParams = @{
                    PrimaryZone = $PrimaryZone
                    DefaultRecords = $DefaultRecords
                }
                
                $recordsResult = Invoke-Command -Session $session -ScriptBlock $createRecordsScript -ArgumentList $recordsParams
                
                if (-not $recordsResult.Success) {
                    throw "Failed to create DNS records: $($recordsResult.Error)"
                }
                
                # Log record creation results
                foreach ($result in $recordsResult.Results) {
                    Write-LogMessage -Level Info -Message "DNS record: $($result.Name) -> $($result.IPAddress): $($result.Result)"
                }
            }
            
            # Verify DNS Server is running
            $verifyDNSScript = {
                $dnsService = Get-Service -Name DNS
                $dnsRole = Get-WindowsFeature -Name DNS-Server
                $forwardZone = Get-DnsServerZone -Name $using:PrimaryZone -ErrorAction SilentlyContinue
                $reverseZone = if ($using:ReverseZone) { Get-DnsServerZone -Name $using:ReverseZone -ErrorAction SilentlyContinue } else { $null }
                
                return @{
                    ServiceRunning = $dnsService.Status -eq 'Running'
                    RoleInstalled = $dnsRole.Installed
                    ForwardZoneExists = $null -ne $forwardZone
                    ReverseZoneExists = $null -ne $reverseZone
                }
            }
            
            $verifyResult = Invoke-Command -Session $session -ScriptBlock $verifyDNSScript
            
            if (-not $verifyResult.ServiceRunning) {
                Write-LogMessage -Level Warning -Message "DNS service is not running. Attempting to start it..."
                
                Invoke-Command -Session $session -ScriptBlock {
                    Start-Service -Name DNS
                }
            }
            
            if (-not $verifyResult.ForwardZoneExists) {
                Write-LogMessage -Level Warning -Message "Forward zone $PrimaryZone does not exist or could not be verified."
            }
            
            if ($ReverseZone -and -not $verifyResult.ReverseZoneExists) {
                Write-LogMessage -Level Warning -Message "Reverse zone $ReverseZone does not exist or could not be verified."
            }
            
            Update-OperationProgress -StepNumber 5 -StepDescription "DNS Server configuration complete" -Completed $true
            Write-LogMessage -Level Info -Message "DNS Server configuration completed successfully for VM: $VMName"
        }
        catch {
            $errorMessage = "Failed to configure DNS Server: $_"
            Write-LogMessage -Level Error -Message $errorMessage
            throw $errorMessage
        }
        finally {
            # Clean up PSSession if it exists
            if ($session) {
                Remove-PSSession -Session $session -ErrorAction SilentlyContinue
            }
        }
    }
    
    end {
        Complete-TrackingOperation -OperationName "ConfigureDNSServer"
    }
}

Export-ModuleMember -Function Install-DNSServer 
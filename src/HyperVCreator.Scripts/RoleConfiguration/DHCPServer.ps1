<#
.SYNOPSIS
    Configures a Hyper-V VM as a DHCP Server.

.DESCRIPTION
    This script installs and configures the DHCP Server role on a Windows Server VM.
    It handles the installation of prerequisites, DHCP Server role, and post-installation
    configuration including scopes and options.

.PARAMETER VMName
    Name of the VM to configure.

.PARAMETER DomainName
    Domain name to use for DHCP configuration (e.g., contoso.local).

.PARAMETER DNSServerIP
    IP address of the DNS server to be used by DHCP clients.

.PARAMETER ScopeID
    Scope ID for the DHCP scope (e.g., 192.168.1.0).

.PARAMETER StartRange
    Start range for DHCP scope (e.g., 192.168.1.100).

.PARAMETER EndRange
    End range for DHCP scope (e.g., 192.168.1.200).

.PARAMETER SubnetMask
    Subnet mask for DHCP scope (e.g., 255.255.255.0).

.PARAMETER Router
    Default gateway for DHCP clients.

.PARAMETER LeaseDurationDays
    Lease duration in days. Default is 8.

.PARAMETER ScopeName
    Friendly name for the DHCP scope. Default is "Default Scope".

.PARAMETER ScopeDescription
    Description for the DHCP scope. Default is "Default DHCP Scope".

.PARAMETER AuthorizeDHCP
    Whether to authorize the DHCP server in Active Directory. Default is $false.

.PARAMETER ConfigureFailover
    Whether to configure DHCP failover. Default is $false.

.PARAMETER FailoverPartnerServer
    FQDN of failover partner DHCP server.

.PARAMETER FailoverScopePercent
    Percentage of addresses to allocate to primary server. Default is 50.

.NOTES
    File Name      : DHCPServer.ps1
    Author         : HyperV Creator Team
    Prerequisite   : PowerShell 5.1 or later
#>

# Source common functions
. "$PSScriptRoot\..\Common\ErrorHandling.ps1"
. "$PSScriptRoot\..\Common\Logging.ps1"
. "$PSScriptRoot\..\Common\Validation.ps1"
. "$PSScriptRoot\..\Monitoring\TrackProgress.ps1"

function Install-DHCPServer {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$VMName,
        
        [Parameter(Mandatory=$true)]
        [string]$DomainName,
        
        [Parameter(Mandatory=$true)]
        [string]$DNSServerIP,
        
        [Parameter(Mandatory=$true)]
        [string]$ScopeID,
        
        [Parameter(Mandatory=$true)]
        [string]$StartRange,
        
        [Parameter(Mandatory=$true)]
        [string]$EndRange,
        
        [Parameter(Mandatory=$true)]
        [string]$SubnetMask,
        
        [Parameter(Mandatory=$true)]
        [string]$Router,
        
        [Parameter(Mandatory=$false)]
        [int]$LeaseDurationDays = 8,
        
        [Parameter(Mandatory=$false)]
        [string]$ScopeName = "Default Scope",
        
        [Parameter(Mandatory=$false)]
        [string]$ScopeDescription = "Default DHCP Scope",
        
        [Parameter(Mandatory=$false)]
        [bool]$AuthorizeDHCP = $false,
        
        [Parameter(Mandatory=$false)]
        [bool]$ConfigureFailover = $false,
        
        [Parameter(Mandatory=$false)]
        [string]$FailoverPartnerServer,
        
        [Parameter(Mandatory=$false)]
        [int]$FailoverScopePercent = 50
    )
    
    begin {
        Write-LogMessage -Level Info -Message "Starting DHCP Server configuration for VM: $VMName"
        Start-TrackingOperation -OperationName "ConfigureDHCPServer" -TotalSteps 5
    }
    
    process {
        try {
            # Validate parameters
            $ipValid = Test-IPAddress -IPAddress $DNSServerIP
            if (-not $ipValid.IsValid) {
                throw "Invalid DNS server IP address: $DNSServerIP"
            }
            
            $ipValid = Test-IPAddress -IPAddress $ScopeID
            if (-not $ipValid.IsValid) {
                throw "Invalid Scope ID: $ScopeID"
            }
            
            $ipValid = Test-IPAddress -IPAddress $StartRange
            if (-not $ipValid.IsValid) {
                throw "Invalid Start Range: $StartRange"
            }
            
            $ipValid = Test-IPAddress -IPAddress $EndRange
            if (-not $ipValid.IsValid) {
                throw "Invalid End Range: $EndRange"
            }
            
            $ipValid = Test-IPAddress -IPAddress $SubnetMask
            if (-not $ipValid.IsValid) {
                throw "Invalid Subnet Mask: $SubnetMask"
            }
            
            $ipValid = Test-IPAddress -IPAddress $Router
            if (-not $ipValid.IsValid) {
                throw "Invalid Router IP: $Router"
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
            
            # Step 2: Install DHCP Server role
            Update-OperationProgress -StepNumber 2 -StepDescription "Installing DHCP Server role"
            
            $installDHCPRoleScript = {
                Install-WindowsFeature -Name DHCP -IncludeManagementTools
            }
            
            $installResult = Invoke-Command -Session $session -ScriptBlock $installDHCPRoleScript
            
            if (-not $installResult.Success) {
                throw "Failed to install DHCP Server role: $($installResult.ExitCode)"
            }
            
            # Step 3: Configure DHCP Server
            Update-OperationProgress -StepNumber 3 -StepDescription "Configuring DHCP Server"
            
            $configureDHCPScript = {
                param (
                    [string]$DomainName
                )
                
                # Add DHCP Management security groups
                netsh dhcp add securitygroups
                
                # Restart the DHCP service
                Restart-Service dhcpserver
                
                # Configure DHCP server settings
                Set-DhcpServerv4Binding -BindingState $true -InterfaceAlias "Ethernet*"
                
                # Set server level DNS settings
                Set-DhcpServerv4OptionValue -DnsDomain $DomainName -DnsServer $using:DNSServerIP
                
                return @{
                    Success = $true
                }
            }
            
            $configResult = Invoke-Command -Session $session -ScriptBlock $configureDHCPScript -ArgumentList $DomainName
            
            if (-not $configResult.Success) {
                throw "Failed to configure DHCP Server"
            }
            
            # Step 4: Create DHCP scope
            Update-OperationProgress -StepNumber 4 -StepDescription "Creating DHCP scope"
            
            $createScopeScript = {
                param (
                    [string]$ScopeID,
                    [string]$StartRange,
                    [string]$EndRange,
                    [string]$SubnetMask,
                    [string]$ScopeName,
                    [string]$ScopeDescription,
                    [int]$LeaseDurationDays
                )
                
                try {
                    # Create the scope
                    Add-DhcpServerv4Scope -Name $ScopeName -Description $ScopeDescription -StartRange $StartRange -EndRange $EndRange -SubnetMask $SubnetMask -State Active
                    
                    # Configure lease duration
                    Set-DhcpServerv4Scope -ScopeId $ScopeID -LeaseDuration (New-TimeSpan -Days $LeaseDurationDays)
                    
                    # Set scope options
                    Set-DhcpServerv4OptionValue -ScopeId $ScopeID -Router $using:Router -DnsServer $using:DNSServerIP -DnsDomain $using:DomainName
                    
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
            
            $scopeParams = @{
                ScopeID = $ScopeID
                StartRange = $StartRange
                EndRange = $EndRange
                SubnetMask = $SubnetMask
                ScopeName = $ScopeName
                ScopeDescription = $ScopeDescription
                LeaseDurationDays = $LeaseDurationDays
            }
            
            $scopeResult = Invoke-Command -Session $session -ScriptBlock $createScopeScript -ArgumentList $scopeParams
            
            if (-not $scopeResult.Success) {
                throw "Failed to create DHCP scope: $($scopeResult.Error)"
            }
            
            # Step 5: Post-configuration tasks
            Update-OperationProgress -StepNumber 5 -StepDescription "Performing post-configuration tasks"
            
            # Authorize DHCP server in Active Directory (if required)
            if ($AuthorizeDHCP) {
                $authorizeDHCPScript = {
                    try {
                        # Get the server FQDN
                        $computerName = $env:COMPUTERNAME
                        $fqdn = ([System.Net.Dns]::GetHostByName($computerName)).HostName
                        
                        # Authorize the DHCP server in AD
                        Add-DhcpServerInDC -DnsName $fqdn -IPAddress (Get-NetIPAddress -AddressFamily IPv4 -InterfaceAlias "Ethernet*").IPAddress
                        
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
                
                $authorizeResult = Invoke-Command -Session $session -ScriptBlock $authorizeDHCPScript
                
                if (-not $authorizeResult.Success) {
                    Write-LogMessage -Level Warning -Message "Failed to authorize DHCP server in Active Directory: $($authorizeResult.Error)"
                }
            }
            
            # Configure DHCP failover (if required)
            if ($ConfigureFailover -and -not [string]::IsNullOrEmpty($FailoverPartnerServer)) {
                $configureFailoverScript = {
                    param (
                        [string]$PartnerServer,
                        [string]$ScopeID,
                        [int]$ScopePercent
                    )
                    
                    try {
                        # Configure failover for the scope
                        $computerName = $env:COMPUTERNAME
                        $relationshipName = "$computerName-$PartnerServer-Failover"
                        
                        Add-DhcpServerv4Failover -ComputerName $computerName -PartnerServer $PartnerServer -Name $relationshipName -ScopeId $ScopeID -LoadBalancePercent $ScopePercent -SharedSecret (ConvertTo-SecureString -String "DHCPFailoverSecret" -AsPlainText -Force) -Force
                        
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
                
                $failoverParams = @{
                    PartnerServer = $FailoverPartnerServer
                    ScopeID = $ScopeID
                    ScopePercent = $FailoverScopePercent
                }
                
                $failoverResult = Invoke-Command -Session $session -ScriptBlock $configureFailoverScript -ArgumentList $failoverParams
                
                if (-not $failoverResult.Success) {
                    Write-LogMessage -Level Warning -Message "Failed to configure DHCP failover: $($failoverResult.Error)"
                }
            }
            
            # Verify DHCP Server is running
            $verifyDHCPScript = {
                $dhcpService = Get-Service -Name DHCPServer
                $scopeExists = Get-DhcpServerv4Scope | Where-Object { $_.ScopeId -eq $using:ScopeID }
                
                return @{
                    ServiceRunning = $dhcpService.Status -eq 'Running'
                    ScopeExists = $null -ne $scopeExists
                }
            }
            
            $verifyResult = Invoke-Command -Session $session -ScriptBlock $verifyDHCPScript
            
            if (-not $verifyResult.ServiceRunning) {
                Write-LogMessage -Level Warning -Message "DHCP Server service is not running. Attempting to start it..."
                
                Invoke-Command -Session $session -ScriptBlock {
                    Start-Service -Name DHCPServer
                }
            }
            
            if (-not $verifyResult.ScopeExists) {
                Write-LogMessage -Level Warning -Message "DHCP Scope $ScopeID does not exist or could not be verified."
            }
            
            Update-OperationProgress -StepNumber 5 -StepDescription "DHCP Server configuration complete" -Completed $true
            Write-LogMessage -Level Info -Message "DHCP Server configuration completed successfully for VM: $VMName"
        }
        catch {
            $errorMessage = "Failed to configure DHCP Server: $_"
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
        Complete-TrackingOperation -OperationName "ConfigureDHCPServer"
    }
}

Export-ModuleMember -Function Install-DHCPServer 
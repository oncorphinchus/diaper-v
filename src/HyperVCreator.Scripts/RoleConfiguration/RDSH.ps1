<#
.SYNOPSIS
    Configures a Hyper-V VM as a Remote Desktop Session Host.

.DESCRIPTION
    This script configures a Windows Server VM as a Remote Desktop Session Host (RDSH),
    including installing the Remote Desktop Services role and configuring RDS settings.

.PARAMETER VMName
    Name of the VM to configure.

.PARAMETER RDSHCollectionName
    Name of the RDS collection to create.

.PARAMETER LicenseServer
    Name or IP address of the RDS license server. If not specified, licensing will be configured later.

.PARAMETER InstallRDWeb
    Whether to install RD Web Access role on this server. Default is $false.

.PARAMETER InstallRDGateway
    Whether to install RD Gateway role on this server. Default is $false.

.PARAMETER InstallRDConnection
    Whether to install RD Connection Broker role on this server. Default is $false.

.PARAMETER JoinDomain
    Whether to join this server to a domain. Default is $false.

.PARAMETER DomainName
    Domain name to join when JoinDomain is $true.

.PARAMETER DomainCredential
    Credential object for domain join when JoinDomain is $true.

.NOTES
    File Name      : RDSH.ps1
    Author         : HyperV Creator Team
    Prerequisite   : PowerShell 5.1 or later
#>

# Source common functions
. "$PSScriptRoot\..\Common\ErrorHandling.ps1"
. "$PSScriptRoot\..\Common\Logging.ps1"
. "$PSScriptRoot\..\Common\Validation.ps1"
. "$PSScriptRoot\..\Monitoring\TrackProgress.ps1"

function Install-RDSH {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$VMName,
        
        [Parameter(Mandatory=$true)]
        [string]$RDSHCollectionName,
        
        [Parameter(Mandatory=$false)]
        [string]$LicenseServer = "",
        
        [Parameter(Mandatory=$false)]
        [bool]$InstallRDWeb = $false,
        
        [Parameter(Mandatory=$false)]
        [bool]$InstallRDGateway = $false,
        
        [Parameter(Mandatory=$false)]
        [bool]$InstallRDConnection = $false,
        
        [Parameter(Mandatory=$false)]
        [bool]$JoinDomain = $false,
        
        [Parameter(Mandatory=$false)]
        [string]$DomainName = "",
        
        [Parameter(Mandatory=$false)]
        [PSCredential]$DomainCredential = $null
    )
    
    begin {
        Write-LogMessage -Level Info -Message "Starting Remote Desktop Session Host configuration for VM: $VMName"
        $totalSteps = 3
        if ($JoinDomain) { $totalSteps++ }
        if ($InstallRDWeb) { $totalSteps++ }
        if ($InstallRDGateway) { $totalSteps++ }
        if ($InstallRDConnection) { $totalSteps++ }
        
        Start-TrackingOperation -OperationName "ConfigureRDSH" -TotalSteps $totalSteps
        $currentStep = 1
    }
    
    process {
        try {
            # Step 1: Ensure VM is running
            Update-OperationProgress -StepNumber $currentStep -StepDescription "Verifying VM is running"
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
            $currentStep++
            
            # Step 2: Join domain if specified
            if ($JoinDomain) {
                Update-OperationProgress -StepNumber $currentStep -StepDescription "Joining domain: $DomainName"
                if ([string]::IsNullOrWhiteSpace($DomainName)) {
                    throw "Domain name is required when JoinDomain is true"
                }
                if ($null -eq $DomainCredential) {
                    throw "Domain credentials are required when JoinDomain is true"
                }
                
                $joinDomainScript = {
                    param ($DomainName, $Credential)
                    
                    try {
                        Add-Computer -DomainName $DomainName -Credential $Credential -Restart
                        return @{
                            Success = $true
                            Message = "Domain join initiated successfully. Server will restart."
                        }
                    }
                    catch {
                        return @{
                            Success = $false
                            Message = "Failed to join domain: $_"
                        }
                    }
                }
                
                $session = New-PSSession -VMName $VMName -Credential $AdminCredential
                $joinResult = Invoke-Command -Session $session -ScriptBlock $joinDomainScript -ArgumentList $DomainName, $DomainCredential
                
                if (-not $joinResult.Success) {
                    throw $joinResult.Message
                }
                
                # Wait for VM to restart after domain join
                Write-LogMessage -Level Info -Message "Waiting for VM to restart after domain join..."
                Start-Sleep -Seconds 60
                Remove-PSSession -Session $session -ErrorAction SilentlyContinue
                
                # Wait for VM to be available again
                $vmAvailable = $false
                $waitAttempts = 0
                $maxWaitAttempts = 20
                
                while (-not $vmAvailable -and $waitAttempts -lt $maxWaitAttempts) {
                    try {
                        $newSession = New-PSSession -VMName $VMName -Credential $AdminCredential -ErrorAction Stop
                        $vmAvailable = $true
                        Remove-PSSession -Session $newSession -ErrorAction SilentlyContinue
                    }
                    catch {
                        Write-LogMessage -Level Warning -Message "VM not yet available after domain join. Waiting..."
                        Start-Sleep -Seconds 15
                        $waitAttempts++
                    }
                }
                
                if (-not $vmAvailable) {
                    throw "VM did not become available after domain join within the timeout period"
                }
                
                $currentStep++
            }
            
            # Step 3: Install RDS role
            Update-OperationProgress -StepNumber $currentStep -StepDescription "Installing Remote Desktop Services roles"
            
            $installRolesScript = {
                param ($InstallRDWeb, $InstallRDGateway, $InstallRDConnection)
                
                # Determine which roles to install
                $rolesToInstall = @("RDS-RD-Server")  # Base RDSH role
                
                if ($InstallRDWeb) {
                    $rolesToInstall += "RDS-Web-Access"
                }
                
                if ($InstallRDGateway) {
                    $rolesToInstall += "RDS-Gateway"
                }
                
                if ($InstallRDConnection) {
                    $rolesToInstall += "RDS-Connection-Broker"
                }
                
                try {
                    # Install RDS roles
                    $installResult = Install-WindowsFeature -Name $rolesToInstall -IncludeManagementTools
                    
                    return @{
                        Success = $installResult.Success
                        RestartNeeded = ($installResult.RestartNeeded -eq "Yes")
                        InstalledRoles = $installResult.FeatureResult.Name -join ", "
                        Message = if ($installResult.Success) { "RDS roles installed successfully" } else { "Failed to install RDS roles" }
                    }
                }
                catch {
                    return @{
                        Success = $false
                        Message = "Exception during RDS role installation: $_"
                    }
                }
            }
            
            $session = New-PSSession -VMName $VMName -Credential $AdminCredential
            $installResult = Invoke-Command -Session $session -ScriptBlock $installRolesScript -ArgumentList $InstallRDWeb, $InstallRDGateway, $InstallRDConnection
            
            if (-not $installResult.Success) {
                throw $installResult.Message
            }
            
            Write-LogMessage -Level Info -Message "Installed RDS roles: $($installResult.InstalledRoles)"
            
            # Restart if needed
            if ($installResult.RestartNeeded) {
                Write-LogMessage -Level Info -Message "Restarting VM after RDS role installation..."
                Remove-PSSession -Session $session -ErrorAction SilentlyContinue
                Restart-VM -Name $VMName -Force
                
                # Wait for VM to be available again
                Start-Sleep -Seconds 60
                $vmAvailable = $false
                $waitAttempts = 0
                $maxWaitAttempts = 20
                
                while (-not $vmAvailable -and $waitAttempts -lt $maxWaitAttempts) {
                    try {
                        $newSession = New-PSSession -VMName $VMName -Credential $AdminCredential -ErrorAction Stop
                        $vmAvailable = $true
                        Remove-PSSession -Session $newSession -ErrorAction SilentlyContinue
                    }
                    catch {
                        Write-LogMessage -Level Warning -Message "VM not yet available after restart. Waiting..."
                        Start-Sleep -Seconds 15
                        $waitAttempts++
                    }
                }
                
                if (-not $vmAvailable) {
                    throw "VM did not become available after restart within the timeout period"
                }
            }
            
            $currentStep++
            
            # Step 4: Configure RDS deployment and collection
            Update-OperationProgress -StepNumber $currentStep -StepDescription "Configuring RDS deployment and collection"
            
            $configureRDSScript = {
                param ($RDSHCollectionName, $LicenseServer)
                
                try {
                    Import-Module RemoteDesktop
                    
                    # Create a new RDS deployment if this server has the connection broker role
                    $hasBroker = (Get-WindowsFeature -Name RDS-Connection-Broker).Installed
                    
                    if ($hasBroker) {
                        # Create new RDS deployment
                        New-RDSessionDeployment -ConnectionBroker $env:COMPUTERNAME -WebAccessServer $env:COMPUTERNAME -SessionHost $env:COMPUTERNAME
                        
                        # Create session collection
                        New-RDSessionCollection -CollectionName $RDSHCollectionName -SessionHost $env:COMPUTERNAME -ConnectionBroker $env:COMPUTERNAME
                        
                        # Configure licensing if specified
                        if (-not [string]::IsNullOrWhiteSpace($LicenseServer)) {
                            Set-RDLicenseConfiguration -LicenseServer $LicenseServer -Mode PerUser -ConnectionBroker $env:COMPUTERNAME
                        }
                        
                        return @{
                            Success = $true
                            Message = "RDS deployment and collection created successfully"
                        }
                    }
                    else {
                        # This is just a session host without connection broker
                        return @{
                            Success = $true
                            Message = "RDS session host role installed. Connection broker is required for full deployment."
                        }
                    }
                }
                catch {
                    return @{
                        Success = $false
                        Message = "Failed to configure RDS deployment: $_"
                    }
                }
            }
            
            $session = New-PSSession -VMName $VMName -Credential $AdminCredential
            $configResult = Invoke-Command -Session $session -ScriptBlock $configureRDSScript -ArgumentList $RDSHCollectionName, $LicenseServer
            
            if (-not $configResult.Success) {
                throw $configResult.Message
            }
            
            Write-LogMessage -Level Info -Message $configResult.Message
            Update-OperationProgress -StepNumber $currentStep -StepDescription "Remote Desktop Session Host configuration complete" -Completed $true
            Write-LogMessage -Level Info -Message "Remote Desktop Session Host configuration completed successfully for VM: $VMName"
        }
        catch {
            $errorMessage = "Failed to configure Remote Desktop Session Host: $_"
            Write-LogMessage -Level Error -Message $errorMessage
            throw $errorMessage
        }
        finally {
            # Clean up PSSession if it exists
            if ($session) {
                Remove-PSSession -Session $session -ErrorAction SilentlyContinue
            }
            if ($newSession) {
                Remove-PSSession -Session $newSession -ErrorAction SilentlyContinue
            }
        }
    }
    
    end {
        Complete-TrackingOperation -OperationName "ConfigureRDSH"
    }
}

Export-ModuleMember -Function Install-RDSH 
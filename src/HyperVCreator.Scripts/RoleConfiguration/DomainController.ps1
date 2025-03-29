<#
.SYNOPSIS
    Configures a Hyper-V VM as a Domain Controller.

.DESCRIPTION
    This script configures a Windows Server VM as a Domain Controller,
    including installing the Active Directory Domain Services role and promoting
    the server to a domain controller.

.PARAMETER VMName
    Name of the VM to configure.

.PARAMETER DomainName
    FQDN of the domain to create (e.g., contoso.local).

.PARAMETER DomainNetBIOSName
    NetBIOS name of the domain (e.g., CONTOSO).

.PARAMETER SafeModeAdminPassword
    Password for Directory Services Restore Mode.

.PARAMETER ForestMode
    Active Directory forest functional level. Default is WinThreshold (Windows Server 2016).

.PARAMETER DomainMode
    Active Directory domain functional level. Default is WinThreshold (Windows Server 2016).

.PARAMETER DatabasePath
    Path for the Active Directory database. Default is "C:\Windows\NTDS".

.PARAMETER LogPath
    Path for the Active Directory log files. Default is "C:\Windows\NTDS".

.PARAMETER SysvolPath
    Path for the SYSVOL. Default is "C:\Windows\SYSVOL".

.PARAMETER InstallDNS
    Whether to install DNS Server role. Default is $true.

.NOTES
    File Name      : DomainController.ps1
    Author         : HyperV Creator Team
    Prerequisite   : PowerShell 5.1 or later
#>

# Source common functions
. "$PSScriptRoot\..\Common\ErrorHandling.ps1"
. "$PSScriptRoot\..\Common\Logging.ps1"
. "$PSScriptRoot\..\Common\Validation.ps1"
. "$PSScriptRoot\..\Monitoring\TrackProgress.ps1"

function Install-DomainController {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$VMName,
        
        [Parameter(Mandatory=$true)]
        [string]$DomainName,
        
        [Parameter(Mandatory=$true)]
        [string]$DomainNetBIOSName,
        
        [Parameter(Mandatory=$true)]
        [securestring]$SafeModeAdminPassword,
        
        [Parameter(Mandatory=$false)]
        [string]$ForestMode = "WinThreshold",
        
        [Parameter(Mandatory=$false)]
        [string]$DomainMode = "WinThreshold",
        
        [Parameter(Mandatory=$false)]
        [string]$DatabasePath = "C:\Windows\NTDS",
        
        [Parameter(Mandatory=$false)]
        [string]$LogPath = "C:\Windows\NTDS",
        
        [Parameter(Mandatory=$false)]
        [string]$SysvolPath = "C:\Windows\SYSVOL",
        
        [Parameter(Mandatory=$false)]
        [bool]$InstallDNS = $true
    )
    
    begin {
        Write-LogMessage -Level Info -Message "Starting Domain Controller configuration for VM: $VMName"
        Start-TrackingOperation -OperationName "ConfigureDomainController" -TotalSteps 4
    }
    
    process {
        try {
            # Validate parameters
            $domainValid = Test-DomainName -DomainName $DomainName
            if (-not $domainValid.IsValid) {
                throw "Invalid domain name: $($domainValid.Message)"
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
            
            # Step 2: Install AD DS role
            Update-OperationProgress -StepNumber 2 -StepDescription "Installing Active Directory Domain Services role"
            $installADDSScript = {
                Install-WindowsFeature -Name AD-Domain-Services -IncludeManagementTools
            }
            
            $session = New-PSSession -VMName $VMName -Credential $AdminCredential
            $addsResult = Invoke-Command -Session $session -ScriptBlock $installADDSScript
            
            if (-not $addsResult.Success) {
                throw "Failed to install AD DS role: $($addsResult.ExitCode)"
            }
            
            # Step 3: Promote to Domain Controller
            Update-OperationProgress -StepNumber 3 -StepDescription "Promoting server to Domain Controller"
            $promoteDCScript = {
                param (
                    $DomainName,
                    $DomainNetBIOSName,
                    $SafeModeAdminPassword,
                    $ForestMode,
                    $DomainMode,
                    $DatabasePath,
                    $LogPath,
                    $SysvolPath,
                    $InstallDNS
                )
                
                $params = @{
                    CreateDnsDelegation           = $false
                    DatabasePath                  = $DatabasePath
                    DomainMode                    = $DomainMode
                    DomainName                    = $DomainName
                    DomainNetbiosName             = $DomainNetBIOSName
                    ForestMode                    = $ForestMode
                    InstallDns                    = $InstallDNS
                    LogPath                       = $LogPath
                    NoRebootOnCompletion          = $false
                    SysvolPath                    = $SysvolPath
                    Force                         = $true
                    SafeModeAdministratorPassword = $SafeModeAdminPassword
                }
                
                Import-Module ADDSDeployment
                Install-ADDSForest @params
            }
            
            $promoteDCParams = @{
                DomainName            = $DomainName
                DomainNetBIOSName     = $DomainNetBIOSName
                SafeModeAdminPassword = $SafeModeAdminPassword
                ForestMode            = $ForestMode
                DomainMode            = $DomainMode
                DatabasePath          = $DatabasePath
                LogPath               = $LogPath
                SysvolPath            = $SysvolPath
                InstallDNS            = $InstallDNS
            }
            
            Invoke-Command -Session $session -ScriptBlock $promoteDCScript -ArgumentList $promoteDCParams
            
            # VM will reboot automatically after promotion
            
            # Step 4: Verify Domain Controller setup
            Update-OperationProgress -StepNumber 4 -StepDescription "Verifying Domain Controller configuration"
            
            # Wait for VM to come back online
            Write-LogMessage -Level Info -Message "Waiting for VM to restart after DC promotion..."
            Start-Sleep -Seconds 60
            
            $verificationRetries = 10
            $verificationSuccess = $false
            
            for ($i = 0; $i -lt $verificationRetries; $i++) {
                try {
                    $newSession = New-PSSession -VMName $VMName -Credential $AdminCredential -ErrorAction Stop
                    
                    $verifyDCScript = {
                        $addsInstalled = (Get-WindowsFeature -Name AD-Domain-Services).Installed
                        $dcPromoCompleted = Test-Path "HKLM:\SYSTEM\CurrentControlSet\Services\NTDS"
                        
                        return @{
                            ADDSInstalled   = $addsInstalled
                            DCPromoCompleted = $dcPromoCompleted
                        }
                    }
                    
                    $verifyResult = Invoke-Command -Session $newSession -ScriptBlock $verifyDCScript
                    
                    if ($verifyResult.ADDSInstalled -and $verifyResult.DCPromoCompleted) {
                        $verificationSuccess = $true
                        Write-LogMessage -Level Info -Message "Domain Controller configuration verified successfully"
                        break
                    }
                    
                    Write-LogMessage -Level Warning -Message "Verification attempt $($i+1) failed, waiting 30 seconds..."
                    Remove-PSSession -Session $newSession -ErrorAction SilentlyContinue
                    Start-Sleep -Seconds 30
                }
                catch {
                    Write-LogMessage -Level Warning -Message "Failed to connect to VM for verification: $_"
                    Start-Sleep -Seconds 30
                }
            }
            
            if (-not $verificationSuccess) {
                throw "Failed to verify Domain Controller configuration after multiple attempts"
            }
            
            Update-OperationProgress -StepNumber 4 -StepDescription "Domain Controller configuration complete" -Completed $true
            Write-LogMessage -Level Info -Message "Domain Controller configuration completed successfully for VM: $VMName"
        }
        catch {
            $errorMessage = "Failed to configure Domain Controller: $_"
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
        Complete-TrackingOperation -OperationName "ConfigureDomainController"
    }
}

Export-ModuleMember -Function Install-DomainController 
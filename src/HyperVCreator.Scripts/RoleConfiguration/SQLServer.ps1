<#
.SYNOPSIS
    Configures a Hyper-V VM as a SQL Server.

.DESCRIPTION
    This script installs and configures SQL Server on a Windows Server VM.
    It handles the installation of SQL Server prerequisites, features, and 
    post-installation configuration.

.PARAMETER VMName
    Name of the VM to configure.

.PARAMETER SQLServerISOPath
    Path to the SQL Server installation ISO.

.PARAMETER SQLEdition
    SQL Server edition to install (e.g., Standard, Enterprise, Developer).
    Default is "Developer".

.PARAMETER InstanceName
    Name of the SQL Server instance. Default is "MSSQLSERVER" (default instance).

.PARAMETER SQLServiceAccount
    Username for the SQL Server service account. Default is "NT Service\MSSQLSERVER".

.PARAMETER SQLServicePassword
    Password for the SQL Server service account. Required if using a domain account.

.PARAMETER SQLSysAdminAccounts
    List of accounts to add to the SQL Server sysadmin role. Default includes current user.

.PARAMETER SQLInstallationPath
    Path where SQL Server will be installed. Default is "C:\Program Files\Microsoft SQL Server".

.PARAMETER SQLDataPath
    Path for SQL Server data files. Default is "C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA".

.PARAMETER SQLLogPath
    Path for SQL Server log files. Default is "C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA".

.PARAMETER SQLBackupPath
    Path for SQL Server backup files. Default is "C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\Backup".

.PARAMETER SQLTempDBPath
    Path for SQL Server TempDB files. Default is "C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA".

.PARAMETER SQLFeatures
    Features to install. Default is "SQLENGINE,SSMS".

.PARAMETER SQLCollation
    SQL Server collation to use. Default is "SQL_Latin1_General_CP1_CI_AS".

.PARAMETER ConfigureFirewall
    Whether to configure the Windows Firewall for SQL Server. Default is $true.

.PARAMETER EnableMixedMode
    Whether to enable SQL Server mixed mode authentication. Default is $false.

.PARAMETER SAPassword
    SA account password, required if EnableMixedMode is $true.

.NOTES
    File Name      : SQLServer.ps1
    Author         : HyperV Creator Team
    Prerequisite   : PowerShell 5.1 or later
#>

# Source common functions
. "$PSScriptRoot\..\Common\ErrorHandling.ps1"
. "$PSScriptRoot\..\Common\Logging.ps1"
. "$PSScriptRoot\..\Common\Validation.ps1"
. "$PSScriptRoot\..\Monitoring\TrackProgress.ps1"

function Install-SQLServer {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$VMName,
        
        [Parameter(Mandatory=$true)]
        [string]$SQLServerISOPath,
        
        [Parameter(Mandatory=$false)]
        [ValidateSet("Standard", "Enterprise", "Developer", "Express", "Web")]
        [string]$SQLEdition = "Developer",
        
        [Parameter(Mandatory=$false)]
        [string]$InstanceName = "MSSQLSERVER",
        
        [Parameter(Mandatory=$false)]
        [string]$SQLServiceAccount = "NT Service\MSSQLSERVER",
        
        [Parameter(Mandatory=$false)]
        [securestring]$SQLServicePassword,
        
        [Parameter(Mandatory=$false)]
        [string[]]$SQLSysAdminAccounts = @("BUILTIN\Administrators"),
        
        [Parameter(Mandatory=$false)]
        [string]$SQLInstallationPath = "C:\Program Files\Microsoft SQL Server",
        
        [Parameter(Mandatory=$false)]
        [string]$SQLDataPath = "C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA",
        
        [Parameter(Mandatory=$false)]
        [string]$SQLLogPath = "C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA",
        
        [Parameter(Mandatory=$false)]
        [string]$SQLBackupPath = "C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\Backup",
        
        [Parameter(Mandatory=$false)]
        [string]$SQLTempDBPath = "C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA",
        
        [Parameter(Mandatory=$false)]
        [string]$SQLFeatures = "SQLENGINE,SSMS",
        
        [Parameter(Mandatory=$false)]
        [string]$SQLCollation = "SQL_Latin1_General_CP1_CI_AS",
        
        [Parameter(Mandatory=$false)]
        [bool]$ConfigureFirewall = $true,
        
        [Parameter(Mandatory=$false)]
        [bool]$EnableMixedMode = $false,
        
        [Parameter(Mandatory=$false)]
        [securestring]$SAPassword
    )
    
    begin {
        Write-LogMessage -Level Info -Message "Starting SQL Server installation and configuration for VM: $VMName"
        Start-TrackingOperation -OperationName "ConfigureSQLServer" -TotalSteps 6
    }
    
    process {
        try {
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
            
            # Step 2: Check for prerequisite software and Windows features
            Update-OperationProgress -StepNumber 2 -StepDescription "Installing prerequisites"
            
            # Establish a PowerShell session to the VM
            $session = New-PSSession -VMName $VMName -Credential $AdminCredential
            
            # Install prerequisite Windows features
            $installPrereqsScript = {
                # Install .NET Framework 4.8 if not already installed
                # Note: Modern SQL Server versions require .NET Framework 4.8
                $netFramework = Get-WindowsFeature -Name NET-Framework-45-Core
                if (-not $netFramework.Installed) {
                    Install-WindowsFeature -Name NET-Framework-45-Core -IncludeManagementTools
                }
                
                # Install other features required for SQL Server
                Install-WindowsFeature -Name RSAT-AD-PowerShell, RSAT-AD-AdminCenter
            }
            
            Invoke-Command -Session $session -ScriptBlock $installPrereqsScript
            
            # Step 3: Mount SQL Server ISO and prepare for installation
            Update-OperationProgress -StepNumber 3 -StepDescription "Mounting SQL Server ISO and preparing installation"
            
            # Check if ISO exists
            if (-not (Test-Path $SQLServerISOPath)) {
                throw "SQL Server ISO not found at path: $SQLServerISOPath"
            }
            
            # Mount the ISO to the VM
            $mountISOScript = {
                param (
                    [string]$ISOPath
                )
                
                # Create a temporary directory to copy the ISO content
                $tempDir = "C:\SQLServerSetup"
                if (-not (Test-Path $tempDir)) {
                    New-Item -Path $tempDir -ItemType Directory -Force | Out-Null
                }
                
                return $tempDir
            }
            
            $setupDir = Invoke-Command -Session $session -ScriptBlock $mountISOScript -ArgumentList $SQLServerISOPath
            
            # Copy the ISO content to the VM
            Write-LogMessage -Level Info -Message "Copying SQL Server installation files to VM..."
            Copy-Item -ToSession $session -Path $SQLServerISOPath -Destination "$setupDir\SQLServer.iso" -Force
            
            # Extract ISO on the VM
            $extractISOScript = {
                param (
                    [string]$SetupDir
                )
                
                # Mount the ISO
                $mountResult = Mount-DiskImage -ImagePath "$SetupDir\SQLServer.iso" -PassThru
                $driveLetter = ($mountResult | Get-Volume).DriveLetter
                
                # Create destination for SQL setup files
                $sqlSetupDir = "$SetupDir\SQLSetup"
                if (-not (Test-Path $sqlSetupDir)) {
                    New-Item -Path $sqlSetupDir -ItemType Directory -Force | Out-Null
                }
                
                # Copy files from ISO to setup directory
                Copy-Item -Path "${driveLetter}:\*" -Destination $sqlSetupDir -Recurse -Force
                
                # Unmount the ISO
                Dismount-DiskImage -ImagePath "$SetupDir\SQLServer.iso"
                
                return $sqlSetupDir
            }
            
            $sqlSetupDir = Invoke-Command -Session $session -ScriptBlock $extractISOScript -ArgumentList $setupDir
            
            # Step 4: Create SQL Server configuration file
            Update-OperationProgress -StepNumber 4 -StepDescription "Creating SQL Server configuration file"
            
            $createConfigFileScript = {
                param (
                    [string]$SQLSetupDir,
                    [string]$SQLEdition,
                    [string]$InstanceName,
                    [string]$SQLServiceAccount,
                    [string]$SQLInstallationPath,
                    [string]$SQLDataPath,
                    [string]$SQLLogPath,
                    [string]$SQLBackupPath,
                    [string]$SQLTempDBPath,
                    [string]$SQLFeatures,
                    [string]$SQLCollation,
                    [bool]$EnableMixedMode,
                    [string[]]$SQLSysAdminAccounts
                )
                
                $configFilePath = "$SQLSetupDir\ConfigurationFile.ini"
                
                # Create the configuration file content
                $configContent = @"
[OPTIONS]
ACTION="Install"
IACCEPTSQLSERVERLICENSETERMS="True"
QUIET="True"
QUIETSIMPLE="False"
UpdateEnabled="True"
FEATURES="$SQLFeatures"
InstanceName="$InstanceName"
InstanceID="$InstanceName"
INSTANCEDIR="$SQLInstallationPath"
INSTALLSHAREDDIR="$SQLInstallationPath"
INSTALLSHAREDWOWDIR="$SQLInstallationPath\x86"
SQLCOLLATION="$SQLCollation"

; SQL Server service account
SQLSVCACCOUNT="$SQLServiceAccount"
SQLSVCSTARTUPTYPE="Automatic"

; SQL Server Agent service account
AGTSVCACCOUNT="NT Service\SQLSERVERAGENT"
AGTSVCSTARTUPTYPE="Automatic"

; SQL Server database file locations
INSTALLSQLDATADIR="$SQLDataPath"
SQLUSERDBDIR="$SQLDataPath"
SQLUSERDBLOGDIR="$SQLLogPath"
SQLBACKUPDIR="$SQLBackupPath"
SQLTEMPDBDIR="$SQLTempDBPath"
SQLTEMPDBLOGDIR="$SQLTempDBPath"

; Security settings
SECURITYMODE="{0}"
SAPWD="{1}"

; System admin accounts
SQLSYSADMINACCOUNTS="{2}"

; Browser and network configuration
BROWSERSVCSTARTUPTYPE="Automatic"
TCPENABLED="1"
NPENABLED="0"
"@
                
                $securityMode = if ($EnableMixedMode) { "SQL" } else { "Windows" }
                $saPassword = if ($EnableMixedMode) { $using:SAPassword | ConvertFrom-SecureString -AsPlainText } else { "" }
                $sysAdminAccountsStr = $SQLSysAdminAccounts -join " "
                
                $configContent = $configContent -f $securityMode, $saPassword, $sysAdminAccountsStr
                
                # Write configuration file
                Set-Content -Path $configFilePath -Value $configContent -Force
                
                return $configFilePath
            }
            
            # Convert secure string to plain text for the script block
            $saPasswordPlain = ""
            if ($EnableMixedMode -and $null -ne $SAPassword) {
                $saPasswordPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
                    [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SAPassword)
                )
            }
            
            $sqlServicePasswordPlain = ""
            if ($null -ne $SQLServicePassword) {
                $sqlServicePasswordPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
                    [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SQLServicePassword)
                )
            }
            
            # Create the config file for SQL Server setup
            $configFileParams = @{
                SQLSetupDir         = $sqlSetupDir
                SQLEdition          = $SQLEdition
                InstanceName        = $InstanceName
                SQLServiceAccount   = $SQLServiceAccount
                SQLInstallationPath = $SQLInstallationPath
                SQLDataPath         = $SQLDataPath
                SQLLogPath          = $SQLLogPath
                SQLBackupPath       = $SQLBackupPath
                SQLTempDBPath       = $SQLTempDBPath
                SQLFeatures         = $SQLFeatures
                SQLCollation        = $SQLCollation
                EnableMixedMode     = $EnableMixedMode
                SQLSysAdminAccounts = $SQLSysAdminAccounts
            }
            
            $configFilePath = Invoke-Command -Session $session -ScriptBlock $createConfigFileScript -ArgumentList $configFileParams
            
            # Step 5: Install SQL Server
            Update-OperationProgress -StepNumber 5 -StepDescription "Installing SQL Server"
            
            $installSQLScript = {
                param (
                    [string]$SQLSetupDir,
                    [string]$ConfigFilePath,
                    [string]$SQLEdition
                )
                
                $setupExe = Join-Path -Path $SQLSetupDir -ChildPath "setup.exe"
                $editionSwitch = "/IACCEPTSQLSERVERLICENSETERMS /ENU /QUIET"
                
                if (-not (Test-Path $setupExe)) {
                    throw "SQL Server setup.exe not found at: $setupExe"
                }
                
                # Start the installation
                $process = Start-Process -FilePath $setupExe -ArgumentList "/ConfigurationFile=$ConfigFilePath $editionSwitch" -PassThru -Wait -NoNewWindow
                
                return @{
                    ExitCode = $process.ExitCode
                    Success = $process.ExitCode -eq 0
                }
            }
            
            $installResult = Invoke-Command -Session $session -ScriptBlock $installSQLScript -ArgumentList $sqlSetupDir, $configFilePath, $SQLEdition
            
            if (-not $installResult.Success) {
                throw "SQL Server installation failed with exit code: $($installResult.ExitCode)"
            }
            
            # Step 6: Post-installation configuration
            Update-OperationProgress -StepNumber 6 -StepDescription "Performing post-installation configuration"
            
            if ($ConfigureFirewall) {
                $configureFirewallScript = {
                    # Configure Windows Firewall for SQL Server
                    New-NetFirewallRule -DisplayName "SQL Server" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
                    New-NetFirewallRule -DisplayName "SQL Admin Connection" -Direction Inbound -Protocol TCP -LocalPort 1434 -Action Allow
                    New-NetFirewallRule -DisplayName "SQL Service Broker" -Direction Inbound -Protocol TCP -LocalPort 4022 -Action Allow
                    New-NetFirewallRule -DisplayName "SQL Debugger/RPC" -Direction Inbound -Protocol TCP -LocalPort 135 -Action Allow
                    
                    # Enable SQL Server Browser service
                    Set-Service -Name SQLBrowser -StartupType Automatic
                    Start-Service -Name SQLBrowser
                    
                    # Create firewall rule for SQL Browser
                    New-NetFirewallRule -DisplayName "SQL Browser" -Direction Inbound -Protocol UDP -LocalPort 1434 -Action Allow
                }
                
                Invoke-Command -Session $session -ScriptBlock $configureFirewallScript
            }
            
            # Validate SQL Server installation
            $validateSQLScript = {
                $sqlServices = Get-Service -Name MSSQL* | Where-Object { $_.DisplayName -like "SQL Server (*)" }
                $sqlAgentServices = Get-Service -Name SQLAGENT* | Where-Object { $_.DisplayName -like "SQL Server Agent (*)" }
                
                return @{
                    SQLServiceRunning = $sqlServices.Count -gt 0 -and $sqlServices[0].Status -eq 'Running'
                    SQLAgentServiceRunning = $sqlAgentServices.Count -gt 0 -and $sqlAgentServices[0].Status -eq 'Running'
                }
            }
            
            $validationResult = Invoke-Command -Session $session -ScriptBlock $validateSQLScript
            
            if (-not $validationResult.SQLServiceRunning) {
                Write-LogMessage -Level Warning -Message "SQL Server service is not running. Attempting to start it..."
                
                Invoke-Command -Session $session -ScriptBlock {
                    Start-Service -Name MSSQL* -ErrorAction SilentlyContinue
                }
            }
            
            Update-OperationProgress -StepNumber 6 -StepDescription "SQL Server installation and configuration complete" -Completed $true
            Write-LogMessage -Level Info -Message "SQL Server installation and configuration completed successfully for VM: $VMName"
            
            # Clean up temporary files
            Invoke-Command -Session $session -ScriptBlock {
                param($setupDir)
                Remove-Item -Path $setupDir -Recurse -Force -ErrorAction SilentlyContinue
            } -ArgumentList $setupDir
        }
        catch {
            $errorMessage = "Failed to install and configure SQL Server: $_"
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
        Complete-TrackingOperation -OperationName "ConfigureSQLServer"
    }
}

Export-ModuleMember -Function Install-SQLServer 
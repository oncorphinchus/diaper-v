<#
.SYNOPSIS
    Configures a Hyper-V VM as a File Server.

.DESCRIPTION
    This script configures a Windows Server VM as a File Server,
    including installing the File Server role, configuring disks,
    and setting up shares.

.PARAMETER VMName
    Name of the VM to configure.

.PARAMETER Shares
    Array of share configurations. Each share should have Name, Path, and Description properties.

.PARAMETER DataDisks
    Array of data disk configurations for file shares. Each disk should have SizeGB and DriveLetter properties.

.PARAMETER JoinDomain
    Whether to join this server to a domain. Default is $false.

.PARAMETER DomainName
    Domain name to join when JoinDomain is $true.

.PARAMETER DomainCredential
    Credential object for domain join when JoinDomain is $true.

.PARAMETER EnableFSRM
    Whether to enable File Server Resource Manager. Default is $false.

.PARAMETER ConfigureQuotas
    Whether to configure quotas on shares. Default is $false.

.PARAMETER EnableABE
    Whether to enable Access-Based Enumeration on shares. Default is $true.

.PARAMETER EnableShadowCopies
    Whether to enable shadow copies for volumes. Default is $true.

.NOTES
    File Name      : FileServer.ps1
    Author         : HyperV Creator Team
    Prerequisite   : PowerShell 5.1 or later
#>

# Source common functions
. "$PSScriptRoot\..\Common\ErrorHandling.ps1"
. "$PSScriptRoot\..\Common\Logging.ps1"
. "$PSScriptRoot\..\Common\Validation.ps1"
. "$PSScriptRoot\..\Monitoring\TrackProgress.ps1"

function Install-FileServer {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$VMName,
        
        [Parameter(Mandatory=$false)]
        [PSObject[]]$Shares = @(),
        
        [Parameter(Mandatory=$false)]
        [PSObject[]]$DataDisks = @(),
        
        [Parameter(Mandatory=$false)]
        [bool]$JoinDomain = $false,
        
        [Parameter(Mandatory=$false)]
        [string]$DomainName = "",
        
        [Parameter(Mandatory=$false)]
        [PSCredential]$DomainCredential = $null,
        
        [Parameter(Mandatory=$false)]
        [bool]$EnableFSRM = $false,
        
        [Parameter(Mandatory=$false)]
        [bool]$ConfigureQuotas = $false,
        
        [Parameter(Mandatory=$false)]
        [bool]$EnableABE = $true,
        
        [Parameter(Mandatory=$false)]
        [bool]$EnableShadowCopies = $true
    )
    
    begin {
        Write-LogMessage -Level Info -Message "Starting File Server configuration for VM: $VMName"
        $totalSteps = 4
        if ($JoinDomain) { $totalSteps++ }
        if ($EnableFSRM) { $totalSteps++ }
        if ($EnableShadowCopies) { $totalSteps++ }
        
        Start-TrackingOperation -OperationName "ConfigureFileServer" -TotalSteps $totalSteps
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
            
            # Step 3: Install File Server role
            Update-OperationProgress -StepNumber $currentStep -StepDescription "Installing File Server role"
            
            $installRolesScript = {
                param ($EnableFSRM)
                
                # Determine which roles to install
                $rolesToInstall = @("FS-FileServer")  # Base File Server role
                
                if ($EnableFSRM) {
                    $rolesToInstall += "FS-Resource-Manager"
                }
                
                try {
                    # Install File Server roles
                    $installResult = Install-WindowsFeature -Name $rolesToInstall -IncludeManagementTools
                    
                    return @{
                        Success = $installResult.Success
                        RestartNeeded = ($installResult.RestartNeeded -eq "Yes")
                        InstalledRoles = $installResult.FeatureResult.Name -join ", "
                        Message = if ($installResult.Success) { "File Server roles installed successfully" } else { "Failed to install File Server roles" }
                    }
                }
                catch {
                    return @{
                        Success = $false
                        Message = "Exception during File Server role installation: $_"
                    }
                }
            }
            
            $session = New-PSSession -VMName $VMName -Credential $AdminCredential
            $installResult = Invoke-Command -Session $session -ScriptBlock $installRolesScript -ArgumentList $EnableFSRM
            
            if (-not $installResult.Success) {
                throw $installResult.Message
            }
            
            Write-LogMessage -Level Info -Message "Installed File Server roles: $($installResult.InstalledRoles)"
            
            # Restart if needed
            if ($installResult.RestartNeeded) {
                Write-LogMessage -Level Info -Message "Restarting VM after File Server role installation..."
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
            
            # Step 4: Format and configure data disks
            Update-OperationProgress -StepNumber $currentStep -StepDescription "Configuring data disks"
            
            if ($DataDisks.Count -gt 0) {
                $configureDisksScript = {
                    param ($DataDisks)
                    
                    $results = @()
                    
                    foreach ($disk in $DataDisks) {
                        try {
                            # Find the raw disk
                            $rawDisk = Get-Disk | Where-Object { $_.OperationalStatus -eq 'Offline' } | Select-Object -First 1
                            
                            if ($null -eq $rawDisk) {
                                $results += @{
                                    Success = $false
                                    DriveLetter = $disk.DriveLetter
                                    Message = "No offline disk found for configuration"
                                }
                                continue
                            }
                            
                            # Initialize, partition, and format the disk
                            $rawDisk | Initialize-Disk -PartitionStyle GPT -PassThru |
                                New-Partition -DriveLetter $disk.DriveLetter -UseMaximumSize |
                                Format-Volume -FileSystem NTFS -NewFileSystemLabel "Data" -Confirm:$false
                            
                            $results += @{
                                Success = $true
                                DriveLetter = $disk.DriveLetter
                                Message = "Disk configured successfully as drive $($disk.DriveLetter)"
                            }
                        }
                        catch {
                            $results += @{
                                Success = $false
                                DriveLetter = $disk.DriveLetter
                                Message = "Failed to configure disk: $_"
                            }
                        }
                    }
                    
                    return $results
                }
                
                $session = New-PSSession -VMName $VMName -Credential $AdminCredential
                $diskResults = Invoke-Command -Session $session -ScriptBlock $configureDisksScript -ArgumentList $DataDisks
                
                foreach ($result in $diskResults) {
                    if ($result.Success) {
                        Write-LogMessage -Level Info -Message $result.Message
                    }
                    else {
                        Write-LogMessage -Level Warning -Message $result.Message
                    }
                }
            }
            
            $currentStep++
            
            # Step 5: Create shares
            Update-OperationProgress -StepNumber $currentStep -StepDescription "Creating file shares"
            
            if ($Shares.Count -gt 0) {
                $createSharesScript = {
                    param ($Shares, $EnableABE)
                    
                    $results = @()
                    
                    foreach ($share in $Shares) {
                        try {
                            # Create the directory if it doesn't exist
                            if (-not (Test-Path -Path $share.Path)) {
                                New-Item -Path $share.Path -ItemType Directory -Force | Out-Null
                            }
                            
                            # Create the share
                            $newShare = New-SmbShare -Name $share.Name -Path $share.Path -Description $share.Description -FullAccess "Everyone"
                            
                            # Enable Access-Based Enumeration if specified
                            if ($EnableABE) {
                                Set-SmbShare -Name $share.Name -FolderEnumerationMode AccessBased
                            }
                            
                            $results += @{
                                Success = $true
                                ShareName = $share.Name
                                Message = "Share '$($share.Name)' created successfully at $($share.Path)"
                            }
                        }
                        catch {
                            $results += @{
                                Success = $false
                                ShareName = $share.Name
                                Message = "Failed to create share '$($share.Name)': $_"
                            }
                        }
                    }
                    
                    return $results
                }
                
                $session = New-PSSession -VMName $VMName -Credential $AdminCredential
                $shareResults = Invoke-Command -Session $session -ScriptBlock $createSharesScript -ArgumentList $Shares, $EnableABE
                
                foreach ($result in $shareResults) {
                    if ($result.Success) {
                        Write-LogMessage -Level Info -Message $result.Message
                    }
                    else {
                        Write-LogMessage -Level Warning -Message $result.Message
                    }
                }
            }
            
            $currentStep++
            
            # Step 6: Configure FSRM if enabled
            if ($EnableFSRM) {
                Update-OperationProgress -StepNumber $currentStep -StepDescription "Configuring File Server Resource Manager"
                
                $configureFSRMScript = {
                    param ($ConfigureQuotas, $Shares)
                    
                    $results = @{
                        Success = $true
                        Message = "File Server Resource Manager configured successfully"
                    }
                    
                    try {
                        # Enable FSRM
                        Import-Module FileServerResourceManager
                        
                        # Configure quotas if specified
                        if ($ConfigureQuotas -and $Shares.Count -gt 0) {
                            # Create a default quota template with 10GB limit
                            New-FsrmQuotaTemplate -Name "Default 10GB Limit" -Size 10GB -Threshold @(
                                New-FsrmQuotaThreshold -Percentage 85 -ThresholdAction @(New-FsrmAction -Type Event -EventType Warning -Body "File share is at 85% capacity")
                                New-FsrmQuotaThreshold -Percentage 95 -ThresholdAction @(New-FsrmAction -Type Event -EventType Error -Body "File share is at 95% capacity")
                            )
                            
                            # Apply quota to each share
                            foreach ($share in $Shares) {
                                New-FsrmQuota -Path $share.Path -Template "Default 10GB Limit"
                            }
                        }
                    }
                    catch {
                        $results = @{
                            Success = $false
                            Message = "Failed to configure FSRM: $_"
                        }
                    }
                    
                    return $results
                }
                
                $session = New-PSSession -VMName $VMName -Credential $AdminCredential
                $fsrmResult = Invoke-Command -Session $session -ScriptBlock $configureFSRMScript -ArgumentList $ConfigureQuotas, $Shares
                
                if ($fsrmResult.Success) {
                    Write-LogMessage -Level Info -Message $fsrmResult.Message
                }
                else {
                    Write-LogMessage -Level Warning -Message $fsrmResult.Message
                }
                
                $currentStep++
            }
            
            # Step 7: Configure shadow copies if enabled
            if ($EnableShadowCopies) {
                Update-OperationProgress -StepNumber $currentStep -StepDescription "Configuring shadow copies"
                
                $configureShadowCopiesScript = {
                    param ($DataDisks)
                    
                    $results = @()
                    
                    try {
                        # Create shadow copy schedule for C: drive
                        $volume = Get-WmiObject -Class Win32_Volume -Filter "DriveLetter='C:'"
                        if ($volume) {
                            $task = @{
                                Trigger = New-ScheduledTaskTrigger -Daily -At "7:00AM"
                                Action = New-ScheduledTaskAction -Execute "vssadmin.exe" -Argument "create shadow /for=C:"
                                Settings = New-ScheduledTaskSettingsSet
                                TaskName = "ShadowCopy_C"
                                Description = "Create daily shadow copy for C: drive"
                            }
                            Register-ScheduledTask @task -User "SYSTEM" -RunLevel Highest -Force
                            
                            $results += @{
                                Volume = "C:"
                                Success = $true
                                Message = "Scheduled shadow copies for C: drive"
                            }
                        }
                        
                        # Create shadow copy schedule for data disks
                        foreach ($disk in $DataDisks) {
                            $driveLetter = "$($disk.DriveLetter):"
                            $volume = Get-WmiObject -Class Win32_Volume -Filter "DriveLetter='$driveLetter'"
                            
                            if ($volume) {
                                $task = @{
                                    Trigger = New-ScheduledTaskTrigger -Daily -At "7:00AM"
                                    Action = New-ScheduledTaskAction -Execute "vssadmin.exe" -Argument "create shadow /for=$driveLetter"
                                    Settings = New-ScheduledTaskSettingsSet
                                    TaskName = "ShadowCopy_$($disk.DriveLetter)"
                                    Description = "Create daily shadow copy for $driveLetter drive"
                                }
                                Register-ScheduledTask @task -User "SYSTEM" -RunLevel Highest -Force
                                
                                $results += @{
                                    Volume = $driveLetter
                                    Success = $true
                                    Message = "Scheduled shadow copies for $driveLetter drive"
                                }
                            }
                        }
                    }
                    catch {
                        $results += @{
                            Volume = "Unknown"
                            Success = $false
                            Message = "Failed to configure shadow copies: $_"
                        }
                    }
                    
                    return $results
                }
                
                $session = New-PSSession -VMName $VMName -Credential $AdminCredential
                $shadowCopyResults = Invoke-Command -Session $session -ScriptBlock $configureShadowCopiesScript -ArgumentList $DataDisks
                
                foreach ($result in $shadowCopyResults) {
                    if ($result.Success) {
                        Write-LogMessage -Level Info -Message $result.Message
                    }
                    else {
                        Write-LogMessage -Level Warning -Message $result.Message
                    }
                }
                
                $currentStep++
            }
            
            Update-OperationProgress -StepNumber $currentStep -StepDescription "File Server configuration complete" -Completed $true
            Write-LogMessage -Level Info -Message "File Server configuration completed successfully for VM: $VMName"
        }
        catch {
            $errorMessage = "Failed to configure File Server: $_"
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
        Complete-TrackingOperation -OperationName "ConfigureFileServer"
    }
}

Export-ModuleMember -Function Install-FileServer 
# Remote Desktop Session Host Deployment Script
# This script will be used to configure a Windows Server as a Remote Desktop Session Host
# Parameters will be replaced with actual values from the application

param (
    # Basic VM Parameters
    [string]$VMName = "",
    
    # RDSH Parameters
    [string]$ConnectionBroker = "",
    [string]$CollectionName = "",
    [bool]$UseHA = $false,
    [string]$LicenseServer = "",
    [string]$LicenseMode = "",
    [int]$MaxConnections = 0,
    
    # Network Parameters
    [bool]$UseDHCP = $false,
    [string]$IPAddress = "",
    [string]$SubnetMask = "",
    [string]$DefaultGateway = "",
    [string]$PreferredDNS = "",
    
    # Advanced Options
    [bool]$InstallOffice = $false,
    [bool]$InstallRSAT = $false,
    [bool]$EnableRemoteFX = $false,
    [bool]$EnableUserProfileDisks = $false,
    [string]$UserProfileDiskPath = "",
    [int]$UserProfileDiskMaxSizeGB = 0
)

# Set up logging
$logFile = "C:\RDSHDeployment.log"
function Write-Log {
    param([string]$message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    "$timestamp - $message" | Out-File -FilePath $logFile -Append
    Write-Host "$timestamp - $message"
}

Write-Log "Starting Remote Desktop Session Host deployment script"
Write-Log "VM Name: $VMName"
Write-Log "Collection Name: $CollectionName"

# Step 1: Configure network settings if using static IP
if (-not $UseDHCP) {
    try {
        Write-Log "Configuring static IP: $IPAddress with subnet mask: $SubnetMask"
        $netAdapter = Get-NetAdapter | Where-Object { $_.Status -eq "Up" } | Select-Object -First 1
        
        # Remove existing IP configuration
        Remove-NetIPAddress -InterfaceIndex $netAdapter.ifIndex -Confirm:$false -ErrorAction SilentlyContinue
        Remove-NetRoute -InterfaceIndex $netAdapter.ifIndex -Confirm:$false -ErrorAction SilentlyContinue
        
        # Set new IP configuration
        New-NetIPAddress -InterfaceIndex $netAdapter.ifIndex -IPAddress $IPAddress -PrefixLength (ConvertTo-PrefixLength $SubnetMask) -DefaultGateway $DefaultGateway
        Set-DnsClientServerAddress -InterfaceIndex $netAdapter.ifIndex -ServerAddresses $PreferredDNS
        
        Write-Log "Static IP configuration completed successfully"
    }
    catch {
        Write-Log "Error configuring static IP: $_"
        throw
    }
}

# Helper function to convert subnet mask to prefix length
function ConvertTo-PrefixLength {
    param([string]$SubnetMask)
    
    $binaryOctets = $SubnetMask.Split(".") | ForEach-Object { [Convert]::ToString([byte]$_, 2) }
    $binaryString = $binaryOctets -join ""
    
    return ($binaryString -replace "0", "" -replace " ", "").Length
}

# Step 2: Install required roles and features
try {
    Write-Log "Installing Remote Desktop Services roles and features"
    
    # Install RDS roles and features
    Install-WindowsFeature -Name RDS-RD-Server, RDS-Licensing -IncludeManagementTools
    
    if ($InstallRSAT) {
        Write-Log "Installing Remote Server Administration Tools"
        Install-WindowsFeature -Name RSAT-RDS-Tools
    }
    
    Write-Log "Role installation completed successfully"
}
catch {
    Write-Log "Error installing roles and features: $_"
    throw
}

# Step 3: Configure Remote Desktop Session Host
try {
    Write-Log "Configuring Remote Desktop Session Host settings"
    
    # Set maximum connections
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server' -Name "MaxConnectionCount" -Value $MaxConnections
    
    # Enable or disable RemoteFX
    if ($EnableRemoteFX) {
        Write-Log "Enabling RemoteFX"
        Enable-RemoteFXvGPU
    }
    
    # Configure RDS licensing mode
    if ($LicenseServer -and $LicenseMode) {
        Write-Log "Configuring RDS licensing: Server=$LicenseServer, Mode=$LicenseMode"
        Set-RDLicenseConfiguration -LicenseServer $LicenseServer -Mode $LicenseMode -Force
    }
    
    Write-Log "Remote Desktop Session Host configuration completed successfully"
}
catch {
    Write-Log "Error configuring RDSH: $_"
    throw
}

# Step 4: Join to RDS deployment if connection broker specified
if ($ConnectionBroker) {
    try {
        Write-Log "Joining RDS deployment on connection broker: $ConnectionBroker"
        
        # Add this server to the RDS deployment
        Add-RDServer -Server $env:COMPUTERNAME -Role RDS-RD-SERVER -ConnectionBroker $ConnectionBroker
        
        # Add to collection if provided
        if ($CollectionName) {
            Write-Log "Adding to session collection: $CollectionName"
            Add-RDSessionHost -CollectionName $CollectionName -SessionHost $env:COMPUTERNAME -ConnectionBroker $ConnectionBroker
        }
        
        Write-Log "Successfully joined RDS deployment"
    }
    catch {
        Write-Log "Error joining RDS deployment: $_"
        # Continue even if this fails - might be standalone RDSH
    }
}

# Step 5: Configure User Profile Disks if enabled
if ($EnableUserProfileDisks -and $UserProfileDiskPath) {
    try {
        Write-Log "Configuring User Profile Disks: Path=$UserProfileDiskPath, MaxSize=${UserProfileDiskMaxSizeGB}GB"
        
        # Make sure collection exists before configuring UPD
        if ($ConnectionBroker -and $CollectionName) {
            Enable-RDUserProfileDisk -ConnectionBroker $ConnectionBroker -CollectionName $CollectionName -DiskPath $UserProfileDiskPath -MaxUserProfileDiskSizeGB $UserProfileDiskMaxSizeGB
            Write-Log "User Profile Disks configured successfully"
        }
        else {
            Write-Log "Warning: Cannot configure User Profile Disks without a connection broker and collection name"
        }
    }
    catch {
        Write-Log "Error configuring User Profile Disks: $_"
    }
}

# Step 6: Install Office if requested
if ($InstallOffice) {
    try {
        Write-Log "Installing Microsoft Office"
        
        # In a real implementation, you would download and install Office using the Office Deployment Tool
        # This is a placeholder for demonstration purposes
        Write-Log "Office installation would begin here"
        
        Write-Log "Office installation completed"
    }
    catch {
        Write-Log "Error installing Office: $_"
    }
}

# Step 7: Configure recommended settings for RDSH
try {
    Write-Log "Configuring recommended settings for RDSH"
    
    # Enable Audio redirection
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp' -Name "AudioInRedirection" -Value 1
    
    # Set Keep-Alive connection interval
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp' -Name "KeepAliveTimeout" -Value 1
    
    # Configure Shadow settings (permit remote control with user's permission)
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp' -Name "Shadow" -Value 2
    
    # Enable clipboard redirection
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp' -Name "fDisableClip" -Value 0
    
    # Enable drive redirection
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp' -Name "fDisableCdm" -Value 0
    
    # Enable printer redirection
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp' -Name "fDisableCpm" -Value 0
    
    # Enable timezone redirection
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp' -Name "fInheritTimeZone" -Value 0
    
    Write-Log "Recommended settings configured successfully"
}
catch {
    Write-Log "Error configuring recommended settings: $_"
}

# Restarting the RDS service to apply changes
try {
    Write-Log "Restarting Remote Desktop Services"
    Restart-Service -Name TermService -Force
    Write-Log "Remote Desktop Services restarted successfully"
}
catch {
    Write-Log "Error restarting Remote Desktop Services: $_"
}

Write-Log "Remote Desktop Session Host deployment script completed. Reboot is recommended to apply all settings." 
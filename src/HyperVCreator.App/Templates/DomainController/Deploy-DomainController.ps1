# Domain Controller Deployment Script
# This script will be used to configure a Windows Server as a Domain Controller
# Parameters will be replaced with actual values from the application

param (
    # Basic VM Parameters
    [string]$VMName = "{{VMName}}",
    
    # Domain Parameters
    [string]$DomainName = "{{DomainName}}",
    [string]$NetBIOSName = "{{NetBIOSName}}",
    [string]$DSRMPassword = "{{DSRMPassword}}",
    [string]$ForestFunctionalLevel = "{{ForestFunctionalLevel}}",
    [string]$DomainFunctionalLevel = "{{DomainFunctionalLevel}}",
    
    # DNS Parameters
    [bool]$ConfigureDNS = ${{ConfigureDNS}},
    [string]$DNSForwarders = "{{DNSForwarders}}",
    [bool]$CreateReverseLookupZone = ${{CreateReverseLookupZone}},
    
    # Network Parameters
    [bool]$UseDHCP = ${{UseDHCP}},
    [string]$IPAddress = "{{IPAddress}}",
    [string]$SubnetMask = "{{SubnetMask}}",
    [string]$DefaultGateway = "{{DefaultGateway}}",
    [string]$PreferredDNS = "{{PreferredDNS}}",
    
    # Advanced Options
    [bool]$CreateOUStructure = ${{CreateOUStructure}},
    [bool]$ConfigureGPOs = ${{ConfigureGPOs}},
    [bool]$InstallRSAT = ${{InstallRSAT}}
)

# Set up logging
$logFile = "C:\DomainControllerDeployment.log"
function Write-Log {
    param([string]$message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    "$timestamp - $message" | Out-File -FilePath $logFile -Append
    Write-Host "$timestamp - $message"
}

Write-Log "Starting Domain Controller deployment script"
Write-Log "Domain Name: $DomainName"
Write-Log "NetBIOS Name: $NetBIOSName"
Write-Log "Forest Functional Level: $ForestFunctionalLevel"

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
    Write-Log "Installing AD DS role and management tools"
    Install-WindowsFeature -Name AD-Domain-Services -IncludeManagementTools
    
    if ($ConfigureDNS) {
        Write-Log "Installing DNS Server role"
        Install-WindowsFeature -Name DNS -IncludeManagementTools
    }
    
    if ($InstallRSAT) {
        Write-Log "Installing Remote Server Administration Tools"
        Install-WindowsFeature -Name RSAT-ADDS, RSAT-AD-Tools, RSAT-AD-PowerShell
    }
    
    Write-Log "Role installation completed successfully"
}
catch {
    Write-Log "Error installing roles and features: $_"
    throw
}

# Step 3: Create a new AD Forest
try {
    Write-Log "Creating new AD Forest: $DomainName"
    
    # Convert plain text password to secure string
    $securePassword = ConvertTo-SecureString $DSRMPassword -AsPlainText -Force
    
    # Map functional level strings to actual values
    $forestLevel = switch ($ForestFunctionalLevel) {
        "Windows Server 2016" { "WinThreshold" }
        "Windows Server 2019" { "WinThreshold" }
        "Windows Server 2022" { "WinThreshold" }
        default { "WinThreshold" }
    }
    
    $domainLevel = switch ($DomainFunctionalLevel) {
        "Windows Server 2016" { "WinThreshold" }
        "Windows Server 2019" { "WinThreshold" }
        "Windows Server 2022" { "WinThreshold" }
        default { "WinThreshold" }
    }
    
    $installParams = @{
        CreateDnsDelegation = $false
        DatabasePath = "C:\Windows\NTDS"
        DomainMode = $domainLevel
        DomainName = $DomainName
        DomainNetbiosName = $NetBIOSName
        ForestMode = $forestLevel
        InstallDns = $ConfigureDNS
        LogPath = "C:\Windows\NTDS"
        NoRebootOnCompletion = $false
        SafeModeAdministratorPassword = $securePassword
        Force = $true
    }
    
    Install-ADDSForest @installParams
    
    # Note: System will reboot after this command completes successfully
    Write-Log "AD Forest creation initiated. System will reboot."
}
catch {
    Write-Log "Error creating AD Forest: $_"
    throw
}

# Step 4: Post AD DS installation configuration (will run after reboot)
# Create a scheduled task to run the post-configuration
$postConfigScript = @"
# Check if AD DS is installed and configured properly
if ((Get-Service NTDS).Status -eq 'Running') {
    Write-Log "AD DS is running, proceeding with post-configuration"
    
    # Configure DNS if required
    if ($ConfigureDNS) {
        try {
            Write-Log "Configuring DNS Server"
            
            # Add DNS forwarders
            foreach ($forwarder in "$DNSForwarders".Split(',').Trim()) {
                if (-not [string]::IsNullOrWhiteSpace($forwarder)) {
                    Add-DnsServerForwarder -IPAddress $forwarder -PassThru
                }
            }
            
            # Create reverse lookup zone if requested
            if ($CreateReverseLookupZone -and -not $UseDHCP) {
                # Extract network ID from IP address and subnet
                $ipParts = $IPAddress.Split('.')
                $network = "$($ipParts[0]).$($ipParts[1]).$($ipParts[2])"
                
                # Create reverse lookup zone
                Add-DnsServerPrimaryZone -NetworkID "$network.0/24" -ReplicationScope "Forest" -DynamicUpdate "Secure"
            }
            
            Write-Log "DNS Server configuration completed successfully"
        }
        catch {
            Write-Log "Error configuring DNS Server: $_"
        }
    }
    
    # Create OU structure if requested
    if ($CreateOUStructure) {
        try {
            Write-Log "Creating organizational unit structure"
            
            $baseOU = "DC=$($DomainName.Replace('.', ',DC='))"
            
            $ous = @(
                "OU=Users",
                "OU=Computers",
                "OU=Groups",
                "OU=Service Accounts",
                "OU=Servers",
                "OU=Administrative Units"
            )
            
            foreach ($ou in $ous) {
                $ouDN = "$ou,$baseOU"
                
                if (-not (Get-ADOrganizationalUnit -Filter "DistinguishedName -eq '$ouDN'" -ErrorAction SilentlyContinue)) {
                    New-ADOrganizationalUnit -Name $ou.Split('=')[1] -Path $baseOU
                }
            }
            
            Write-Log "OU structure created successfully"
        }
        catch {
            Write-Log "Error creating OU structure: $_"
        }
    }
    
    # Configure default Group Policies if requested
    if ($ConfigureGPOs) {
        try {
            Write-Log "Configuring default Group Policies"
            
            # Update the Default Domain Policy
            $defaultPolicy = Get-GPO -Name "Default Domain Policy"
            
            # Configure password policy
            Set-ADDefaultDomainPasswordPolicy -Identity $DomainName -ComplexityEnabled $true -LockoutDuration "0.0:30:0" -LockoutObservationWindow "0.0:30:0" -LockoutThreshold 5 -MaxPasswordAge "42.0:0:0" -MinPasswordAge "1.0:0:0" -MinPasswordLength 12 -PasswordHistoryCount 24
            
            # Import and link additional GPOs as needed
            
            Write-Log "Group Policy configuration completed successfully"
        }
        catch {
            Write-Log "Error configuring Group Policies: $_"
        }
    }
    
    Write-Log "Domain Controller post-configuration completed successfully"
}
else {
    Write-Log "AD DS is not running, post-configuration skipped"
}
"@

$postConfigScriptPath = "C:\PostADConfig.ps1"
$postConfigScript | Out-File -FilePath $postConfigScriptPath -Force

# Create scheduled task to run after reboot
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-ExecutionPolicy Bypass -File $postConfigScriptPath"
$trigger = New-ScheduledTaskTrigger -AtStartup
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -Hidden
Register-ScheduledTask -TaskName "ADPostConfiguration" -Action $action -Trigger $trigger -Settings $settings -RunLevel Highest -Force

Write-Log "Domain Controller deployment script completed. System will reboot to complete AD DS installation." 
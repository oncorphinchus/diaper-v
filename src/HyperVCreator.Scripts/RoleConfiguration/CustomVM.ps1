# CustomVM.ps1
# Purpose: Implements the Custom VM role functionality with flexible configuration options
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$VMName,
    
    [Parameter(Mandatory=$true)]
    [string]$OSVersion,
    
    [Parameter(Mandatory=$false)]
    [string]$VMDescription = "Custom Virtual Machine",
    
    [Parameter(Mandatory=$false)]
    [int]$CPUCount = 2,
    
    [Parameter(Mandatory=$false)]
    [int]$MemoryGB = 4,
    
    [Parameter(Mandatory=$false)]
    [int]$StorageGB = 80,
    
    [Parameter(Mandatory=$false)]
    [string]$VirtualSwitch = "Default Switch",
    
    [Parameter(Mandatory=$false)]
    [string]$VHDPath = $null,
    
    [Parameter(Mandatory=$false)]
    [string]$ISOPath = $null,
    
    [Parameter(Mandatory=$false)]
    [int]$Generation = 2,
    
    [Parameter(Mandatory=$false)]
    [bool]$EnableSecureBoot = $true,
    
    [Parameter(Mandatory=$false)]
    [string]$IPAddress = $null,
    
    [Parameter(Mandatory=$false)]
    [string]$SubnetMask = "255.255.255.0",
    
    [Parameter(Mandatory=$false)]
    [string]$DefaultGateway = $null,
    
    [Parameter(Mandatory=$false)]
    [string]$DNSServer = $null,
    
    [Parameter(Mandatory=$false)]
    [string]$AdditionalDisk = $null,
    
    [Parameter(Mandatory=$false)]
    [int]$AdditionalDiskSizeGB = 0,
    
    [Parameter(Mandatory=$false)]
    [bool]$AutoStart = $false,
    
    [Parameter(Mandatory=$false)]
    [bool]$UseUnattendXML = $false,
    
    [Parameter(Mandatory=$false)]
    [string]$ProductKey = "",
    
    [Parameter(Mandatory=$false)]
    [string]$ComputerName = "",

    [Parameter(Mandatory=$false)]
    [string]$AdminPassword = "P@ssw0rd",
    
    [Parameter(Mandatory=$false)]
    [int]$TimeZone = 85
)

# Import common functions
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$commonPath = Join-Path (Split-Path -Parent $scriptPath) "Common"
$hypervPath = Join-Path (Split-Path -Parent $scriptPath) "HyperV"
$unattendPath = Join-Path (Split-Path -Parent $scriptPath) "UnattendXML"

# Import required modules
. "$commonPath\ErrorHandling.ps1"
. "$commonPath\Logging.ps1"
. "$hypervPath\CreateVM.ps1"
. "$hypervPath\ConfigureNetwork.ps1"
. "$hypervPath\ConfigureStorage.ps1"

function Initialize-CustomVM {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [hashtable]$Parameters
    )
    
    try {
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 0
            StatusMessage = "Initializing Custom VM deployment..."
        }
        Write-Output $statusUpdate

        # Set ComputerName to VMName if not specified
        if ([string]::IsNullOrEmpty($Parameters.ComputerName)) {
            $Parameters.ComputerName = $Parameters.VMName
        }
        
        # Validate parameters
        $validationResult = Test-CustomVMParameters -Parameters $Parameters
        if (-not $validationResult.Success) {
            throw $validationResult.Message
        }
        
        # Create unattend.xml if needed
        if ($Parameters.UseUnattendXML) {
            $statusUpdate = [PSCustomObject]@{
                Type = "StatusUpdate"
                PercentComplete = 10
                StatusMessage = "Creating unattended installation file..."
            }
            Write-Output $statusUpdate
            
            $unattendXmlPath = "$unattendPath\Custom-$($Parameters.VMName).xml"
            $unattendXmlContent = Create-CustomUnattendXml -Parameters $Parameters
            $unattendXmlContent | Out-File -FilePath $unattendXmlPath -Encoding utf8
            
            $Parameters.UnattendXmlPath = $unattendXmlPath
        }
        
        # Create the VM
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 20
            StatusMessage = "Creating virtual machine..."
        }
        Write-Output $statusUpdate
        
        $vmParams = @{
            VMName = $Parameters.VMName
            CPUCount = $Parameters.CPUCount
            MemoryGB = $Parameters.MemoryGB
            StorageGB = $Parameters.StorageGB
            VirtualSwitch = $Parameters.VirtualSwitch
            VHDPath = $Parameters.VHDPath
            Generation = $Parameters.Generation
            EnableSecureBoot = $Parameters.EnableSecureBoot
        }
        
        $vmResult = & "$hypervPath\CreateVM.ps1" @vmParams
        if (-not $vmResult.Success) {
            throw "Failed to create VM: $($vmResult.Message)"
        }
        
        # Configure network if static IP is provided
        if (-not [string]::IsNullOrEmpty($Parameters.IPAddress)) {
            $statusUpdate = [PSCustomObject]@{
                Type = "StatusUpdate"
                PercentComplete = 50
                StatusMessage = "Configuring network settings..."
            }
            Write-Output $statusUpdate
            
            $networkParams = @{
                VMName = $Parameters.VMName
                IPAddress = $Parameters.IPAddress
                SubnetMask = $Parameters.SubnetMask
                DefaultGateway = $Parameters.DefaultGateway
                DNSServer = $Parameters.DNSServer
            }
            
            $networkResult = & "$hypervPath\ConfigureNetwork.ps1" @networkParams
            if (-not $networkResult.Success) {
                throw "Failed to configure network: $($networkResult.Message)"
            }
        }
        
        # Add additional disk if specified
        if ($Parameters.AdditionalDiskSizeGB -gt 0) {
            $statusUpdate = [PSCustomObject]@{
                Type = "StatusUpdate"
                PercentComplete = 70
                StatusMessage = "Adding additional storage disk..."
            }
            Write-Output $statusUpdate
            
            $additionalDiskPath = if ([string]::IsNullOrEmpty($Parameters.AdditionalDisk)) {
                $vhdFolder = Split-Path -Parent $vmResult.VHDPath
                Join-Path $vhdFolder "$($Parameters.VMName)_Data.vhdx"
            } else {
                $Parameters.AdditionalDisk
            }
            
            $storageParams = @{
                VMName = $Parameters.VMName
                DiskPath = $additionalDiskPath
                SizeGB = $Parameters.AdditionalDiskSizeGB
            }
            
            $storageResult = & "$hypervPath\ConfigureStorage.ps1" @storageParams
            if (-not $storageResult.Success) {
                throw "Failed to add additional disk: $($storageResult.Message)"
            }
        }
        
        # Mount ISO if provided
        if (-not [string]::IsNullOrEmpty($Parameters.ISOPath)) {
            $statusUpdate = [PSCustomObject]@{
                Type = "StatusUpdate"
                PercentComplete = 80
                StatusMessage = "Mounting installation media..."
            }
            Write-Output $statusUpdate
            
            try {
                Add-VMDvdDrive -VMName $Parameters.VMName -Path $Parameters.ISOPath
                Set-VMFirmware -VMName $Parameters.VMName -FirstBootDevice (Get-VMDvdDrive -VMName $Parameters.VMName)
            }
            catch {
                Write-Warning "Failed to mount ISO: $_"
            }
        }
        
        # Configure auto-start if required
        if ($Parameters.AutoStart) {
            $statusUpdate = [PSCustomObject]@{
                Type = "StatusUpdate"
                PercentComplete = 90
                StatusMessage = "Configuring VM startup options..."
            }
            Write-Output $statusUpdate
            
            try {
                Set-VM -Name $Parameters.VMName -AutomaticStartAction Start -AutomaticStopAction ShutDown
            }
            catch {
                Write-Warning "Failed to configure auto-start: $_"
            }
        }
        
        # Final configuration
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 95
            StatusMessage = "Finalizing VM configuration..."
        }
        Write-Output $statusUpdate
        
        try {
            # Set VM description
            Set-VM -Name $Parameters.VMName -Notes $Parameters.VMDescription
            
            # Start VM if installation media is provided
            if (-not [string]::IsNullOrEmpty($Parameters.ISOPath)) {
                Start-VM -Name $Parameters.VMName
            }
        }
        catch {
            Write-Warning "Failed during final configuration: $_"
        }
        
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 100
            StatusMessage = "Custom VM deployment completed successfully."
        }
        Write-Output $statusUpdate
        
        # Return success result
        $result = [PSCustomObject]@{
            Type = "Result"
            Success = $true
            Message = "Custom VM '$($Parameters.VMName)' deployed successfully."
            VMName = $Parameters.VMName
            VMDescription = $Parameters.VMDescription
            OSVersion = $Parameters.OSVersion
        }
        
        Write-Output $result
    }
    catch {
        $errorMessage = "Failed to deploy Custom VM: $_"
        Write-Error $errorMessage
        
        $result = [PSCustomObject]@{
            Type = "Result"
            Success = $false
            Message = $errorMessage
            ErrorDetails = $_
        }
        
        Write-Output $result
    }
}

function Test-CustomVMParameters {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [hashtable]$Parameters
    )
    
    try {
        # Check for required parameters
        if ([string]::IsNullOrEmpty($Parameters.VMName)) {
            throw "VM Name is required."
        }
        
        if ([string]::IsNullOrEmpty($Parameters.OSVersion)) {
            throw "OS Version is required."
        }
        
        # Validate memory and CPU settings
        if ($Parameters.CPUCount -lt 1) {
            throw "CPU Count must be at least 1."
        }
        
        if ($Parameters.MemoryGB -lt 1) {
            throw "Memory must be at least 1 GB."
        }
        
        if ($Parameters.StorageGB -lt 20) {
            throw "Storage must be at least 20 GB."
        }
        
        # Validate Generation
        if ($Parameters.Generation -notin @(1, 2)) {
            throw "VM Generation must be either 1 or 2."
        }
        
        # Validate ISO path if provided
        if (-not [string]::IsNullOrEmpty($Parameters.ISOPath) -and -not (Test-Path $Parameters.ISOPath)) {
            throw "ISO file not found at path: $($Parameters.ISOPath)"
        }
        
        # Validate IP configuration if provided
        if (-not [string]::IsNullOrEmpty($Parameters.IPAddress)) {
            $ipRegex = "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$"
            
            if (-not ($Parameters.IPAddress -match $ipRegex)) {
                throw "Invalid IP address format: $($Parameters.IPAddress)"
            }
            
            if (-not [string]::IsNullOrEmpty($Parameters.DefaultGateway) -and -not ($Parameters.DefaultGateway -match $ipRegex)) {
                throw "Invalid default gateway format: $($Parameters.DefaultGateway)"
            }
            
            if (-not [string]::IsNullOrEmpty($Parameters.DNSServer) -and -not ($Parameters.DNSServer -match $ipRegex)) {
                throw "Invalid DNS server format: $($Parameters.DNSServer)"
            }
        }
        
        # Success result
        return [PSCustomObject]@{
            Success = $true
            Message = "Parameter validation successful."
        }
    }
    catch {
        return [PSCustomObject]@{
            Success = $false
            Message = "Parameter validation failed: $_"
        }
    }
}

function Create-CustomUnattendXml {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [hashtable]$Parameters
    )
    
    # Define Windows Server editions with OS version strings
    $osVersionMap = @{
        "Windows Server 2022 Standard" = "Windows Server 2022 SERVERSTANDARD"
        "Windows Server 2022 Datacenter" = "Windows Server 2022 SERVERDATACENTER"
        "Windows Server 2019 Standard" = "Windows Server 2019 SERVERSTANDARD"
        "Windows Server 2019 Datacenter" = "Windows Server 2019 SERVERDATACENTER"
        "Windows Server 2016 Standard" = "Windows Server 2016 SERVERSTANDARD"
        "Windows Server 2016 Datacenter" = "Windows Server 2016 SERVERDATACENTER"
        "Windows 11 Pro" = "Windows 11 Pro"
        "Windows 11 Enterprise" = "Windows 11 Enterprise"
        "Windows 10 Pro" = "Windows 10 Pro"
        "Windows 10 Enterprise" = "Windows 10 Enterprise"
    }
    
    # Map OS version to edition string or use as-is if not found
    $editionString = $osVersionMap[$Parameters.OSVersion]
    if ([string]::IsNullOrEmpty($editionString)) {
        $editionString = $Parameters.OSVersion
    }
    
    # Generate XML content
    $unattendXml = @"
<?xml version="1.0" encoding="utf-8"?>
<unattend xmlns="urn:schemas-microsoft-com:unattend">
    <servicing>
        <package action="configure">
            <assemblyIdentity name="Microsoft-Windows-Foundation-Package" version="10.0.19041.1" processorArchitecture="amd64" publicKeyToken="31bf3856ad364e35" language="" />
            <selection name="$editionString" state="true" />
        </package>
    </servicing>
    <settings pass="specialize">
        <component name="Microsoft-Windows-Shell-Setup" processorArchitecture="amd64" publicKeyToken="31bf3856ad364e35" language="neutral" versionScope="nonSxS" xmlns:wcm="http://schemas.microsoft.com/WMIConfig/2002/State" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
            <ComputerName>$($Parameters.ComputerName)</ComputerName>
            <RegisteredOrganization>Custom Organization</RegisteredOrganization>
            <RegisteredOwner>Custom Administrator</RegisteredOwner>
            <TimeZone>$($Parameters.TimeZone)</TimeZone>
        </component>
    </settings>
    <settings pass="oobeSystem">
        <component name="Microsoft-Windows-Shell-Setup" processorArchitecture="amd64" publicKeyToken="31bf3856ad364e35" language="neutral" versionScope="nonSxS" xmlns:wcm="http://schemas.microsoft.com/WMIConfig/2002/State" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
            <UserAccounts>
                <AdministratorPassword>
                    <Value>$($Parameters.AdminPassword)</Value>
                    <PlainText>true</PlainText>
                </AdministratorPassword>
            </UserAccounts>
            <OOBE>
                <HideEULAPage>true</HideEULAPage>
                <SkipMachineOOBE>true</SkipMachineOOBE>
                <SkipUserOOBE>true</SkipUserOOBE>
                <HideLocalAccountScreen>true</HideLocalAccountScreen>
                <HideOnlineAccountScreens>true</HideOnlineAccountScreens>
                <HideWirelessSetupInOOBE>true</HideWirelessSetupInOOBE>
                <NetworkLocation>Work</NetworkLocation>
                <ProtectYourPC>1</ProtectYourPC>
            </OOBE>
            <AutoLogon>
                <Password>
                    <Value>$($Parameters.AdminPassword)</Value>
                    <PlainText>true</PlainText>
                </Password>
                <Username>Administrator</Username>
                <Enabled>true</Enabled>
                <LogonCount>1</LogonCount>
            </AutoLogon>
            <FirstLogonCommands>
                <SynchronousCommand wcm:action="add">
                    <CommandLine>powershell.exe -Command "Enable-PSRemoting -Force"</CommandLine>
                    <Description>Enable PowerShell Remoting</Description>
                    <Order>1</Order>
                </SynchronousCommand>
                <SynchronousCommand wcm:action="add">
                    <CommandLine>powershell.exe -Command "Set-ItemProperty -Path 'HKLM:\System\CurrentControlSet\Control\Terminal Server' -Name 'fDenyTSConnections' -Value 0"</CommandLine>
                    <Description>Enable Remote Desktop</Description>
                    <Order>2</Order>
                </SynchronousCommand>
                <SynchronousCommand wcm:action="add">
                    <CommandLine>powershell.exe -Command "Enable-NetFirewallRule -DisplayGroup 'Remote Desktop'"</CommandLine>
                    <Description>Enable Remote Desktop Firewall Rule</Description>
                    <Order>3</Order>
                </SynchronousCommand>
            </FirstLogonCommands>
        </component>
    </settings>
</unattend>
"@

    return $unattendXml
}

# Execute deployment with provided parameters
$params = @{
    VMName = $VMName
    OSVersion = $OSVersion
    VMDescription = $VMDescription
    CPUCount = $CPUCount
    MemoryGB = $MemoryGB
    StorageGB = $StorageGB
    VirtualSwitch = $VirtualSwitch
    VHDPath = $VHDPath
    ISOPath = $ISOPath
    Generation = $Generation
    EnableSecureBoot = $EnableSecureBoot
    IPAddress = $IPAddress
    SubnetMask = $SubnetMask
    DefaultGateway = $DefaultGateway
    DNSServer = $DNSServer
    AdditionalDisk = $AdditionalDisk
    AdditionalDiskSizeGB = $AdditionalDiskSizeGB
    AutoStart = $AutoStart
    UseUnattendXML = $UseUnattendXML
    ProductKey = $ProductKey
    ComputerName = $ComputerName
    AdminPassword = $AdminPassword
    TimeZone = $TimeZone
}

Initialize-CustomVM -Parameters $params 
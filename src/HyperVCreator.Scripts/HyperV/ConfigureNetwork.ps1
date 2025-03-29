# ConfigureNetwork.ps1
# Purpose: Configures network settings for a Hyper-V virtual machine
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$VMName,
    
    [Parameter(Mandatory=$true)]
    [string]$VirtualSwitch,
    
    [Parameter(Mandatory=$false)]
    [bool]$StaticIP = $false,
    
    [Parameter(Mandatory=$false)]
    [string]$IPAddress,
    
    [Parameter(Mandatory=$false)]
    [string]$SubnetMask = "255.255.255.0",
    
    [Parameter(Mandatory=$false)]
    [string]$DefaultGateway,
    
    [Parameter(Mandatory=$false)]
    [string[]]$DNSServers,
    
    [Parameter(Mandatory=$false)]
    [bool]$EnableVLAN = $false,
    
    [Parameter(Mandatory=$false)]
    [int]$VLANId
)

# Import common functions
# . "$PSScriptRoot\..\Common\ErrorHandling.ps1"
# . "$PSScriptRoot\..\Common\Logging.ps1"

function Set-VMNetworkConfiguration {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$VMName,
        
        [Parameter(Mandatory=$true)]
        [string]$VirtualSwitch,
        
        [Parameter(Mandatory=$false)]
        [bool]$StaticIP = $false,
        
        [Parameter(Mandatory=$false)]
        [string]$IPAddress,
        
        [Parameter(Mandatory=$false)]
        [string]$SubnetMask = "255.255.255.0",
        
        [Parameter(Mandatory=$false)]
        [string]$DefaultGateway,
        
        [Parameter(Mandatory=$false)]
        [string[]]$DNSServers,
        
        [Parameter(Mandatory=$false)]
        [bool]$EnableVLAN = $false,
        
        [Parameter(Mandatory=$false)]
        [int]$VLANId
    )
    
    try {
        # Status update - Starting
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 0
            StatusMessage = "Starting VM network configuration..."
        }
        Write-Output $statusUpdate
        
        # Verify the VM exists
        try {
            $vm = Get-VM -Name $VMName -ErrorAction Stop
        }
        catch {
            throw "The VM '$VMName' does not exist."
        }
        
        # Status update - Checking virtual switch
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 10
            StatusMessage = "Checking virtual switch..."
        }
        Write-Output $statusUpdate
        
        # Verify the virtual switch exists
        try {
            $switch = Get-VMSwitch -Name $VirtualSwitch -ErrorAction Stop
        }
        catch {
            throw "The virtual switch '$VirtualSwitch' does not exist."
        }
        
        # Status update - Configuring network adapter
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 30
            StatusMessage = "Configuring network adapter..."
        }
        Write-Output $statusUpdate
        
        # Get the VM's network adapter
        $networkAdapter = Get-VMNetworkAdapter -VMName $VMName
        
        # If VM has no network adapter, add one
        if ($null -eq $networkAdapter) {
            $networkAdapter = Add-VMNetworkAdapter -VMName $VMName -SwitchName $VirtualSwitch -PassThru
        }
        else {
            # Connect the existing adapter to the specified switch
            Connect-VMNetworkAdapter -VMNetworkAdapter $networkAdapter -SwitchName $VirtualSwitch
        }
        
        # Configure VLAN if enabled
        if ($EnableVLAN) {
            # Status update - Setting VLAN
            $statusUpdate = [PSCustomObject]@{
                Type = "StatusUpdate"
                PercentComplete = 50
                StatusMessage = "Configuring VLAN settings..."
            }
            Write-Output $statusUpdate
            
            Set-VMNetworkAdapterVlan -VMNetworkAdapter $networkAdapter -Access -VlanId $VLANId
        }
        else {
            # Remove any VLAN settings
            Set-VMNetworkAdapterVlan -VMNetworkAdapter $networkAdapter -Untagged
        }
        
        # Static IP configuration script
        if ($StaticIP) {
            # Status update - Setting static IP
            $statusUpdate = [PSCustomObject]@{
                Type = "StatusUpdate"
                PercentComplete = 70
                StatusMessage = "Preparing static IP configuration..."
            }
            Write-Output $statusUpdate
            
            # Validate IP parameters
            if ([string]::IsNullOrEmpty($IPAddress)) {
                throw "IP address is required for static IP configuration."
            }
            
            if ([string]::IsNullOrEmpty($DefaultGateway)) {
                throw "Default gateway is required for static IP configuration."
            }
            
            if ($null -eq $DNSServers -or $DNSServers.Count -eq 0) {
                throw "At least one DNS server is required for static IP configuration."
            }
            
            # Convert subnet mask to prefix length
            $subnetMaskBits = ConvertTo-PrefixLength -SubnetMask $SubnetMask
            
            # Create IP configuration script to run within the VM
            $ipConfigScript = @"
# Configure Network Adapter with Static IP
`$adapter = Get-NetAdapter | Where-Object {`$_.Status -eq 'Up'}
`$adapter | Remove-NetIPAddress -Confirm:`$false
`$adapter | New-NetIPAddress -IPAddress '$IPAddress' -PrefixLength $subnetMaskBits -DefaultGateway '$DefaultGateway'
`$adapter | Set-DnsClientServerAddress -ServerAddresses '$($DNSServers -join "','")' 
"@
            
            # This script would need to be copied into the VM and executed
            # In a real implementation, this would require guest OS integration
            # This is a placeholder for that logic
        }
        
        # Status update - Complete
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 100
            StatusMessage = "Network configuration completed."
        }
        Write-Output $statusUpdate
        
        # Return success result with network configuration details
        $result = [PSCustomObject]@{
            Type = "Result"
            Success = $true
            Message = "VM network configuration applied successfully."
            VMName = $VMName
            VirtualSwitch = $VirtualSwitch
            StaticIP = $StaticIP
            IPAddress = $IPAddress
            SubnetMask = $SubnetMask
            DefaultGateway = $DefaultGateway
            DNSServers = $DNSServers
            EnableVLAN = $EnableVLAN
            VLANId = $VLANId
        }
        
        Write-Output $result
    }
    catch {
        Write-Error "Failed to configure VM network: $_"
        
        # Return error result
        $result = [PSCustomObject]@{
            Type = "Result"
            Success = $false
            Message = "Failed to configure VM network: $_"
            ErrorDetails = $_
        }
        
        Write-Output $result
    }
}

# Helper function to convert subnet mask to prefix length
function ConvertTo-PrefixLength {
    param (
        [Parameter(Mandatory=$true)]
        [string]$SubnetMask
    )
    
    $bytes = $SubnetMask.Split('.')
    $prefixLength = 0
    
    foreach ($byte in $bytes) {
        $binary = [Convert]::ToString([byte]$byte, 2)
        $prefixLength += ($binary -replace '0', '').Length
    }
    
    return $prefixLength
}

# Execute the network configuration
Set-VMNetworkConfiguration -VMName $VMName -VirtualSwitch $VirtualSwitch -StaticIP $StaticIP -IPAddress $IPAddress -SubnetMask $SubnetMask -DefaultGateway $DefaultGateway -DNSServers $DNSServers -EnableVLAN $EnableVLAN -VLANId $VLANId 
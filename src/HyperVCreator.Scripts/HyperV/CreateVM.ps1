# CreateVM.ps1
# Purpose: Creates a new Hyper-V virtual machine with specified parameters
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$VMName,
    
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
    [string]$Generation = 2,

    [Parameter(Mandatory=$false)]
    [bool]$EnableSecureBoot = $true
)

# Import common functions
# . "$PSScriptRoot\..\Common\ErrorHandling.ps1"
# . "$PSScriptRoot\..\Common\Logging.ps1"

function New-HyperVVM {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$VMName,
        
        [Parameter(Mandatory=$true)]
        [int]$CPUCount,
        
        [Parameter(Mandatory=$true)]
        [int]$MemoryGB,
        
        [Parameter(Mandatory=$true)]
        [int]$StorageGB,
        
        [Parameter(Mandatory=$true)]
        [string]$VirtualSwitch,
        
        [Parameter(Mandatory=$false)]
        [string]$VHDPath,

        [Parameter(Mandatory=$false)]
        [string]$Generation = 2,

        [Parameter(Mandatory=$false)]
        [bool]$EnableSecureBoot = $true
    )
    
    try {
        # Status update - Starting
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 0
            StatusMessage = "Creating virtual machine..."
        }
        Write-Output $statusUpdate
        
        # Validate parameters
        if ([string]::IsNullOrEmpty($VHDPath)) {
            $VHDPath = "C:\Users\Public\Documents\Hyper-V\Virtual Hard Disks\$VMName.vhdx"
        }
        
        # Check if VM exists
        $existingVM = Get-VM -Name $VMName -ErrorAction SilentlyContinue
        if ($existingVM) {
            throw "A VM with the name '$VMName' already exists."
        }
        
        # Verify that Hyper-V is installed and running
        try {
            $hyperv = Get-Service vmms -ErrorAction Stop
            if ($hyperv.Status -ne 'Running') {
                throw "Hyper-V Virtual Machine Management service is not running."
            }
        }
        catch {
            throw "Hyper-V does not appear to be installed or accessible. Please ensure Hyper-V is properly installed and you have administrator rights."
        }

        # Verify virtual switch exists
        try {
            $switch = Get-VMSwitch -Name $VirtualSwitch -ErrorAction Stop
        }
        catch {
            throw "The virtual switch '$VirtualSwitch' does not exist."
        }
        
        # Status update - Creating VHD
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 10
            StatusMessage = "Creating virtual hard disk..."
        }
        Write-Output $statusUpdate
        
        # Create VHD
        try {
            $vhd = New-VHD -Path $VHDPath -SizeBytes ($StorageGB * 1GB) -Dynamic
        }
        catch {
            throw "Failed to create virtual hard disk: $_"
        }
        
        # Status update - Creating VM
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 30
            StatusMessage = "Creating virtual machine configuration..."
        }
        Write-Output $statusUpdate
        
        # Create VM
        try {
            $vm = New-VM -Name $VMName -MemoryStartupBytes ($MemoryGB * 1GB) -VHDPath $VHDPath -Generation $Generation -SwitchName $VirtualSwitch
        }
        catch {
            # Attempt to clean up the VHD if VM creation fails
            Remove-Item -Path $VHDPath -Force -ErrorAction SilentlyContinue
            throw "Failed to create virtual machine: $_"
        }
        
        # Status update - Configuring VM
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 50
            StatusMessage = "Configuring virtual machine properties..."
        }
        Write-Output $statusUpdate
        
        # Configure VM settings
        try {
            $vm | Set-VMProcessor -Count $CPUCount
            $vm | Set-VMMemory -DynamicMemoryEnabled $true -MinimumBytes (1GB) -MaximumBytes ($MemoryGB * 1GB) -StartupBytes ($MemoryGB * 1GB)
            
            # Enable Secure Boot for Generation 2 VMs
            if ($Generation -eq 2 -and $EnableSecureBoot) {
                $vm | Set-VMFirmware -EnableSecureBoot On
            }
        }
        catch {
            # Attempt to clean up if configuration fails
            Remove-VM -Name $VMName -Force -ErrorAction SilentlyContinue
            Remove-Item -Path $VHDPath -Force -ErrorAction SilentlyContinue
            throw "Failed to configure virtual machine: $_"
        }
        
        # Status update - Completing
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 90
            StatusMessage = "Finalizing virtual machine creation..."
        }
        Write-Output $statusUpdate
        
        # Final VM check
        $createdVM = Get-VM -Name $VMName
        
        # Status update - Complete
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 100
            StatusMessage = "Virtual machine created successfully."
        }
        Write-Output $statusUpdate
        
        # Return success result
        $result = [PSCustomObject]@{
            Type = "Result"
            Success = $true
            Message = "VM '$VMName' created successfully."
            VMName = $VMName
            VHDPath = $VHDPath
            CPUCount = $CPUCount
            MemoryGB = $MemoryGB
            VirtualSwitch = $VirtualSwitch
        }
        
        Write-Output $result
    }
    catch {
        Write-Error "Failed to create VM: $_"
        
        # Return error result
        $result = [PSCustomObject]@{
            Type = "Result"
            Success = $false
            Message = "Failed to create VM: $_"
            ErrorDetails = $_
        }
        
        Write-Output $result
    }
}

# Execute the VM creation
New-HyperVVM -VMName $VMName -CPUCount $CPUCount -MemoryGB $MemoryGB -StorageGB $StorageGB -VirtualSwitch $VirtualSwitch -VHDPath $VHDPath -Generation $Generation -EnableSecureBoot $EnableSecureBoot 
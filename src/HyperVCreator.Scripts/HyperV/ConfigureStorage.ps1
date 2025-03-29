# ConfigureStorage.ps1
# Purpose: Configures storage settings for a Hyper-V virtual machine
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$VMName,
    
    [Parameter(Mandatory=$false)]
    [string]$PrimaryVHDPath,
    
    [Parameter(Mandatory=$false)]
    [int]$PrimaryVHDSizeGB,
    
    [Parameter(Mandatory=$false)]
    [string[]]$AdditionalVHDPaths,
    
    [Parameter(Mandatory=$false)]
    [int[]]$AdditionalVHDSizesGB,
    
    [Parameter(Mandatory=$false)]
    [string]$ISOPath,
    
    [Parameter(Mandatory=$false)]
    [bool]$UseDynamicDisks = $true,
    
    [Parameter(Mandatory=$false)]
    [string]$ControllerType = "SCSI"
)

# Import common functions
# . "$PSScriptRoot\..\Common\ErrorHandling.ps1"
# . "$PSScriptRoot\..\Common\Logging.ps1"

function Set-VMStorageConfiguration {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$VMName,
        
        [Parameter(Mandatory=$false)]
        [string]$PrimaryVHDPath,
        
        [Parameter(Mandatory=$false)]
        [int]$PrimaryVHDSizeGB,
        
        [Parameter(Mandatory=$false)]
        [string[]]$AdditionalVHDPaths,
        
        [Parameter(Mandatory=$false)]
        [int[]]$AdditionalVHDSizesGB,
        
        [Parameter(Mandatory=$false)]
        [string]$ISOPath,
        
        [Parameter(Mandatory=$false)]
        [bool]$UseDynamicDisks = $true,
        
        [Parameter(Mandatory=$false)]
        [string]$ControllerType = "SCSI"
    )
    
    try {
        # Status update - Starting
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 0
            StatusMessage = "Starting VM storage configuration..."
        }
        Write-Output $statusUpdate
        
        # Verify the VM exists
        try {
            $vm = Get-VM -Name $VMName -ErrorAction Stop
        }
        catch {
            throw "The VM '$VMName' does not exist."
        }
        
        # Configure primary VHD if specified
        if (-not [string]::IsNullOrEmpty($PrimaryVHDPath) -and $PrimaryVHDSizeGB -gt 0) {
            # Status update - Primary VHD
            $statusUpdate = [PSCustomObject]@{
                Type = "StatusUpdate"
                PercentComplete = 10
                StatusMessage = "Configuring primary virtual hard disk..."
            }
            Write-Output $statusUpdate
            
            # Check if file exists
            if (Test-Path $PrimaryVHDPath) {
                # Resize existing VHD
                $currentVHD = Get-VHD -Path $PrimaryVHDPath
                
                if ($currentVHD.Size -lt $PrimaryVHDSizeGB * 1GB) {
                    Resize-VHD -Path $PrimaryVHDPath -SizeBytes ($PrimaryVHDSizeGB * 1GB)
                }
            }
            else {
                # Create new VHD
                if ($UseDynamicDisks) {
                    New-VHD -Path $PrimaryVHDPath -SizeBytes ($PrimaryVHDSizeGB * 1GB) -Dynamic
                }
                else {
                    New-VHD -Path $PrimaryVHDPath -SizeBytes ($PrimaryVHDSizeGB * 1GB) -Fixed
                }
                
                # Attach to VM
                Add-VMHardDiskDrive -VMName $VMName -Path $PrimaryVHDPath -ControllerType $ControllerType
            }
        }
        
        # Add additional VHDs if specified
        if ($null -ne $AdditionalVHDPaths -and $AdditionalVHDPaths.Count -gt 0) {
            # Status update - Additional VHDs
            $statusUpdate = [PSCustomObject]@{
                Type = "StatusUpdate"
                PercentComplete = 30
                StatusMessage = "Adding additional virtual hard disks..."
            }
            Write-Output $statusUpdate
            
            # Verify that we have sizes for each VHD
            if ($null -eq $AdditionalVHDSizesGB -or $AdditionalVHDSizesGB.Count -ne $AdditionalVHDPaths.Count) {
                throw "Number of additional VHD sizes must match number of additional VHD paths."
            }
            
            # Process each additional VHD
            for ($i = 0; $i -lt $AdditionalVHDPaths.Count; $i++) {
                $vhdPath = $AdditionalVHDPaths[$i]
                $vhdSize = $AdditionalVHDSizesGB[$i]
                
                # Status update for each VHD
                $statusUpdate = [PSCustomObject]@{
                    Type = "StatusUpdate"
                    PercentComplete = (30 + (50 * $i / $AdditionalVHDPaths.Count))
                    StatusMessage = "Creating additional VHD: $vhdPath"
                }
                Write-Output $statusUpdate
                
                # Create the VHD if it doesn't exist
                if (-not (Test-Path $vhdPath)) {
                    if ($UseDynamicDisks) {
                        New-VHD -Path $vhdPath -SizeBytes ($vhdSize * 1GB) -Dynamic
                    }
                    else {
                        New-VHD -Path $vhdPath -SizeBytes ($vhdSize * 1GB) -Fixed
                    }
                    
                    # Attach the VHD to the VM
                    Add-VMHardDiskDrive -VMName $VMName -Path $vhdPath -ControllerType $ControllerType
                }
                else {
                    Write-Warning "VHD at path $vhdPath already exists. Skipping creation."
                    
                    # Check if already attached
                    $isAttached = $false
                    $drives = Get-VMHardDiskDrive -VMName $VMName
                    
                    foreach ($drive in $drives) {
                        if ($drive.Path -eq $vhdPath) {
                            $isAttached = $true
                            break
                        }
                    }
                    
                    if (-not $isAttached) {
                        # Attach the existing VHD to the VM
                        Add-VMHardDiskDrive -VMName $VMName -Path $vhdPath -ControllerType $ControllerType
                    }
                }
            }
        }
        
        # Mount ISO if specified
        if (-not [string]::IsNullOrEmpty($ISOPath)) {
            # Status update - ISO
            $statusUpdate = [PSCustomObject]@{
                Type = "StatusUpdate"
                PercentComplete = 80
                StatusMessage = "Mounting ISO image..."
            }
            Write-Output $statusUpdate
            
            # Check if ISO exists
            if (-not (Test-Path $ISOPath)) {
                throw "ISO file not found at path: $ISOPath"
            }
            
            # Get DVD drives
            $dvdDrive = Get-VMDvdDrive -VMName $VMName
            
            if ($null -eq $dvdDrive) {
                # Add DVD drive if none exists
                $dvdDrive = Add-VMDvdDrive -VMName $VMName -Path $ISOPath -PassThru
            }
            else {
                # Mount ISO to existing DVD drive
                $dvdDrive | Set-VMDvdDrive -Path $ISOPath
            }
        }
        
        # Status update - Complete
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 100
            StatusMessage = "Storage configuration completed."
        }
        Write-Output $statusUpdate
        
        # Return success result with storage configuration details
        $result = [PSCustomObject]@{
            Type = "Result"
            Success = $true
            Message = "VM storage configuration applied successfully."
            VMName = $VMName
            PrimaryVHDPath = $PrimaryVHDPath
            PrimaryVHDSizeGB = $PrimaryVHDSizeGB
            AdditionalVHDPaths = $AdditionalVHDPaths
            AdditionalVHDSizesGB = $AdditionalVHDSizesGB
            ISOPath = $ISOPath
            UseDynamicDisks = $UseDynamicDisks
        }
        
        Write-Output $result
    }
    catch {
        Write-Error "Failed to configure VM storage: $_"
        
        # Return error result
        $result = [PSCustomObject]@{
            Type = "Result"
            Success = $false
            Message = "Failed to configure VM storage: $_"
            ErrorDetails = $_
        }
        
        Write-Output $result
    }
}

# Execute the storage configuration
Set-VMStorageConfiguration -VMName $VMName -PrimaryVHDPath $PrimaryVHDPath -PrimaryVHDSizeGB $PrimaryVHDSizeGB -AdditionalVHDPaths $AdditionalVHDPaths -AdditionalVHDSizesGB $AdditionalVHDSizesGB -ISOPath $ISOPath -UseDynamicDisks $UseDynamicDisks -ControllerType $ControllerType 
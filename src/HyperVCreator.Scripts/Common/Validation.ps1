<#
.SYNOPSIS
    Validation functions for the HyperV VM Creation Application.

.DESCRIPTION
    This script contains common validation functions used across the PowerShell scripts
    in the HyperV VM Creation Application.

.NOTES
    File Name      : Validation.ps1
    Author         : HyperV Creator Team
    Prerequisite   : PowerShell 5.1 or later
#>

# Source error handling functions
. "$PSScriptRoot\ErrorHandling.ps1"

#region Functions

<#
.SYNOPSIS
    Validates VM name for use with Hyper-V.

.DESCRIPTION
    Checks that a VM name meets the requirements for Hyper-V VM names.

.PARAMETER VMName
    The name to validate.

.PARAMETER AllowExisting
    Whether to allow the VM to already exist. Default is $false.

.RETURNS
    A validation result object with Success and Message properties.

.EXAMPLE
    $result = Test-VMName -VMName "TestVM"
    if (-not $result.Success) {
        Write-Error $result.Message
    }
#>
function Test-VMName {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$VMName,
        
        [Parameter(Mandatory=$false)]
        [bool]$AllowExisting = $false
    )
    
    process {
        # Initialize result object
        $result = [PSCustomObject]@{
            Success = $true
            Message = "VM name is valid."
        }
        
        # Check for null or empty name
        if ([string]::IsNullOrEmpty($VMName)) {
            $result.Success = $false
            $result.Message = "VM name cannot be empty."
            return $result
        }
        
        # Check name length
        if ($VMName.Length -gt 64) {
            $result.Success = $false
            $result.Message = "VM name cannot exceed 64 characters."
            return $result
        }
        
        # Check for invalid characters
        $invalidChars = [IO.Path]::GetInvalidFileNameChars()
        $invalidChars += @('<', '>', '|', '?', '*')
        
        foreach ($char in $invalidChars) {
            if ($VMName.Contains($char)) {
                $result.Success = $false
                $result.Message = "VM name contains invalid character: $char"
                return $result
            }
        }
        
        # Check if VM already exists
        if (-not $AllowExisting) {
            try {
                $existingVM = Get-VM -Name $VMName -ErrorAction SilentlyContinue
                if ($null -ne $existingVM) {
                    $result.Success = $false
                    $result.Message = "A VM with the name '$VMName' already exists."
                    return $result
                }
            }
            catch {
                # Ignore errors when checking for VM existence
            }
        }
        
        return $result
    }
}

<#
.SYNOPSIS
    Validates hardware parameters for a Hyper-V VM.

.DESCRIPTION
    Checks that hardware configuration parameters are valid for creating a Hyper-V VM.

.PARAMETER CPUCount
    The number of virtual CPUs.

.PARAMETER MemoryGB
    The amount of memory in GB.

.PARAMETER StorageGB
    The size of the virtual hard disk in GB.

.EXAMPLE
    $result = Test-VMHardware -CPUCount 2 -MemoryGB 4 -StorageGB 80
    if (-not $result.IsValid) {
        Write-Error $result.Message
    }
#>
function Test-VMHardware {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [int]$CPUCount,
        
        [Parameter(Mandatory=$true)]
        [int]$MemoryGB,
        
        [Parameter(Mandatory=$true)]
        [int]$StorageGB
    )
    
    $result = [PSCustomObject]@{
        IsValid = $true
        Message = ""
    }
    
    # Validate CPU count
    if ($CPUCount -lt 1) {
        $result.IsValid = $false
        $result.Message = "CPU count must be at least 1"
        return $result
    }
    elseif ($CPUCount -gt 64) {
        $result.IsValid = $false
        $result.Message = "CPU count exceeds maximum allowed (64)"
        return $result
    }
    
    # Validate memory
    if ($MemoryGB -lt 1) {
        $result.IsValid = $false
        $result.Message = "Memory must be at least 1 GB"
        return $result
    }
    elseif ($MemoryGB -gt 1024) {
        $result.IsValid = $false
        $result.Message = "Memory exceeds maximum allowed (1024 GB)"
        return $result
    }
    
    # Validate storage
    if ($StorageGB -lt 8) {
        $result.IsValid = $false
        $result.Message = "Storage must be at least 8 GB"
        return $result
    }
    elseif ($StorageGB -gt 64 * 1024) {
        $result.IsValid = $false
        $result.Message = "Storage exceeds maximum allowed (64 TB)"
        return $result
    }
    
    return $result
}

<#
.SYNOPSIS
    Validates a virtual switch name.

.DESCRIPTION
    Checks that a specified virtual switch exists on the Hyper-V host.

.PARAMETER SwitchName
    The name of the virtual switch.

.EXAMPLE
    $result = Test-VirtualSwitch -SwitchName "Default Switch"
    if (-not $result.IsValid) {
        Write-Error $result.Message
    }
#>
function Test-VirtualSwitch {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$SwitchName
    )
    
    $result = [PSCustomObject]@{
        IsValid = $true
        Message = ""
    }
    
    if ([string]::IsNullOrWhiteSpace($SwitchName)) {
        $result.IsValid = $false
        $result.Message = "Switch name cannot be empty"
        return $result
    }
    
    # Check if the switch exists
    try {
        $switch = Get-VMSwitch -Name $SwitchName -ErrorAction SilentlyContinue
        if (-not $switch) {
            $result.IsValid = $false
            $result.Message = "Virtual switch '$SwitchName' does not exist"
        }
    }
    catch {
        $result.IsValid = $false
        $result.Message = "Error checking virtual switch: $_"
    }
    
    return $result
}

<#
.SYNOPSIS
    Validates a domain name.

.DESCRIPTION
    Checks that a domain name follows proper formatting rules.

.PARAMETER DomainName
    The domain name to validate.

.EXAMPLE
    $result = Test-DomainName -DomainName "contoso.local"
    if (-not $result.IsValid) {
        Write-Error $result.Message
    }
#>
function Test-DomainName {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$DomainName
    )
    
    $result = [PSCustomObject]@{
        IsValid = $true
        Message = ""
    }
    
    if ([string]::IsNullOrWhiteSpace($DomainName)) {
        $result.IsValid = $false
        $result.Message = "Domain name cannot be empty"
        return $result
    }
    
    # Basic domain name pattern
    $pattern = "^([a-z0-9]+(-[a-z0-9]+)*\.)+[a-z]{2,}$"
    
    if (-not ($DomainName -match $pattern)) {
        $result.IsValid = $false
        $result.Message = "Domain name format is invalid (example: contoso.local)"
        return $result
    }
    
    return $result
}

<#
.SYNOPSIS
    Validates a password for complexity requirements.

.DESCRIPTION
    Checks that a password meets complexity requirements.

.PARAMETER Password
    The password to validate.

.PARAMETER MinLength
    Minimum required length. Default is 8.

.PARAMETER RequireComplexity
    Whether to enforce complexity requirements (uppercase, lowercase, digit, special char). Default is $true.

.EXAMPLE
    $result = Test-Password -Password "P@ssw0rd"
    if (-not $result.IsValid) {
        Write-Error $result.Message
    }
#>
function Test-Password {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$Password,
        
        [Parameter(Mandatory=$false)]
        [int]$MinLength = 8,
        
        [Parameter(Mandatory=$false)]
        [bool]$RequireComplexity = $true
    )
    
    $result = [PSCustomObject]@{
        IsValid = $true
        Message = ""
    }
    
    if ([string]::IsNullOrEmpty($Password)) {
        $result.IsValid = $false
        $result.Message = "Password cannot be empty"
        return $result
    }
    
    # Check length
    if ($Password.Length -lt $MinLength) {
        $result.IsValid = $false
        $result.Message = "Password must be at least $MinLength characters long"
        return $result
    }
    
    # Check complexity
    if ($RequireComplexity) {
        $hasUpper = $Password -cmatch "[A-Z]"
        $hasLower = $Password -cmatch "[a-z]"
        $hasDigit = $Password -match "\d"
        $hasSpecial = $Password -match "[^\w\d]"
        
        if (-not ($hasUpper -and $hasLower -and $hasDigit -and $hasSpecial)) {
            $result.IsValid = $false
            $result.Message = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character"
            return $result
        }
    }
    
    return $result
}

<#
.SYNOPSIS
    Validates IP address format.

.DESCRIPTION
    Checks that a string is a valid IPv4 address.

.PARAMETER IPAddress
    The IP address to validate.

.EXAMPLE
    $result = Test-IPAddress -IPAddress "192.168.1.100"
    if (-not $result.IsValid) {
        Write-Error $result.Message
    }
#>
function Test-IPAddress {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$IPAddress
    )
    
    $result = [PSCustomObject]@{
        IsValid = $true
        Message = ""
    }
    
    # Try to parse as IPv4 address
    [System.Net.IPAddress]$parsedIP = $null
    if (-not [System.Net.IPAddress]::TryParse($IPAddress, [ref]$parsedIP)) {
        $result.IsValid = $false
        $result.Message = "Invalid IP address format"
        return $result
    }
    
    # Ensure it's IPv4
    if ($parsedIP.AddressFamily -ne [System.Net.Sockets.AddressFamily]::InterNetwork) {
        $result.IsValid = $false
        $result.Message = "IP address must be IPv4 format"
        return $result
    }
    
    return $result
}

#endregion

# Export functions
Export-ModuleMember -Function Test-VMName, Test-VMHardware, Test-VirtualSwitch, Test-DomainName, Test-Password, Test-IPAddress 
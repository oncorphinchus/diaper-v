# Test-CustomVMConfig.ps1
# Purpose: Test script for Custom VM role configuration
[CmdletBinding()]
param()

# Script paths
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$roleConfigPath = Join-Path $scriptPath "RoleConfiguration"
$customVMScript = Join-Path $roleConfigPath "CustomVM.ps1"

# Test parameters for minimal configuration
$minimalParams = @{
    VMName = "Test-CustomVM-Minimal"
    OSVersion = "Windows Server 2022 Standard"
}

# Test parameters for full configuration
$fullParams = @{
    VMName = "Test-CustomVM-Full"
    VMDescription = "Test Custom VM with full configuration"
    OSVersion = "Windows Server 2022 Standard"
    CPUCount = 4
    MemoryGB = 8
    StorageGB = 100
    VirtualSwitch = "Default Switch"
    IPAddress = "192.168.1.100"
    SubnetMask = "255.255.255.0"
    DefaultGateway = "192.168.1.1"
    DNSServer = "192.168.1.1"
    AdditionalDiskSizeGB = 50
    UseUnattendXML = $true
    ComputerName = "TESTCUSTOMVM"
    AdminPassword = "P@ssw0rd123"
    TimeZone = 85
    AutoStart = $true
}

# Define mock ISO path for testing purposes
$mockISOPath = "C:\temp\test.iso"

# Function to test parameter validation
function Test-ParameterValidation {
    Write-Host "Testing parameter validation..." -ForegroundColor Cyan
    
    # Valid parameters test
    $validParameters = @{
        VMName = "Test-Valid"
        OSVersion = "Windows Server 2022 Standard"
        CPUCount = 2
        MemoryGB = 4
        StorageGB = 80
    }
    
    # Call Test-CustomVMParameters directly
    $validResult = & {
        # Load the script content
        . $customVMScript
        
        # Call the parameter validation function
        Test-CustomVMParameters -Parameters $validParameters
    }
    
    Write-Host "Valid parameters test: $($validResult.Success)" -ForegroundColor $(if ($validResult.Success) { "Green" } else { "Red" })
    
    # Invalid parameters tests
    $invalidTests = @(
        @{
            Name = "Missing VM Name"
            Parameters = @{
                OSVersion = "Windows Server 2022 Standard"
            }
        },
        @{
            Name = "Missing OS Version"
            Parameters = @{
                VMName = "Test-Invalid"
            }
        },
        @{
            Name = "Invalid CPU Count"
            Parameters = @{
                VMName = "Test-Invalid"
                OSVersion = "Windows Server 2022 Standard"
                CPUCount = 0
            }
        },
        @{
            Name = "Invalid Memory"
            Parameters = @{
                VMName = "Test-Invalid"
                OSVersion = "Windows Server 2022 Standard"
                MemoryGB = 0
            }
        },
        @{
            Name = "Invalid Storage"
            Parameters = @{
                VMName = "Test-Invalid"
                OSVersion = "Windows Server 2022 Standard"
                StorageGB = 10
            }
        },
        @{
            Name = "Invalid Generation"
            Parameters = @{
                VMName = "Test-Invalid"
                OSVersion = "Windows Server 2022 Standard"
                Generation = 3
            }
        },
        @{
            Name = "Invalid IP Address"
            Parameters = @{
                VMName = "Test-Invalid"
                OSVersion = "Windows Server 2022 Standard"
                IPAddress = "300.168.1.1"
            }
        }
    )
    
    foreach ($test in $invalidTests) {
        $invalidResult = & {
            # Load the script content
            . $customVMScript
            
            # Call the parameter validation function
            Test-CustomVMParameters -Parameters $test.Parameters
        }
        
        Write-Host "$($test.Name) test: $(-not $invalidResult.Success)" -ForegroundColor $(if (-not $invalidResult.Success) { "Green" } else { "Red" })
    }
}

# Function to test unattend.xml generation
function Test-UnattendXMLGeneration {
    Write-Host "Testing unattend.xml generation..." -ForegroundColor Cyan
    
    $unattendParams = @{
        VMName = "Test-UnattendXML"
        OSVersion = "Windows Server 2022 Standard"
        ComputerName = "TESTVM"
        AdminPassword = "P@ssw0rd123"
        TimeZone = 85
    }
    
    $unattendXml = & {
        # Load the script content
        . $customVMScript
        
        # Call the unattend.xml generation function
        Create-CustomUnattendXml -Parameters $unattendParams
    }
    
    $success = $unattendXml -match "<unattend" -and 
              $unattendXml -match "<ComputerName>TESTVM</ComputerName>" -and
              $unattendXml -match "<Value>P@ssw0rd123</Value>"
    
    Write-Host "Unattend.xml generation test: $success" -ForegroundColor $(if ($success) { "Green" } else { "Red" })
    
    # Test different OS versions
    $osVersions = @(
        "Windows Server 2019 Standard",
        "Windows Server 2016 Datacenter",
        "Windows 10 Pro"
    )
    
    foreach ($osVersion in $osVersions) {
        $unattendParams.OSVersion = $osVersion
        
        $unattendXml = & {
            # Load the script content
            . $customVMScript
            
            # Call the unattend.xml generation function
            Create-CustomUnattendXml -Parameters $unattendParams
        }
        
        $success = $unattendXml -match "<unattend" -and 
                  $unattendXml -match "<selection name=`".*$osVersion.*`" state=`"true`" />"
        
        Write-Host "Unattend.xml generation for '$osVersion' test: $success" -ForegroundColor $(if ($success) { "Green" } else { "Red" })
    }
}

# Mock CreateVM.ps1 for testing without actual VM creation
function Mock-VMCreation {
    Write-Host "Mocking VM creation for testing..." -ForegroundColor Cyan
    
    # Create a temporary mock version of CreateVM.ps1
    $mockCreateVMPath = Join-Path $scriptPath "Mock-CreateVM.ps1"
    
    @"
# Mock-CreateVM.ps1
# This is a mock version for testing
[CmdletBinding()]
param (
    [Parameter(Mandatory=`$true)]
    [string]`$VMName,
    
    [Parameter(Mandatory=`$false)]
    [int]`$CPUCount = 2,
    
    [Parameter(Mandatory=`$false)]
    [int]`$MemoryGB = 4,
    
    [Parameter(Mandatory=`$false)]
    [int]`$StorageGB = 80,
    
    [Parameter(Mandatory=`$false)]
    [string]`$VirtualSwitch = "Default Switch",
    
    [Parameter(Mandatory=`$false)]
    [string]`$VHDPath = `$null,
    
    [Parameter(Mandatory=`$false)]
    [string]`$Generation = 2,

    [Parameter(Mandatory=`$false)]
    [bool]`$EnableSecureBoot = `$true
)

# Return a success result
`$result = [PSCustomObject]@{
    Type = "Result"
    Success = `$true
    Message = "VM '`$VMName' created successfully (mock)."
    VMName = `$VMName
    VHDPath = if ([string]::IsNullOrEmpty(`$VHDPath)) { "C:\Users\Public\Documents\Hyper-V\Virtual Hard Disks\`$VMName.vhdx" } else { `$VHDPath }
    CPUCount = `$CPUCount
    MemoryGB = `$MemoryGB
    VirtualSwitch = `$VirtualSwitch
}

Write-Output `$result
"@ | Out-File -FilePath $mockCreateVMPath -Encoding utf8
    
    # Also create mock network and storage scripts
    $mockConfigureNetworkPath = Join-Path $scriptPath "Mock-ConfigureNetwork.ps1"
    
    @"
# Mock-ConfigureNetwork.ps1
[CmdletBinding()]
param (
    [Parameter(Mandatory=`$true)]
    [string]`$VMName,
    
    [Parameter(Mandatory=`$true)]
    [string]`$IPAddress,
    
    [Parameter(Mandatory=`$false)]
    [string]`$SubnetMask = "255.255.255.0",
    
    [Parameter(Mandatory=`$false)]
    [string]`$DefaultGateway = `$null,
    
    [Parameter(Mandatory=`$false)]
    [string]`$DNSServer = `$null
)

# Return a success result
`$result = [PSCustomObject]@{
    Type = "Result"
    Success = `$true
    Message = "Network configuration for VM '`$VMName' completed successfully (mock)."
    VMName = `$VMName
    IPAddress = `$IPAddress
    SubnetMask = `$SubnetMask
    DefaultGateway = `$DefaultGateway
    DNSServer = `$DNSServer
}

Write-Output `$result
"@ | Out-File -FilePath $mockConfigureNetworkPath -Encoding utf8
    
    $mockConfigureStoragePath = Join-Path $scriptPath "Mock-ConfigureStorage.ps1"
    
    @"
# Mock-ConfigureStorage.ps1
[CmdletBinding()]
param (
    [Parameter(Mandatory=`$true)]
    [string]`$VMName,
    
    [Parameter(Mandatory=`$true)]
    [string]`$DiskPath,
    
    [Parameter(Mandatory=`$true)]
    [int]`$SizeGB
)

# Return a success result
`$result = [PSCustomObject]@{
    Type = "Result"
    Success = `$true
    Message = "Additional disk for VM '`$VMName' added successfully (mock)."
    VMName = `$VMName
    DiskPath = `$DiskPath
    SizeGB = `$SizeGB
}

Write-Output `$result
"@ | Out-File -FilePath $mockConfigureStoragePath -Encoding utf8

    # Return paths of mock scripts
    return @{
        CreateVM = $mockCreateVMPath
        ConfigureNetwork = $mockConfigureNetworkPath
        ConfigureStorage = $mockConfigureStoragePath
    }
}

# Function to test VM creation with modified script
function Test-CustomVMCreation {
    param (
        [Parameter(Mandatory=$true)]
        [hashtable]$MockScripts,
        
        [Parameter(Mandatory=$true)]
        [hashtable]$Parameters,
        
        [Parameter(Mandatory=$true)]
        [string]$TestName
    )
    
    Write-Host "Testing $TestName..." -ForegroundColor Cyan
    
    # Create a temporary modified version of CustomVM.ps1
    $tempCustomVMPath = Join-Path $scriptPath "Temp-CustomVM.ps1"
    
    # Load the original script content
    $originalContent = Get-Content -Path $customVMScript -Raw
    
    # Modify the paths to use our mock scripts
    $modifiedContent = $originalContent -replace '"\$hypervPath\\CreateVM.ps1"', "'$($MockScripts.CreateVM)'" `
                                     -replace '"\$hypervPath\\ConfigureNetwork.ps1"', "'$($MockScripts.ConfigureNetwork)'" `
                                     -replace '"\$hypervPath\\ConfigureStorage.ps1"', "'$($MockScripts.ConfigureStorage)'"
    
    # Save the modified content
    $modifiedContent | Out-File -FilePath $tempCustomVMPath -Encoding utf8
    
    # Execute the test with the modified script
    $result = & {
        # Load the modified script
        . $tempCustomVMPath
        
        # Initialize the VM with provided parameters
        Initialize-CustomVM -Parameters $Parameters
    }
    
    # Check the result
    $success = $result.Success
    
    Write-Host "$TestName result: $success" -ForegroundColor $(if ($success) { "Green" } else { "Red" })
    
    # Cleanup
    Remove-Item -Path $tempCustomVMPath -Force
    
    return $success
}

# Function to cleanup temporary files
function Cleanup-MockFiles {
    param (
        [Parameter(Mandatory=$true)]
        [hashtable]$MockScripts
    )
    
    foreach ($script in $MockScripts.Values) {
        if (Test-Path $script) {
            Remove-Item -Path $script -Force
        }
    }
}

# Main test execution
Write-Host "Starting Custom VM role tests..." -ForegroundColor Yellow

# Run parameter validation tests
Test-ParameterValidation

# Run unattend.xml generation tests
Test-UnattendXMLGeneration

# Create mock scripts for VM creation
$mockScripts = Mock-VMCreation

# Run minimal configuration test
$minimalSuccess = Test-CustomVMCreation -MockScripts $mockScripts -Parameters $minimalParams -TestName "Minimal configuration"

# Add mock ISO path to full parameters
$fullParams.ISOPath = $mockISOPath

# Run full configuration test
$fullSuccess = Test-CustomVMCreation -MockScripts $mockScripts -Parameters $fullParams -TestName "Full configuration"

# Cleanup mock files
Cleanup-MockFiles -MockScripts $mockScripts

# Summary
Write-Host "`nTest Summary:" -ForegroundColor Yellow
Write-Host "Parameter validation tests completed"
Write-Host "Unattend.xml generation tests completed"
Write-Host "Minimal configuration test: $(if ($minimalSuccess) { 'Passed' } else { 'Failed' })" -ForegroundColor $(if ($minimalSuccess) { "Green" } else { "Red" })
Write-Host "Full configuration test: $(if ($fullSuccess) { 'Passed' } else { 'Failed' })" -ForegroundColor $(if ($fullSuccess) { "Green" } else { "Red" })

# Overall result
$overallSuccess = $minimalSuccess -and $fullSuccess
Write-Host "`nOverall test result: $(if ($overallSuccess) { 'PASSED' } else { 'FAILED' })" -ForegroundColor $(if ($overallSuccess) { "Green" } else { "Red" }) 
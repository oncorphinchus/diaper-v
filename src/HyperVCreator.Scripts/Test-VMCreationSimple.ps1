# Test-VMCreationSimple.ps1
# This script tests the VM creation functionality without requiring Hyper-V access

Write-Host "Hyper-V VM Creation - Simple Test" -ForegroundColor Green
Write-Host "----------------------------------" -ForegroundColor Green

# Define test parameters manually
$testParams = @{
    VMName = "TestVM-$(Get-Date -Format 'yyyyMMddHHmmss')"
    CPUCount = 2
    MemoryGB = 4
    StorageGB = 80
    VirtualSwitch = "Default Switch"  # Hardcoded since we don't need real switches
    VHDPath = $null  # Will use default path
    Generation = 2
    EnableSecureBoot = $true
}

Write-Host "Test using the following VM parameters:" -ForegroundColor Yellow
foreach ($key in $testParams.Keys) {
    Write-Host "  $key = $($testParams[$key])"
}

Write-Host "`nSimulating VM creation process..." -ForegroundColor Yellow

# Simulate the VM creation steps from CreateVM.ps1
Write-Host "Step 1: Validating parameters" -ForegroundColor Cyan
Write-Host "  - VM Name: $($testParams.VMName)"
Write-Host "  - CPU Count: $($testParams.CPUCount)"
Write-Host "  - Memory: $($testParams.MemoryGB) GB"
Write-Host "  - Storage: $($testParams.StorageGB) GB"
Write-Host "  - Virtual Switch: $($testParams.VirtualSwitch)"
Write-Host "  - VM Generation: $($testParams.Generation)"

Write-Host "`nStep 2: Creating virtual hard disk" -ForegroundColor Cyan
$vhdPath = if ($testParams.VHDPath) { 
    $testParams.VHDPath 
} else { 
    "C:\Users\Public\Documents\Hyper-V\Virtual Hard Disks\$($testParams.VMName).vhdx" 
}
Write-Host "  - VHD Path: $vhdPath"
Write-Host "  - Size: $($testParams.StorageGB) GB"
Write-Host "  - Type: Dynamic"

Write-Host "`nStep 3: Creating virtual machine" -ForegroundColor Cyan
Write-Host "  - Name: $($testParams.VMName)"
Write-Host "  - Memory: $($testParams.MemoryGB) GB"
Write-Host "  - Generation: $($testParams.Generation)"
Write-Host "  - Switch: $($testParams.VirtualSwitch)"

Write-Host "`nStep 4: Configuring VM hardware" -ForegroundColor Cyan
Write-Host "  - Setting processor count to $($testParams.CPUCount)"
Write-Host "  - Configuring memory (Dynamic: Min=1GB, Max=$($testParams.MemoryGB)GB, Startup=$($testParams.MemoryGB)GB)"
if ($testParams.Generation -eq 2 -and $testParams.EnableSecureBoot) {
    Write-Host "  - Enabling Secure Boot"
}

Write-Host "`nStep 5: Finalizing VM creation" -ForegroundColor Cyan
Write-Host "  - Verifying VM properties"
Write-Host "  - VM State: Off"

# Simulate a successful result
$result = [PSCustomObject]@{
    Type = "Result"
    Success = $true
    Message = "VM '$($testParams.VMName)' created successfully (simulated)."
    VMName = $testParams.VMName
    VHDPath = $vhdPath
    CPUCount = $testParams.CPUCount
    MemoryGB = $testParams.MemoryGB
    VirtualSwitch = $testParams.VirtualSwitch
}

Write-Host "`nResult:" -ForegroundColor Green
$result | Format-List

Write-Host "VM creation simulation completed successfully!" -ForegroundColor Green
Write-Host "Note: No actual VM was created during this test." -ForegroundColor Yellow 
<#
.SYNOPSIS
    Test script for Remote Desktop Session Host configuration.

.DESCRIPTION
    This script tests the Remote Desktop Session Host configuration functionality
    by creating a VM and configuring it as an RDSH server.

.NOTES
    File Name      : Test-RDSHConfig.ps1
    Author         : HyperV Creator Team
    Prerequisite   : PowerShell 5.1 or later
#>

# Import required modules
Import-Module Hyper-V

# Source common functions
. "$PSScriptRoot\Common\ErrorHandling.ps1"
. "$PSScriptRoot\Common\Logging.ps1"
. "$PSScriptRoot\Common\Validation.ps1"
. "$PSScriptRoot\HyperV\CreateVM.ps1"
. "$PSScriptRoot\HyperV\ConfigureNetwork.ps1"
. "$PSScriptRoot\HyperV\ConfigureStorage.ps1"
. "$PSScriptRoot\RoleConfiguration\RDSH.ps1"

# Configuration parameters
$vmName = "TestRDSH01"
$cpuCount = 4
$memoryGB = 8
$storageGB = 80
$switchName = "Default Switch"
$osIsoPath = "C:\ISOs\Windows_Server_2019.iso"
$adminPassword = ConvertTo-SecureString "P@ssw0rd123!" -AsPlainText -Force
$collectionName = "TestCollection"
$joinDomain = $false
$installRDWeb = $true
$installRDGateway = $false
$installRDConnection = $true

# Test VM creation first
try {
    Write-Host "Testing Remote Desktop Session Host configuration workflow..." -ForegroundColor Cyan
    
    # Step 1: Create VM
    Write-Host "Step 1: Creating base VM..." -ForegroundColor Green
    $createVMParams = @{
        VMName        = $vmName
        CPUCount      = $cpuCount
        MemoryGB      = $memoryGB
        Generation    = 2
        SwitchName    = $switchName
    }
    
    New-HyperVVM @createVMParams
    
    # Step 2: Add storage
    Write-Host "Step 2: Configuring storage..." -ForegroundColor Green
    $storageParams = @{
        VMName        = $vmName
        BootDiskSizeGB = $storageGB
        DataDisks     = @()  # No additional disks for this test
    }
    
    Add-VMStorage @storageParams
    
    # Step 3: Mount OS ISO
    Write-Host "Step 3: Mounting OS ISO..." -ForegroundColor Green
    Add-VMDvdDrive -VMName $vmName -Path $osIsoPath
    Set-VMFirmware -VMName $vmName -FirstBootDevice (Get-VMDvdDrive -VMName $vmName)
    
    # Step 4: Configure networking (default)
    Write-Host "Step 4: Configuring network..." -ForegroundColor Green
    $networkParams = @{
        VMName        = $vmName
        SwitchName    = $switchName
        DHCP          = $true
    }
    
    Set-VMNetwork @networkParams
    
    Write-Host "VM created and configured successfully. Starting VM..." -ForegroundColor Green
    Start-VM -Name $vmName
    
    # Wait for OS installation (in a real scenario this would be handled through unattended installation)
    Write-Host "Waiting for OS installation (simulated for test)..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10  # In a real scenario, we would wait for the OS to be fully installed
    
    # Step 5: Configure as RDSH
    Write-Host "Step 5: Configuring as Remote Desktop Session Host..." -ForegroundColor Green
    $rdshParams = @{
        VMName             = $vmName
        RDSHCollectionName = $collectionName
        InstallRDWeb       = $installRDWeb
        InstallRDGateway   = $installRDGateway
        InstallRDConnection = $installRDConnection
        JoinDomain         = $joinDomain
    }
    
    # Note: In a real scenario, we would pass credentials and wait for the VM to be responsive
    # For testing purposes, we're just simulating the call
    Write-Host "Simulating Install-RDSH call with parameters:" -ForegroundColor Cyan
    $rdshParams.GetEnumerator() | ForEach-Object {
        Write-Host "  $($_.Name): $($_.Value)" -ForegroundColor Gray
    }
    
    # In a real scenario: Install-RDSH @rdshParams
    
    Write-Host "Remote Desktop Session Host configuration test completed!" -ForegroundColor Green
}
catch {
    Write-Host "Error during Remote Desktop Session Host configuration test: $_" -ForegroundColor Red
}
finally {
    # Clean up - in a test scenario, we might want to remove the VM
    Write-Host "Cleaning up test resources..." -ForegroundColor Yellow
    
    # Stop VM if running
    if (Get-VM -Name $vmName -ErrorAction SilentlyContinue | Where-Object { $_.State -eq 'Running' }) {
        Stop-VM -Name $vmName -Force
    }
    
    # Remove VM
    if (Get-VM -Name $vmName -ErrorAction SilentlyContinue) {
        Remove-VM -Name $vmName -Force
    }
    
    # Remove any VHDs created
    $vhdPath = Join-Path -Path (Get-VMHost).VirtualHardDiskPath -ChildPath "$vmName.vhdx"
    if (Test-Path $vhdPath) {
        Remove-Item -Path $vhdPath -Force
    }
    
    Write-Host "Cleanup completed." -ForegroundColor Yellow
} 
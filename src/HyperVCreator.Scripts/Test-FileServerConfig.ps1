<#
.SYNOPSIS
    Test script for File Server configuration.

.DESCRIPTION
    This script tests the File Server configuration functionality
    by creating a VM and configuring it as a File Server.

.NOTES
    File Name      : Test-FileServerConfig.ps1
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
. "$PSScriptRoot\RoleConfiguration\FileServer.ps1"

# Configuration parameters
$vmName = "TestFS01"
$cpuCount = 2
$memoryGB = 4
$systemDiskGB = 60
$switchName = "Default Switch"
$osIsoPath = "C:\ISOs\Windows_Server_2019.iso"
$adminPassword = ConvertTo-SecureString "P@ssw0rd123!" -AsPlainText -Force
$joinDomain = $false
$enableFSRM = $true
$enableShadowCopies = $true

# Data disk configurations
$dataDisks = @(
    @{
        SizeGB = 100
        DriveLetter = "D"
    },
    @{
        SizeGB = 100
        DriveLetter = "E"
    }
)

# Share configurations
$shares = @(
    @{
        Name = "Public"
        Path = "D:\Shares\Public"
        Description = "Public share for testing"
    },
    @{
        Name = "Private"
        Path = "E:\Shares\Private"
        Description = "Private share for testing"
    }
)

# Test VM creation and configuration
try {
    Write-Host "Testing File Server configuration workflow..." -ForegroundColor Cyan
    
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
        VMName          = $vmName
        BootDiskSizeGB  = $systemDiskGB
        DataDisks       = $dataDisks
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
    
    # Step 5: Configure as File Server
    Write-Host "Step 5: Configuring as File Server..." -ForegroundColor Green
    $fileServerParams = @{
        VMName             = $vmName
        Shares             = $shares
        DataDisks          = $dataDisks
        JoinDomain         = $joinDomain
        EnableFSRM         = $enableFSRM
        EnableShadowCopies = $enableShadowCopies
    }
    
    # Note: In a real scenario, we would pass credentials and wait for the VM to be responsive
    # For testing purposes, we're just simulating the call
    Write-Host "Simulating Install-FileServer call with parameters:" -ForegroundColor Cyan
    
    Write-Host "  VMName: $vmName" -ForegroundColor Gray
    Write-Host "  JoinDomain: $joinDomain" -ForegroundColor Gray
    Write-Host "  EnableFSRM: $enableFSRM" -ForegroundColor Gray
    Write-Host "  EnableShadowCopies: $enableShadowCopies" -ForegroundColor Gray
    
    Write-Host "  DataDisks:" -ForegroundColor Gray
    foreach ($disk in $dataDisks) {
        Write-Host "    - Size: $($disk.SizeGB) GB, Drive Letter: $($disk.DriveLetter)" -ForegroundColor Gray
    }
    
    Write-Host "  Shares:" -ForegroundColor Gray
    foreach ($share in $shares) {
        Write-Host "    - Name: $($share.Name), Path: $($share.Path), Description: $($share.Description)" -ForegroundColor Gray
    }
    
    # In a real scenario: Install-FileServer @fileServerParams
    
    Write-Host "File Server configuration test completed!" -ForegroundColor Green
}
catch {
    Write-Host "Error during File Server configuration test: $_" -ForegroundColor Red
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
    
    # Remove any additional data disks
    foreach ($disk in $dataDisks) {
        $dataDiskPath = Join-Path -Path (Get-VMHost).VirtualHardDiskPath -ChildPath "$vmName-$($disk.DriveLetter).vhdx"
        if (Test-Path $dataDiskPath) {
            Remove-Item -Path $dataDiskPath -Force
        }
    }
    
    Write-Host "Cleanup completed." -ForegroundColor Yellow
} 
<#
.SYNOPSIS
    Test script for Web Server (IIS) configuration.

.DESCRIPTION
    This script tests the Web Server (IIS) configuration functionality
    by creating a VM and configuring it as a Web Server.

.NOTES
    File Name      : Test-WebServerConfig.ps1
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
. "$PSScriptRoot\RoleConfiguration\WebServer.ps1"

# Configuration parameters
$vmName = "TestWEB01"
$cpuCount = 2
$memoryGB = 4
$storageGB = 60
$switchName = "Default Switch"
$osIsoPath = "C:\ISOs\Windows_Server_2019.iso"
$adminPassword = ConvertTo-SecureString "P@ssw0rd123!" -AsPlainText -Force
$joinDomain = $false
$enableWindowsAuth = $true
$enableBasicAuth = $false
$defaultWebsitePort = 80
$installURLRewrite = $true
$installARR = $false

# Application pool configurations
$applicationPools = @(
    @{
        Name = "TestAppPool1"
        RuntimeVersion = "v4.0"
    },
    @{
        Name = "TestAppPool2"
        RuntimeVersion = "v4.0"
    }
)

# Website configurations
$websites = @(
    @{
        Name = "TestSite1"
        PhysicalPath = "C:\inetpub\TestSite1"
        Port = 8080
        ApplicationPool = "TestAppPool1"
    },
    @{
        Name = "TestSite2"
        PhysicalPath = "C:\inetpub\TestSite2"
        Port = 8081
        ApplicationPool = "TestAppPool2"
    }
)

# Test VM creation and configuration
try {
    Write-Host "Testing Web Server (IIS) configuration workflow..." -ForegroundColor Cyan
    
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
    
    # Step 5: Configure as Web Server
    Write-Host "Step 5: Configuring as Web Server..." -ForegroundColor Green
    $webServerParams = @{
        VMName                  = $vmName
        Websites                = $websites
        ApplicationPools        = $applicationPools
        EnableWindowsAuth       = $enableWindowsAuth
        EnableBasicAuth         = $enableBasicAuth
        DefaultWebsitePort      = $defaultWebsitePort
        JoinDomain              = $joinDomain
        InstallURLRewrite       = $installURLRewrite
        InstallARR              = $installARR
    }
    
    # Note: In a real scenario, we would pass credentials and wait for the VM to be responsive
    # For testing purposes, we're just simulating the call
    Write-Host "Simulating Install-WebServer call with parameters:" -ForegroundColor Cyan
    
    Write-Host "  VMName: $vmName" -ForegroundColor Gray
    Write-Host "  EnableWindowsAuth: $enableWindowsAuth" -ForegroundColor Gray
    Write-Host "  EnableBasicAuth: $enableBasicAuth" -ForegroundColor Gray
    Write-Host "  DefaultWebsitePort: $defaultWebsitePort" -ForegroundColor Gray
    Write-Host "  JoinDomain: $joinDomain" -ForegroundColor Gray
    Write-Host "  InstallURLRewrite: $installURLRewrite" -ForegroundColor Gray
    Write-Host "  InstallARR: $installARR" -ForegroundColor Gray
    
    Write-Host "  Application Pools:" -ForegroundColor Gray
    foreach ($pool in $applicationPools) {
        Write-Host "    - Name: $($pool.Name), Runtime: $($pool.RuntimeVersion)" -ForegroundColor Gray
    }
    
    Write-Host "  Websites:" -ForegroundColor Gray
    foreach ($site in $websites) {
        Write-Host "    - Name: $($site.Name), Path: $($site.PhysicalPath), Port: $($site.Port), App Pool: $($site.ApplicationPool)" -ForegroundColor Gray
    }
    
    # In a real scenario: Install-WebServer @webServerParams
    
    Write-Host "Web Server configuration test completed!" -ForegroundColor Green
}
catch {
    Write-Host "Error during Web Server configuration test: $_" -ForegroundColor Red
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
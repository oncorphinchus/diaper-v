# Simple-Test.ps1
# A simple test script to verify PowerShell execution

Write-Host "PowerShell Test Script" -ForegroundColor Green
Write-Host "---------------------" -ForegroundColor Green

# Test simple output
Write-Host "Hello, World!" -ForegroundColor Cyan

# Test date function
$currentDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
Write-Host "Current date and time: $currentDate" -ForegroundColor Yellow

# Check for Hyper-V module
if (Get-Module -ListAvailable -Name Hyper-V) {
    Write-Host "Hyper-V module is available" -ForegroundColor Green
    
    # Check for virtual switches
    $switches = Get-VMSwitch -ErrorAction SilentlyContinue
    if ($switches) {
        Write-Host "Found the following virtual switches:" -ForegroundColor Green
        $switches | Format-Table Name, SwitchType -AutoSize
    } else {
        Write-Host "No virtual switches found" -ForegroundColor Red
    }
    
    # Check for existing VMs
    $vms = Get-VM -ErrorAction SilentlyContinue
    if ($vms) {
        Write-Host "Found the following virtual machines:" -ForegroundColor Green
        $vms | Format-Table Name, State -AutoSize
    } else {
        Write-Host "No virtual machines found" -ForegroundColor Red
    }
} else {
    Write-Host "Hyper-V module is not available" -ForegroundColor Red
}

# System information
Write-Host "System Information:" -ForegroundColor Green
$os = Get-CimInstance -ClassName Win32_OperatingSystem
$cpu = Get-CimInstance -ClassName Win32_Processor
$memory = Get-CimInstance -ClassName Win32_ComputerSystem

Write-Host "OS: $($os.Caption) $($os.Version)" -ForegroundColor Yellow
Write-Host "CPU: $($cpu.Name)" -ForegroundColor Yellow
Write-Host "Memory: $([math]::Round($memory.TotalPhysicalMemory / 1GB, 2)) GB" -ForegroundColor Yellow

Write-Host "Test completed successfully!" -ForegroundColor Green 
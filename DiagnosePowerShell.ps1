# PowerShell environment diagnostic script for HyperV Creator
# This script checks various aspects of the PowerShell environment to ensure it is properly configured

$logFile = Join-Path $env:TEMP "HyperVCreatorDiagnostics_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"

# Function to write to both console and log file
function Write-LogMessage {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    
    Write-Host $Message -ForegroundColor $Color
    Add-Content -Path $logFile -Value $Message
}

Write-LogMessage "=== PowerShell Environment Diagnostic Tool ===" -Color Cyan
Write-LogMessage "Running diagnostics at $(Get-Date)" -Color Cyan
Write-LogMessage "Saving log to: $logFile" -Color Cyan
Write-LogMessage ""

# Check PowerShell version
Write-LogMessage "PowerShell Version Information:" -Color Green
$versionInfo = $PSVersionTable | Format-Table -AutoSize | Out-String
Write-LogMessage $versionInfo

# Check execution policy
Write-LogMessage "Execution Policy:" -Color Green
try {
    $policy = Get-ExecutionPolicy
    Write-LogMessage "Current execution policy: $policy"
    
    if ($policy -eq "Restricted") {
        Write-LogMessage "WARNING: Restricted execution policy may prevent scripts from running." -Color Yellow
        Write-LogMessage "Consider changing the policy with: Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser" -Color Yellow
    }
} catch {
    Write-LogMessage "Error retrieving execution policy: $_" -Color Red
}

# Check if running as administrator
Write-LogMessage "Administrator Status:" -Color Green
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if ($isAdmin) {
    Write-LogMessage "Running with administrator privileges: Yes"
} else {
    Write-LogMessage "Running with administrator privileges: No" -Color Yellow
    Write-LogMessage "Some operations may require administrator privileges." -Color Yellow
}

# Check Hyper-V module
Write-LogMessage "Hyper-V Module Status:" -Color Green
if (Get-Module -ListAvailable -Name Hyper-V) {
    Write-LogMessage "Hyper-V module is available"
    
    try {
        $hyperVCommands = Get-Command -Module Hyper-V -ErrorAction Stop
        Write-LogMessage "Number of Hyper-V commands available: $($hyperVCommands.Count)"
        
        # Check if key commands are available
        $keyCommands = @("New-VM", "Get-VM", "Set-VM", "New-VHD", "Get-VMSwitch")
        foreach ($cmd in $keyCommands) {
            if ($hyperVCommands.Name -contains $cmd) {
                Write-LogMessage "  - $cmd : Available" -Color Green
            } else {
                Write-LogMessage "  - $cmd : Not Available" -Color Red
            }
        }
        
    } catch {
        Write-LogMessage "Error retrieving Hyper-V commands: $_" -Color Red
    }
} else {
    Write-LogMessage "Hyper-V module is not available" -Color Red
    Write-LogMessage "Please ensure Hyper-V is enabled on this system." -Color Red
    
    # Check if Hyper-V is enabled
    try {
        $hyperv = Get-WindowsOptionalFeature -FeatureName Microsoft-Hyper-V-All -Online -ErrorAction Stop
        if ($hyperv.State -eq "Enabled") {
            Write-LogMessage "Hyper-V Windows feature is enabled, but module not available." -Color Yellow
        } else {
            Write-LogMessage "Hyper-V Windows feature is not enabled." -Color Red
            Write-LogMessage "To enable Hyper-V, run the following in an elevated PowerShell:" -Color Yellow
            Write-LogMessage "  Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V -All" -Color Yellow
        }
    } catch {
        Write-LogMessage "Could not check Hyper-V Windows feature status: $_" -Color Red
    }
}

# Virtual switches
Write-LogMessage "Virtual Switches:" -Color Green
try {
    $switches = Get-VMSwitch -ErrorAction Stop
    if ($switches) {
        Write-LogMessage "Available virtual switches:"
        $switchInfo = $switches | Format-Table Name, SwitchType, NetAdapterInterfaceDescription -AutoSize | Out-String
        Write-LogMessage $switchInfo
    } else {
        Write-LogMessage "No virtual switches found." -Color Yellow
        Write-LogMessage "You may need to create a virtual switch for VM networking." -Color Yellow
    }
} catch {
    Write-LogMessage "Error retrieving virtual switches: $_" -Color Red
}

# System environment
Write-LogMessage "System Environment:" -Color Green
Write-LogMessage "Operating System: $(Get-CimInstance Win32_OperatingSystem | Select-Object -ExpandProperty Caption)"
Write-LogMessage "OS Version: $([System.Environment]::OSVersion.Version)"
Write-LogMessage ".NET Version: $([System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription)"
Write-LogMessage "PowerShell Path: $PSHOME"

# Check PSModulePath
Write-LogMessage "PSModulePath Directories:" -Color Green
$env:PSModulePath -split ';' | ForEach-Object {
    $path = $_
    if (Test-Path $path) {
        Write-LogMessage "  $path (Exists)" -Color Green
    } else {
        Write-LogMessage "  $path (Does Not Exist)" -Color Yellow
    }
}

# Recommendation summary
Write-LogMessage "Recommendations:" -Color Cyan

if (!$isAdmin) {
    Write-LogMessage "- Run PowerShell as Administrator for full functionality" -Color Yellow
}

if ($policy -eq "Restricted") {
    Write-LogMessage "- Change execution policy to allow scripts to run" -Color Yellow
    Write-LogMessage "  Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser" -Color Yellow
}

if (!(Get-Module -ListAvailable -Name Hyper-V)) {
    Write-LogMessage "- Enable Hyper-V role on this system" -Color Yellow
    Write-LogMessage "  Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V -All" -Color Yellow
}

if (!$switches) {
    Write-LogMessage "- Create at least one virtual switch" -Color Yellow
    Write-LogMessage "  New-VMSwitch -Name 'Default Switch' -SwitchType Internal" -Color Yellow
}

Write-LogMessage "Diagnostic complete." -Color Cyan
Write-LogMessage "Log saved to: $logFile" -Color Cyan
Write-LogMessage "Press any key to exit..."
$host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") | Out-Null 
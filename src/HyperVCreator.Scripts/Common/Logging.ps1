<#
.SYNOPSIS
    Logging functions for the HyperV VM Creation Application.

.DESCRIPTION
    This script contains common logging functions used across the PowerShell scripts
    in the HyperV VM Creation Application.

.NOTES
    File Name      : Logging.ps1
    Author         : HyperV Creator Team
    Prerequisite   : PowerShell 5.1 or later
#>

#region Variables

# Default log path
$script:DefaultLogPath = Join-Path ([Environment]::GetFolderPath('CommonApplicationData')) "HyperVCreator\Logs"
$script:DefaultLogFile = $null # Will be set based on script name
$script:LoggingEnabled = $true
$script:LogLevel = "Info" # Available levels: Debug, Info, Warning, Error
$script:LogToConsole = $true

# Log level enum
$script:LogLevels = @{
    "Debug" = 0
    "Info" = 1
    "Warning" = 2
    "Error" = 3
}

#endregion

#region Functions

<#
.SYNOPSIS
    Initializes the logging system.

.DESCRIPTION
    Sets up the logging system with the specified log file and level.

.PARAMETER LogPath
    Path to the log directory. Defaults to %TEMP%\HyperVCreator\Logs.

.PARAMETER LogFileName
    Name of the log file. If not specified, a name based on the current date is used.

.PARAMETER Level
    Log level (Debug, Info, Warning, Error). Defaults to Info.

.PARAMETER LogToConsole
    Whether to also log messages to the console. Default is $true.

.EXAMPLE
    Initialize-Logging -LogPath "C:\Logs" -LogFileName "MyScript.log" -Level "Debug"
#>
function Initialize-Logging {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$false)]
        [string]$LogPath = $script:DefaultLogPath,
        
        [Parameter(Mandatory=$false)]
        [string]$LogFileName = $null,
        
        [Parameter(Mandatory=$false)]
        [ValidateSet("Debug", "Info", "Warning", "Error")]
        [string]$Level = "Info",
        
        [Parameter(Mandatory=$false)]
        [bool]$LogToConsole = $true
    )
    
    # Set module variables
    $script:LoggingEnabled = $true
    $script:LogLevel = $Level
    $script:LogToConsole = $LogToConsole
    
    # Create log directory if it doesn't exist
    if (-not (Test-Path -Path $LogPath)) {
        try {
            New-Item -Path $LogPath -ItemType Directory -Force | Out-Null
        }
        catch {
            Write-Error "Failed to create log directory at $LogPath. Logging will be disabled. Error: $_"
            $script:LoggingEnabled = $false
            return
        }
    }
    
    # If log filename not specified, derive from calling script
    if ([string]::IsNullOrEmpty($LogFileName)) {
        $callStack = Get-PSCallStack
        if ($callStack.Count -gt 1) {
            # Get the name of the script that called this function
            $callingScript = $callStack[1].ScriptName
            if (-not [string]::IsNullOrEmpty($callingScript)) {
                $scriptName = [System.IO.Path]::GetFileNameWithoutExtension($callingScript)
                $LogFileName = "$scriptName-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
            }
            else {
                $LogFileName = "HyperVCreator-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
            }
        }
        else {
            $LogFileName = "HyperVCreator-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
        }
    }
    
    # Set the log file path
    $script:DefaultLogFile = Join-Path $LogPath $LogFileName
    
    # Initial log entry
    Write-Log -Message "Logging initialized" -Level "Debug"
    Write-Log -Message "Log file: $($script:DefaultLogFile)" -Level "Debug"
}

<#
.SYNOPSIS
    Writes a log entry.

.DESCRIPTION
    Writes a log entry to the configured log file with the specified level.

.PARAMETER Message
    The message to log.

.PARAMETER Level
    The level of the log entry (Debug, Info, Warning, Error).

.PARAMETER Component
    The component or module generating the log entry.

.EXAMPLE
    Write-Log -Message "VM creation started" -Level "Info" -Component "CreateVM"
#>
function Write-Log {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$Message,
        
        [Parameter(Mandatory=$false)]
        [ValidateSet("Debug", "Info", "Warning", "Error")]
        [string]$Level = "Info",
        
        [Parameter(Mandatory=$false)]
        [string]$Component = "General"
    )
    
    # Initialize logging if not already done
    if ($null -eq $script:DefaultLogFile) {
        Initialize-Logging
    }
    
    # Check if we should log based on the current log level
    if ($script:LogLevels[$Level] -ge $script:LogLevels[$script:LogLevel]) {
        $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        $logEntry = "[$timestamp] [$Level] [$Component] $Message"
        
        # Write to log file
        Add-Content -Path $script:DefaultLogFile -Value $logEntry
        
        # Output to console based on level
        switch ($Level) {
            "Debug" { Write-Verbose $logEntry }
            "Info" { Write-Verbose $logEntry }
            "Warning" { Write-Warning $Message }
            "Error" { Write-Error $Message }
        }
    }
}

<#
.SYNOPSIS
    Creates a status update object for progress reporting.

.DESCRIPTION
    Creates a standardized status update object that can be returned to the C# application
    to report progress of an operation.

.PARAMETER PercentComplete
    The percentage of the operation that is complete (0-100).

.PARAMETER StatusMessage
    A message describing the current status.

.PARAMETER Component
    The component or operation reporting the status.

.EXAMPLE
    $status = New-StatusUpdate -PercentComplete 50 -StatusMessage "Creating virtual disk" -Component "CreateVM"
    Write-Output $status
#>
function New-StatusUpdate {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [ValidateRange(0, 100)]
        [int]$PercentComplete,
        
        [Parameter(Mandatory=$true)]
        [string]$StatusMessage,
        
        [Parameter(Mandatory=$false)]
        [string]$Component = "General"
    )
    
    # Log the status update
    Write-Log -Message "Progress: $PercentComplete% - $StatusMessage" -Level "Info" -Component $Component
    
    # Create a standardized status update object
    $statusUpdate = [PSCustomObject]@{
        Type = "StatusUpdate"
        PercentComplete = $PercentComplete
        StatusMessage = $StatusMessage
        Component = $Component
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    }
    
    return $statusUpdate
}

<#
.SYNOPSIS
    Creates a success result object.

.DESCRIPTION
    Creates a standardized success result object that can be returned to the C# application
    to report successful completion of an operation.

.PARAMETER Message
    A message describing the successful operation.

.PARAMETER Data
    Optional additional data to include in the result.

.PARAMETER Component
    The component or operation reporting the result.

.EXAMPLE
    $result = New-SuccessResult -Message "VM created successfully" -Component "CreateVM" -Data @{ VMName = "TestVM" }
    Write-Output $result
#>
function New-SuccessResult {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$Message,
        
        [Parameter(Mandatory=$false)]
        [object]$Data = $null,
        
        [Parameter(Mandatory=$false)]
        [string]$Component = "General"
    )
    
    # Log the success
    Write-Log -Message "Success: $Message" -Level "Info" -Component $Component
    
    # Create a standardized success result object
    $result = [PSCustomObject]@{
        Type = "Result"
        Success = $true
        Message = $Message
        Data = $Data
        Component = $Component
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    }
    
    return $result
}

#endregion

# Export functions
Export-ModuleMember -Function Initialize-Logging, Write-Log, New-StatusUpdate, New-SuccessResult 
<#
.SYNOPSIS
    Error handling functions for the HyperV VM Creation Application.

.DESCRIPTION
    This script contains common error handling functions used across the PowerShell scripts
    in the HyperV VM Creation Application.

.NOTES
    File Name      : ErrorHandling.ps1
    Author         : HyperV Creator Team
    Prerequisite   : PowerShell 5.1 or later
#>

#region Functions

<#
.SYNOPSIS
    Creates a standardized error object to return to the application.

.DESCRIPTION
    Creates a consistent error object format that can be parsed by the C# application.

.PARAMETER ErrorMessage
    The primary error message.

.PARAMETER ErrorDetails
    Detailed error information, typically the exception object.

.PARAMETER ErrorSource
    The source of the error (function, script, or component name).

.EXAMPLE
    $result = New-ErrorResult -ErrorMessage "Failed to create VM" -ErrorDetails $_ -ErrorSource "New-HyperVVM"
#>
function New-ErrorResult {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$ErrorMessage,
        
        [Parameter(Mandatory=$false)]
        [object]$ErrorDetails = $null,
        
        [Parameter(Mandatory=$false)]
        [string]$ErrorSource = "Unknown"
    )
    
    # Create a standardized error object
    $errorResult = [PSCustomObject]@{
        Type = "Result"
        Success = $false
        Message = $ErrorMessage
        ErrorSource = $ErrorSource
        ErrorDetails = $ErrorDetails
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    }
    
    # Log the error
    Write-Error "$ErrorSource error: $ErrorMessage"
    
    return $errorResult
}

<#
.SYNOPSIS
    Handles exceptions in a standard way across scripts.

.DESCRIPTION
    Provides a common way to catch and process exceptions.

.PARAMETER Exception
    The exception object from a catch block.

.PARAMETER ErrorSource
    The source of the error (function, script, or component name).

.PARAMETER CustomMessage
    Optional custom message to prefix the error with.

.EXAMPLE
    try {
        # Code that may throw an exception
    }
    catch {
        $result = Handle-Exception -Exception $_ -ErrorSource "New-HyperVVM"
        Write-Output $result
    }
#>
function Handle-Exception {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [System.Management.Automation.ErrorRecord]$Exception,
        
        [Parameter(Mandatory=$false)]
        [string]$ErrorSource = "Unknown",
        
        [Parameter(Mandatory=$false)]
        [string]$CustomMessage = ""
    )
    
    # Extract exception information
    $exceptionMessage = $Exception.Exception.Message
    $exceptionType = $Exception.Exception.GetType().Name
    $stackTrace = $Exception.ScriptStackTrace
    
    # Build the error message
    $errorMessage = if ([string]::IsNullOrEmpty($CustomMessage)) {
        "Error in $ErrorSource: $exceptionMessage"
    } else {
        "$CustomMessage`: $exceptionMessage"
    }
    
    # Create extended error details
    $errorDetails = [PSCustomObject]@{
        Message = $exceptionMessage
        Type = $exceptionType
        StackTrace = $stackTrace
        InnerException = if ($Exception.Exception.InnerException) { $Exception.Exception.InnerException.Message } else { $null }
    }
    
    # Return a standard error result
    return New-ErrorResult -ErrorMessage $errorMessage -ErrorDetails $errorDetails -ErrorSource $ErrorSource
}

<#
.SYNOPSIS
    Validates that all required parameters are provided.

.DESCRIPTION
    Checks that all required parameters have values and returns an error result if any are missing.

.PARAMETER Parameters
    A hashtable of parameter names and values.

.PARAMETER RequiredParameters
    An array of parameter names that are required.

.PARAMETER ErrorSource
    The source to include in any error messages.

.EXAMPLE
    $params = @{
        VMName = "TestVM"
        CPUCount = 2
        MemoryGB = $null
    }
    $result = Validate-Parameters -Parameters $params -RequiredParameters @("VMName", "CPUCount", "MemoryGB") -ErrorSource "New-HyperVVM"
    if (-not $result.Success) {
        Write-Output $result
        return
    }
#>
function Validate-Parameters {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [hashtable]$Parameters,
        
        [Parameter(Mandatory=$true)]
        [string[]]$RequiredParameters,
        
        [Parameter(Mandatory=$false)]
        [string]$ErrorSource = "Parameter Validation"
    )
    
    # Default result
    $result = [PSCustomObject]@{
        Success = $true
        MissingParameters = @()
        ErrorResult = $null
    }
    
    # Check each required parameter
    foreach ($param in $RequiredParameters) {
        if (-not $Parameters.ContainsKey($param) -or $null -eq $Parameters[$param] -or ($Parameters[$param] -is [string] -and [string]::IsNullOrWhiteSpace($Parameters[$param]))) {
            $result.Success = $false
            $result.MissingParameters += $param
        }
    }
    
    # If validation failed, create an error result
    if (-not $result.Success) {
        $missingParamsStr = $result.MissingParameters -join ", "
        $errorMessage = "Required parameter(s) missing or null: $missingParamsStr"
        $result.ErrorResult = New-ErrorResult -ErrorMessage $errorMessage -ErrorSource $ErrorSource
    }
    
    return $result
}

#endregion

# Export functions
Export-ModuleMember -Function New-ErrorResult, Handle-Exception, Validate-Parameters 

# ErrorHandling.ps1
# Purpose: Provides standardized error handling functions for PowerShell scripts

<#
.SYNOPSIS
Writes a standardized error object and optionally exits the script.

.DESCRIPTION
Creates a standardized error object with details about the error and outputs it.
Can optionally terminate script execution.

.PARAMETER ErrorMessage
The primary error message to report.

.PARAMETER ErrorRecord
The PowerShell error record if available.

.PARAMETER ExitScript
Whether the script should exit after logging the error. Default is $false.

.PARAMETER ExitCode
The exit code to use if exiting the script. Default is 1.

.EXAMPLE
Write-StandardError -ErrorMessage "Failed to create VM" -ErrorRecord $_

.EXAMPLE
Write-StandardError -ErrorMessage "Critical error" -ExitScript $true -ExitCode 2
#>
function Write-StandardError {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$ErrorMessage,
        
        [Parameter(Mandatory=$false)]
        [System.Management.Automation.ErrorRecord]$ErrorRecord,
        
        [Parameter(Mandatory=$false)]
        [bool]$ExitScript = $false,
        
        [Parameter(Mandatory=$false)]
        [int]$ExitCode = 1
    )
    
    process {
        # Create the error result object
        $errorDetails = [ordered]@{
            Type = "Error"
            Message = $ErrorMessage
            Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        }
        
        # Add error record details if provided
        if ($null -ne $ErrorRecord) {
            $errorDetails.Add("Exception", $ErrorRecord.Exception.Message)
            $errorDetails.Add("CategoryInfo", $ErrorRecord.CategoryInfo.ToString())
            $errorDetails.Add("FullyQualifiedErrorId", $ErrorRecord.FullyQualifiedErrorId)
            $errorDetails.Add("ScriptStackTrace", $ErrorRecord.ScriptStackTrace)
            $errorDetails.Add("PositionMessage", $ErrorRecord.InvocationInfo.PositionMessage)
        }
        
        # Create and output the error object
        $errorResult = [PSCustomObject]$errorDetails
        Write-Output $errorResult
        
        # Write to error stream as well
        Write-Error $ErrorMessage
        
        # Exit if requested
        if ($ExitScript) {
            exit $ExitCode
        }
    }
}

<#
.SYNOPSIS
Handles a try/catch block's error in a standardized way.

.DESCRIPTION
Provides a centralized way to handle caught exceptions, creating standardized
error output and optionally continuing or terminating script execution.

.PARAMETER ErrorRecord
The error record from the catch block ($_).

.PARAMETER ErrorMessage
A custom error message to display. If not provided, the error record's message is used.

.PARAMETER ExitScript
Whether the script should exit after handling the error. Default is $false.

.PARAMETER ExitCode
The exit code to use if exiting the script. Default is 1.

.EXAMPLE
try {
    # Some code that might fail
} catch {
    Handle-Error -ErrorRecord $_ -ErrorMessage "Custom message" -ExitScript $true
}
#>
function Handle-Error {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [System.Management.Automation.ErrorRecord]$ErrorRecord,
        
        [Parameter(Mandatory=$false)]
        [string]$ErrorMessage = "",
        
        [Parameter(Mandatory=$false)]
        [bool]$ExitScript = $false,
        
        [Parameter(Mandatory=$false)]
        [int]$ExitCode = 1
    )
    
    process {
        # Use custom message if provided, otherwise use the error record's message
        if ([string]::IsNullOrEmpty($ErrorMessage)) {
            $ErrorMessage = $ErrorRecord.Exception.Message
        }
        
        # Use the standard error function
        Write-StandardError -ErrorMessage $ErrorMessage -ErrorRecord $ErrorRecord -ExitScript $ExitScript -ExitCode $ExitCode
    }
}

<#
.SYNOPSIS
Creates a standardized result object for returning from script functions.

.DESCRIPTION
Creates a consistent result object structure that can be used across all scripts
to ensure standardized output formatting and handling.

.PARAMETER Success
Whether the operation was successful.

.PARAMETER Message
A message describing the result.

.PARAMETER Data
Additional data to include in the result.

.EXAMPLE
$result = New-ResultObject -Success $true -Message "VM created successfully" -Data @{VMName = "TestVM"}
return $result
#>
function New-ResultObject {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [bool]$Success,
        
        [Parameter(Mandatory=$true)]
        [string]$Message,
        
        [Parameter(Mandatory=$false)]
        [object]$Data = $null
    )
    
    process {
        # Create the base result object
        $result = [PSCustomObject]@{
            Type = "Result"
            Success = $Success
            Message = $Message
            Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        }
        
        # Add any additional data if provided
        if ($null -ne $Data) {
            foreach ($key in $Data.Keys) {
                Add-Member -InputObject $result -MemberType NoteProperty -Name $key -Value $Data[$key]
            }
        }
        
        return $result
    }
}

<#
.SYNOPSIS
Creates a standardized status update object for progress reporting.

.DESCRIPTION
Creates a consistent status update object that can be used for progress reporting
across all scripts to ensure standardized output formatting and handling.

.PARAMETER PercentComplete
The percentage of completion (0-100).

.PARAMETER StatusMessage
A message describing the current status.

.PARAMETER AdditionalData
Additional data to include in the status update.

.EXAMPLE
$status = New-StatusUpdate -PercentComplete 50 -StatusMessage "Creating virtual hard disk..."
Write-Output $status
#>
function New-StatusUpdate {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [int]$PercentComplete,
        
        [Parameter(Mandatory=$true)]
        [string]$StatusMessage,
        
        [Parameter(Mandatory=$false)]
        [hashtable]$AdditionalData = $null
    )
    
    process {
        # Ensure percentage is within valid range
        if ($PercentComplete -lt 0) { $PercentComplete = 0 }
        if ($PercentComplete -gt 100) { $PercentComplete = 100 }
        
        # Create the base status update object
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = $PercentComplete
            StatusMessage = $StatusMessage
            Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        }
        
        # Add any additional data if provided
        if ($null -ne $AdditionalData) {
            foreach ($key in $AdditionalData.Keys) {
                Add-Member -InputObject $statusUpdate -MemberType NoteProperty -Name $key -Value $AdditionalData[$key]
            }
        }
        
        return $statusUpdate
    }
} 
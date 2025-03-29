# TrackProgress.ps1
# Purpose: Tracks and reports VM creation progress
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$VMName,
    
    [Parameter(Mandatory=$false)]
    [int]$ReportIntervalSeconds = 5,
    
    [Parameter(Mandatory=$false)]
    [int]$Timeout = 3600  # Default timeout of 1 hour
)

# Import common functions
# . "$PSScriptRoot\..\Common\ErrorHandling.ps1"
# . "$PSScriptRoot\..\Common\Logging.ps1"

function Track-VMCreationProgress {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$VMName,
        
        [Parameter(Mandatory=$false)]
        [int]$ReportIntervalSeconds = 5,
        
        [Parameter(Mandatory=$false)]
        [int]$Timeout = 3600
    )
    
    try {
        # Initialize progress tracking
        $startTime = Get-Date
        $lastReportTime = $startTime
        $maxTime = $startTime.AddSeconds($Timeout)
        
        # Initial status
        $statusUpdate = [PSCustomObject]@{
            Type = "StatusUpdate"
            PercentComplete = 0
            StatusMessage = "Starting VM creation tracking for $VMName..."
            TimeElapsed = "00:00:00"
            VMState = "Unknown"
        }
        Write-Output $statusUpdate
        
        # Check if VM exists
        try {
            $vm = Get-VM -Name $VMName -ErrorAction Stop
        }
        catch {
            throw "Cannot track VM '$VMName': VM does not exist."
        }
        
        # Track while VM is being created, up to timeout
        do {
            # Get current state
            $vm = Get-VM -Name $VMName
            $currentState = $vm.State
            
            # Calculate time elapsed
            $currentTime = Get-Date
            $timeElapsed = $currentTime - $startTime
            $formattedTimeElapsed = "{0:hh\:mm\:ss}" -f $timeElapsed
            
            # Check if it's time to report
            $timeSinceLastReport = $currentTime - $lastReportTime
            if ($timeSinceLastReport.TotalSeconds -ge $ReportIntervalSeconds) {
                # Determine progress percentage based on state
                $percentComplete = 0
                $statusMessage = ""
                
                switch ($currentState) {
                    "Off" {
                        $percentComplete = 10
                        $statusMessage = "VM is created but not started"
                    }
                    "Starting" {
                        $percentComplete = 25
                        $statusMessage = "VM is starting..."
                    }
                    "Running" {
                        # For running state, check additional metrics
                        $percentComplete = 50
                        
                        # Check if integration services are running
                        $integrationServicesStatus = $vm.IntegrationServicesState
                        if ($integrationServicesStatus -eq "Up to date") {
                            $percentComplete = 75
                            $statusMessage = "VM is running with integration services"
                        }
                        else {
                            $statusMessage = "VM is running, waiting for integration services..."
                        }
                        
                        # Check for heartbeat
                        $heartbeatStatus = (Get-VMIntegrationService -VMName $VMName | Where-Object { $_.Name -eq "Heartbeat" }).PrimaryStatusDescription
                        if ($heartbeatStatus -eq "OK") {
                            $percentComplete = 90
                            $statusMessage = "VM is fully operational with heartbeat detected"
                        }
                    }
                    "Stopping" {
                        $percentComplete = 15
                        $statusMessage = "VM is stopping... This may indicate an issue."
                    }
                    "Saved" {
                        $percentComplete = 20
                        $statusMessage = "VM is in saved state"
                    }
                    "Paused" {
                        $percentComplete = 30
                        $statusMessage = "VM is paused"
                    }
                    default {
                        $percentComplete = 5
                        $statusMessage = "VM is in state: $currentState"
                    }
                }
                
                # Report progress
                $statusUpdate = [PSCustomObject]@{
                    Type = "StatusUpdate"
                    PercentComplete = $percentComplete
                    StatusMessage = $statusMessage
                    TimeElapsed = $formattedTimeElapsed
                    VMState = $currentState
                }
                Write-Output $statusUpdate
                
                # Update last report time
                $lastReportTime = $currentTime
            }
            
            # Sleep before checking again
            Start-Sleep -Seconds 1
            
        } until (($currentState -eq "Running" -and $percentComplete -ge 90) -or ($currentTime -gt $maxTime))
        
        # Final status
        if ($currentTime -gt $maxTime) {
            # Timeout occurred
            $statusUpdate = [PSCustomObject]@{
                Type = "StatusUpdate"
                PercentComplete = $percentComplete
                StatusMessage = "Tracking timed out. VM is in state: $currentState"
                TimeElapsed = $formattedTimeElapsed
                VMState = $currentState
            }
            Write-Output $statusUpdate
            
            $result = [PSCustomObject]@{
                Type = "Result"
                Success = $false
                Message = "VM tracking timed out after $formattedTimeElapsed"
                VMName = $VMName
                FinalState = $currentState
            }
        }
        else {
            # Successful completion
            $statusUpdate = [PSCustomObject]@{
                Type = "StatusUpdate"
                PercentComplete = 100
                StatusMessage = "VM is fully operational"
                TimeElapsed = $formattedTimeElapsed
                VMState = $currentState
            }
            Write-Output $statusUpdate
            
            $result = [PSCustomObject]@{
                Type = "Result"
                Success = $true
                Message = "VM is fully operational after $formattedTimeElapsed"
                VMName = $VMName
                FinalState = $currentState
            }
        }
        
        Write-Output $result
    }
    catch {
        Write-Error "Error tracking VM progress: $_"
        
        # Return error result
        $result = [PSCustomObject]@{
            Type = "Result"
            Success = $false
            Message = "Error tracking VM progress: $_"
            ErrorDetails = $_
        }
        
        Write-Output $result
    }
}

# Execute tracking
Track-VMCreationProgress -VMName $VMName -ReportIntervalSeconds $ReportIntervalSeconds -Timeout $Timeout 
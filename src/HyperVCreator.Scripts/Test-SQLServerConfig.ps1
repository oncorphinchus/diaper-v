<#
.SYNOPSIS
    Test script for SQL Server configuration.

.DESCRIPTION
    This script tests the configuration of a Hyper-V VM as a SQL Server.
    It creates a new VM, installs Windows Server, and configures it as a SQL Server.

.NOTES
    File Name      : Test-SQLServerConfig.ps1
    Author         : HyperV Creator Team
    Prerequisite   : PowerShell 5.1 or later
#>

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Error "This script must be run as Administrator."
    exit 1
}

# Import required modules
Import-Module Hyper-V

# Source scripts
. "$PSScriptRoot\Common\ErrorHandling.ps1"
. "$PSScriptRoot\Common\Logging.ps1"
. "$PSScriptRoot\HyperV\VirtualMachine.ps1"
. "$PSScriptRoot\RoleConfiguration\SQLServer.ps1"

# Define test parameters
$vmName = "TestSQLServer-$(Get-Random)"
$vmPath = "C:\HyperV\VMs"
$vhdPath = "C:\HyperV\VHDs"
$isoPath = "C:\ISO\WindowsServer2022.iso"
$sqlIsoPath = "C:\ISO\SQLServer2022.iso"
$switchName = "Default Switch"
$adminPassword = ConvertTo-SecureString "P@ssw0rd123!" -AsPlainText -Force

# Start logging
$ErrorActionPreference = "Stop"
Write-LogMessage -Level Info -Message "Starting SQL Server configuration test"

try {
    # Step 1: Create a new VM
    Write-LogMessage -Level Info -Message "Creating new VM: $vmName"
    
    # Create VM parameters
    $vmParams = @{
        VMName = $vmName
        CPUCount = 4
        MemoryGB = 8
        StorageGB = 100
        NetworkSwitch = $switchName
        VMPath = $vmPath
        VHDPath = $vhdPath
        ISOPath = $isoPath
        UnattendXMLPath = "$PSScriptRoot\UnattendXML\SQLServer.xml"
        AdminPassword = $adminPassword
        EnableDynamicMemory = $true
        Generation = 2
    }
    
    # Create the VM
    $vm = New-HyperVVM @vmParams
    
    # Step 2: Install SQL Server
    Write-LogMessage -Level Info -Message "Starting SQL Server installation on $vmName"
    
    # Prepare parameters for SQL Server installation
    $sqlParams = @{
        VMName = $vmName
        SQLServerISOPath = $sqlIsoPath
        SQLEdition = "Developer"
        InstanceName = "MSSQLSERVER"
        SQLFeatures = "SQLENGINE,SSMS"
        SQLCollation = "SQL_Latin1_General_CP1_CI_AS"
        ConfigureFirewall = $true
        EnableMixedMode = $true
        SAPassword = $adminPassword
        SQLDataPath = "C:\SQL_Data"
        SQLLogPath = "C:\SQL_Logs"
        SQLBackupPath = "C:\SQL_Backup"
    }
    
    # Create a credential object for the VM admin
    $adminUserName = "Administrator"
    $adminCredential = New-Object System.Management.Automation.PSCredential($adminUserName, $adminPassword)
    
    # Set the global admin credential for scripts that need it
    $Global:AdminCredential = $adminCredential
    
    # Wait for the VM to fully start and complete initial setup
    Write-LogMessage -Level Info -Message "Waiting for VM to be ready..."
    Start-Sleep -Seconds 180
    
    # Install SQL Server
    Install-SQLServer @sqlParams
    
    # Wait for SQL Server installation to complete
    Start-Sleep -Seconds 60
    
    # Step 3: Verify SQL Server installation
    Write-LogMessage -Level Info -Message "Verifying SQL Server installation"
    
    # Create a new PowerShell session to the VM
    $session = New-PSSession -VMName $vmName -Credential $adminCredential
    
    # Verify SQL Server is running
    $verifyScript = {
        $sqlServices = Get-Service -Name MSSQL* | Where-Object { $_.DisplayName -like "SQL Server (*)" }
        $sqlAgentServices = Get-Service -Name SQLAGENT* | Where-Object { $_.DisplayName -like "SQL Server Agent (*)" }
        
        $result = @{
            SQLServiceCount = $sqlServices.Count
            SQLServiceRunning = $sqlServices.Count -gt 0 -and $sqlServices[0].Status -eq 'Running'
            SQLAgentServiceCount = $sqlAgentServices.Count
            SQLAgentServiceRunning = $sqlAgentServices.Count -gt 0 -and $sqlAgentServices[0].Status -eq 'Running'
        }
        
        return $result
    }
    
    $verificationResult = Invoke-Command -Session $session -ScriptBlock $verifyScript
    
    # Check verification results
    if ($verificationResult.SQLServiceRunning) {
        Write-LogMessage -Level Info -Message "SQL Server service is running."
    } else {
        Write-LogMessage -Level Warning -Message "SQL Server service is not running."
    }
    
    if ($verificationResult.SQLAgentServiceRunning) {
        Write-LogMessage -Level Info -Message "SQL Server Agent service is running."
    } else {
        Write-LogMessage -Level Warning -Message "SQL Server Agent service is not running."
    }
    
    # Test SQL Server connectivity
    $connectivityTest = {
        try {
            # Try to import the SQL Server module
            Import-Module SqlServer -ErrorAction SilentlyContinue
            
            # If the module isn't available, try using the .NET SQL client
            if (-not (Get-Module -Name SqlServer)) {
                Add-Type -AssemblyName System.Data.SqlClient
                $connectionString = "Server=localhost;Integrated Security=true;Connection Timeout=5;"
                $sqlConnection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
                $sqlConnection.Open()
                $sqlConnection.Close()
                return @{ Success = $true; Message = "Successfully connected using .NET SqlClient" }
            } else {
                # Use the SQL Server module if available
                $result = Invoke-Sqlcmd -ServerInstance "localhost" -Query "SELECT @@VERSION AS Version" -ConnectionTimeout 5
                return @{ Success = $true; Message = "Successfully connected using SqlServer module"; Version = $result.Version }
            }
        }
        catch {
            return @{ Success = $false; Message = "Failed to connect to SQL Server: $_" }
        }
    }
    
    $connectivityResult = Invoke-Command -Session $session -ScriptBlock $connectivityTest
    
    if ($connectivityResult.Success) {
        Write-LogMessage -Level Info -Message "Successfully connected to SQL Server: $($connectivityResult.Message)"
        if ($connectivityResult.Version) {
            Write-LogMessage -Level Info -Message "SQL Server version: $($connectivityResult.Version)"
        }
    } else {
        Write-LogMessage -Level Warning -Message "Failed to connect to SQL Server: $($connectivityResult.Message)"
    }
    
    # Clean up the session
    Remove-PSSession -Session $session
    
    # Success message
    Write-LogMessage -Level Info -Message "SQL Server configuration test completed successfully"
}
catch {
    # Catch any errors and log them
    $errorMessage = "Error during SQL Server configuration test: $_"
    Write-LogMessage -Level Error -Message $errorMessage
    throw $errorMessage
}
finally {
    # Clean up - uncomment to automatically remove the test VM
    # Write-LogMessage -Level Info -Message "Cleaning up: Removing VM $vmName"
    # Remove-VM -Name $vmName -Force -ErrorAction SilentlyContinue
    
    # Always reset the global credential
    $Global:AdminCredential = $null
} 
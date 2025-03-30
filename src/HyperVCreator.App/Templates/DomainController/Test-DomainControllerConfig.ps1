# Test-DomainControllerConfig.ps1
# This script tests the Domain Controller configuration script and validates its functionality
# It's used for both development testing and CI/CD pipelines

param (
    [string]$TestName = "All",
    [switch]$Detailed,
    [switch]$NoCleanup
)

# Configuration for testing
$TestVMName = "Test-DC01"
$TestDomainName = "test.local"
$TestNetBIOSName = "TEST"
$TestPassword = "P@ssw0rd1234!"

# Initialize test results
$TestResults = @{
    TotalTests = 0
    PassedTests = 0
    FailedTests = 0
    Warnings = 0
    TestDetails = @()
}

function Write-TestHeader {
    param([string]$TestName)
    
    Write-Host "`n===== Running Test: $TestName =====" -ForegroundColor Cyan
    return Get-Date
}

function Write-TestResult {
    param(
        [string]$TestName,
        [bool]$Success,
        [string]$Message,
        [datetime]$StartTime,
        [string]$Details = ""
    )
    
    $EndTime = Get-Date
    $Duration = ($EndTime - $StartTime).TotalSeconds
    
    if ($Success) {
        Write-Host "PASS: $TestName - $Message" -ForegroundColor Green
        $TestResults.PassedTests++
    }
    else {
        Write-Host "FAIL: $TestName - $Message" -ForegroundColor Red
        $TestResults.FailedTests++
    }
    
    Write-Host "Duration: $($Duration.ToString("0.00")) seconds"
    
    if ($Detailed -and $Details) {
        Write-Host "Details: $Details"
    }
    
    $TestResults.TestDetails += [PSCustomObject]@{
        TestName = $TestName
        Result = if ($Success) { "Pass" } else { "Fail" }
        Message = $Message
        Duration = $Duration
        Details = $Details
    }
    
    $TestResults.TotalTests++
}

function Test-ScriptSyntax {
    $StartTime = Write-TestHeader "Script Syntax Validation"
    
    $ScriptPath = Join-Path $PSScriptRoot "Deploy-DomainController.ps1"
    
    try {
        $Errors = $null
        $null = [System.Management.Automation.PSParser]::Tokenize((Get-Content -Path $ScriptPath -Raw), [ref]$Errors)
        
        if ($Errors.Count -gt 0) {
            Write-TestResult -TestName "Script Syntax Validation" -Success $false -Message "Script contains syntax errors" -StartTime $StartTime -Details ($Errors | Out-String)
            return
        }
        
        Write-TestResult -TestName "Script Syntax Validation" -Success $true -Message "Script syntax is valid" -StartTime $StartTime
    }
    catch {
        Write-TestResult -TestName "Script Syntax Validation" -Success $false -Message "Error checking script syntax" -StartTime $StartTime -Details $_.Exception.Message
    }
}

function Test-ParameterValidation {
    $StartTime = Write-TestHeader "Parameter Validation"
    
    $ScriptPath = Join-Path $PSScriptRoot "Deploy-DomainController.ps1"
    
    try {
        # Load the script as a scriptblock to inspect parameters
        $ScriptContent = Get-Content -Path $ScriptPath -Raw
        $ScriptBlock = [ScriptBlock]::Create($ScriptContent)
        
        # Get parameters
        $Parameters = $ScriptBlock.Ast.ParamBlock.Parameters
        
        # Verify required parameters exist
        $RequiredParams = @("VMName", "DomainName", "NetBIOSName", "DSRMPassword")
        $MissingParams = $RequiredParams | Where-Object { $Parameters.Name.VariablePath.UserPath -notcontains $_ }
        
        if ($MissingParams.Count -gt 0) {
            Write-TestResult -TestName "Parameter Validation" -Success $false -Message "Script is missing required parameters" -StartTime $StartTime -Details ("Missing: " + ($MissingParams -join ", "))
            return
        }
        
        # Check parameter types
        $BoolParams = @("ConfigureDNS", "CreateReverseLookupZone", "UseDHCP", "CreateOUStructure", "ConfigureGPOs", "InstallRSAT")
        $StringParams = @("VMName", "DomainName", "NetBIOSName", "DSRMPassword", "ForestFunctionalLevel", 
                         "DomainFunctionalLevel", "DNSForwarders", "IPAddress", "SubnetMask", "DefaultGateway", "PreferredDNS")
        
        $ParamTypeMismatch = $false
        
        foreach ($Param in $Parameters) {
            $ParamName = $Param.Name.VariablePath.UserPath
            
            if ($BoolParams -contains $ParamName) {
                $ParamType = $Param.StaticType.Name
                if ($ParamType -ne "bool" -and $ParamType -ne "Boolean" -and $ParamType -ne "SwitchParameter") {
                    Write-Host "Parameter $ParamName should be boolean, but is $ParamType" -ForegroundColor Yellow
                    $ParamTypeMismatch = $true
                    $TestResults.Warnings++
                }
            }
            
            if ($StringParams -contains $ParamName) {
                $ParamType = $Param.StaticType.Name
                if ($ParamType -ne "string" -and $ParamType -ne "String") {
                    Write-Host "Parameter $ParamName should be string, but is $ParamType" -ForegroundColor Yellow
                    $ParamTypeMismatch = $true
                    $TestResults.Warnings++
                }
            }
        }
        
        if ($ParamTypeMismatch) {
            Write-Host "Warning: Some parameters have unexpected types. See above for details." -ForegroundColor Yellow
        }
        
        Write-TestResult -TestName "Parameter Validation" -Success $true -Message "All required parameters are present" -StartTime $StartTime
    }
    catch {
        Write-TestResult -TestName "Parameter Validation" -Success $false -Message "Error validating parameters" -StartTime $StartTime -Details $_.Exception.Message
    }
}

function Test-TemplateReplacement {
    $StartTime = Write-TestHeader "Template Replacement Validation"
    
    $ScriptPath = Join-Path $PSScriptRoot "Deploy-DomainController.ps1"
    
    try {
        $ScriptContent = Get-Content -Path $ScriptPath -Raw
        
        # Check for template placeholders
        $TemplatePlaceholders = [regex]::Matches($ScriptContent, '\{\{([^}]+)\}\}')
        
        if ($TemplatePlaceholders.Count -gt 0) {
            $PlaceholderList = $TemplatePlaceholders | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique
            
            Write-TestResult -TestName "Template Replacement Validation" -Success $true -Message "Found $($PlaceholderList.Count) template placeholders" -StartTime $StartTime -Details ("Placeholders: " + ($PlaceholderList -join ", "))
        }
        else {
            Write-TestResult -TestName "Template Replacement Validation" -Success $false -Message "No template placeholders found" -StartTime $StartTime
        }
        
        # Verify all parameters have corresponding placeholders
        $ScriptBlock = [ScriptBlock]::Create($ScriptContent)
        $Parameters = $ScriptBlock.Ast.ParamBlock.Parameters
        $ParamsWithPlaceholders = $Parameters | Where-Object { 
            $ParamName = $_.Name.VariablePath.UserPath
            # Check for {{ParamName}} in the script content
            $ScriptContent -match "\{\{$ParamName\}\}"
        }
        
        $MissingPlaceholders = $Parameters | Where-Object {
            $ParamName = $_.Name.VariablePath.UserPath
            # Boolean parameters use a different replacement format ${{ParamName}}
            if ($_.StaticType.Name -eq "bool" -or $_.StaticType.Name -eq "Boolean" -or $_.StaticType.Name -eq "SwitchParameter") {
                -not ($ScriptContent -match "\`$\{\{$ParamName\}\}")
            }
            else {
                -not ($ScriptContent -match "\{\{$ParamName\}\}")
            }
        }
        
        if ($MissingPlaceholders.Count -gt 0) {
            $MissingList = $MissingPlaceholders | ForEach-Object { $_.Name.VariablePath.UserPath }
            Write-Host "Warning: The following parameters don't have corresponding placeholders: $($MissingList -join ", ")" -ForegroundColor Yellow
            $TestResults.Warnings++
        }
    }
    catch {
        Write-TestResult -TestName "Template Replacement Validation" -Success $false -Message "Error validating template replacements" -StartTime $StartTime -Details $_.Exception.Message
    }
}

function Test-ScriptLogic {
    $StartTime = Write-TestHeader "Script Logic Validation"
    
    try {
        # Check for essential script sections
        $ScriptPath = Join-Path $PSScriptRoot "Deploy-DomainController.ps1"
        $ScriptContent = Get-Content -Path $ScriptPath -Raw
        
        $RequiredParts = @(
            @{ Name = "Network Configuration"; Pattern = "Configure network settings" },
            @{ Name = "Role Installation"; Pattern = "Install.+AD-Domain-Services" },
            @{ Name = "Forest Creation"; Pattern = "Install-ADDSForest" },
            @{ Name = "DNS Configuration"; Pattern = "DNS (Server|Forwarder)" },
            @{ Name = "OU Structure"; Pattern = "OU=.+,$baseOU" },
            @{ Name = "Group Policy"; Pattern = "(GPO|Group Policy)" }
        )
        
        $MissingParts = @()
        
        foreach ($Part in $RequiredParts) {
            if ($ScriptContent -notmatch $Part.Pattern) {
                $MissingParts += $Part.Name
            }
        }
        
        if ($MissingParts.Count -gt 0) {
            Write-TestResult -TestName "Script Logic Validation" -Success $false -Message "Script is missing essential parts" -StartTime $StartTime -Details ("Missing: " + ($MissingParts -join ", "))
        }
        else {
            Write-TestResult -TestName "Script Logic Validation" -Success $true -Message "Script contains all essential logical components" -StartTime $StartTime
        }
        
        # Check for error handling
        if ($ScriptContent -match "try" -and $ScriptContent -match "catch") {
            Write-Host "Script includes error handling (try/catch blocks)" -ForegroundColor Green
        }
        else {
            Write-Host "Warning: Script may lack proper error handling" -ForegroundColor Yellow
            $TestResults.Warnings++
        }
        
        # Check for logging
        if ($ScriptContent -match "Write-Log") {
            Write-Host "Script includes logging functionality" -ForegroundColor Green
        }
        else {
            Write-Host "Warning: Script may lack proper logging" -ForegroundColor Yellow
            $TestResults.Warnings++
        }
    }
    catch {
        Write-TestResult -TestName "Script Logic Validation" -Success $false -Message "Error validating script logic" -StartTime $StartTime -Details $_.Exception.Message
    }
}

function Show-TestSummary {
    Write-Host "`n===== Test Summary =====" -ForegroundColor Cyan
    Write-Host "Total Tests: $($TestResults.TotalTests)" -ForegroundColor White
    Write-Host "Passed: $($TestResults.PassedTests)" -ForegroundColor Green
    Write-Host "Failed: $($TestResults.FailedTests)" -ForegroundColor Red
    
    if ($TestResults.Warnings -gt 0) {
        Write-Host "Warnings: $($TestResults.Warnings)" -ForegroundColor Yellow
    }
    
    if ($Detailed) {
        Write-Host "`nTest Details:" -ForegroundColor Cyan
        $TestResults.TestDetails | Format-Table -Property TestName, Result, Duration, Message -AutoSize
    }
    
    # Return success or failure
    return $TestResults.FailedTests -eq 0
}

# Execute tests based on TestName parameter
switch ($TestName) {
    "Syntax" { Test-ScriptSyntax; break }
    "Parameters" { Test-ParameterValidation; break }
    "Templates" { Test-TemplateReplacement; break }
    "Logic" { Test-ScriptLogic; break }
    "All" {
        Test-ScriptSyntax
        Test-ParameterValidation
        Test-TemplateReplacement
        Test-ScriptLogic
    }
    default {
        Write-Host "Unknown test name: $TestName" -ForegroundColor Red
        Write-Host "Available tests: Syntax, Parameters, Templates, Logic, All" -ForegroundColor Yellow
        exit 1
    }
}

# Show test summary and exit with appropriate code
$TestsPassed = Show-TestSummary
if (-not $TestsPassed) {
    exit 1
}
exit 0 
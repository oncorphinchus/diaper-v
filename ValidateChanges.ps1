# Script to validate that all nullable boolean fixes have been applied
Write-Host "Validating nullable boolean fixes..." -ForegroundColor Green

$rootFolder = $PSScriptRoot
$corePath = Join-Path -Path $rootFolder -ChildPath "src\HyperVCreator.Core"

# Files to check
$filesToCheck = @(
    "PowerShell\PowerShellExtensions.cs",
    "Services\HyperVService.cs",
    "Services\ConfigurationService.cs",
    "Services\TemplateService.cs",
    "Tests\VMCreationServiceTests.cs"
)

$issuesFound = 0

foreach ($file in $filesToCheck) {
    $filePath = Join-Path -Path $corePath -ChildPath $file
    if (Test-Path $filePath) {
        Write-Host "Checking $file..." -ForegroundColor Yellow
        
        $content = Get-Content -Path $filePath -Raw
        
        # Check for WasSuccessful extension method
        if ($file -eq "PowerShell\PowerShellExtensions.cs") {
            if ($content -match "public static bool WasSuccessful\(this PowerShellResult result\)") {
                Write-Host "  ✓ WasSuccessful extension method found" -ForegroundColor Green
            } else {
                Write-Host "  ❌ WasSuccessful extension method NOT found" -ForegroundColor Red
                $issuesFound++
            }
        }
        
        # Check for proper ContainsOutput null check
        if ($file -eq "PowerShell\PowerShellExtensions.cs") {
            if ($content -match "if \(result == null\)\s+return false;") {
                Write-Host "  ✓ ContainsOutput null check found" -ForegroundColor Green
            } else {
                Write-Host "  ❌ ContainsOutput null check NOT found" -ForegroundColor Red
                $issuesFound++
            }
        }
        
        # Check for result.Success && without WasSuccessful
        if ($file -match "Services\\(HyperVService|ConfigurationService)\.cs") {
            if ($content -match "result\.Success\s*&&") {
                Write-Host "  ❌ Found 'result.Success &&' without WasSuccessful - needs to be fixed" -ForegroundColor Red
                $issuesFound++
            } else {
                Write-Host "  ✓ No instances of 'result.Success &&' found - using WasSuccessful() properly" -ForegroundColor Green
            }
        }
        
        # Check for Directory.CreateDirectory without null check in VMCreationServiceTests
        if ($file -eq "Tests\VMCreationServiceTests.cs") {
            if ($content -match "if \(!string\.IsNullOrEmpty\(directoryPath\)\)") {
                Write-Host "  ✓ Directory.CreateDirectory null check found" -ForegroundColor Green
            } else {
                Write-Host "  ❌ Directory.CreateDirectory null check NOT found" -ForegroundColor Red
                $issuesFound++
            }
        }
        
        # Check for nullable VMTemplate in TemplateService
        if ($file -eq "Services\TemplateService.cs") {
            if ($content -match "VMTemplate\?\s+template\s*=\s*JsonSerializer\.Deserialize<VMTemplate>") {
                Write-Host "  ✓ Nullable VMTemplate usage found" -ForegroundColor Green
            } else {
                Write-Host "  ❌ Nullable VMTemplate usage NOT found" -ForegroundColor Red
                $issuesFound++
            }
        }
        
        # Check for unused _powerShellService field in TemplateService
        if ($file -eq "Services\TemplateService.cs") {
            if ($content -match "private\s+readonly\s+PowerShellService\s+_powerShellService") {
                Write-Host "  ❌ Unused _powerShellService field found - needs to be removed" -ForegroundColor Red
                $issuesFound++
            } else {
                Write-Host "  ✓ No unused _powerShellService field found" -ForegroundColor Green
            }
        }
        
    } else {
        Write-Host "File not found: $filePath" -ForegroundColor Red
        $issuesFound++
    }
}

if ($issuesFound -eq 0) {
    Write-Host "`nValidation succeeded! All fixes have been applied correctly." -ForegroundColor Green
} else {
    Write-Host "`nValidation failed! $issuesFound issues found that need to be fixed." -ForegroundColor Red
}

Write-Host "`nPress any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") 
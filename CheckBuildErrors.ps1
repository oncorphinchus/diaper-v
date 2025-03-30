Write-Host "Building HyperVCreator.Core with detailed errors..." -ForegroundColor Cyan

# Set working directory to script location
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
Set-Location $scriptPath

try {
    Write-Host "Building Core project..." -ForegroundColor Green
    
    # Run build with detailed output and capture output as an array of strings
    $buildOutput = & dotnet build "src\HyperVCreator.Core\HyperVCreator.Core.csproj" -v detailed 2>&1
    
    # Display output
    $buildOutput | ForEach-Object { Write-Host $_ }
    
    # Count and summarize errors and warnings
    $errors = $buildOutput | Where-Object { $_ -match "error" }
    $warnings = $buildOutput | Where-Object { $_ -match "warning" }
    
    Write-Host "`nSummary:" -ForegroundColor Cyan
    Write-Host "  Errors: $($errors.Count)" -ForegroundColor $(if ($errors.Count -gt 0) { "Red" } else { "Green" })
    Write-Host "  Warnings: $($warnings.Count)" -ForegroundColor $(if ($warnings.Count -gt 0) { "Yellow" } else { "Green" })
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nBuild completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "`nBuild failed with exit code: $LASTEXITCODE" -ForegroundColor Red
        
        # Print unique error types
        if ($errors.Count -gt 0) {
            Write-Host "`nError categories:" -ForegroundColor Red
            $errorCodes = $errors | ForEach-Object { 
                if ($_ -match "CS\d{4}") { $matches[0] }
            } | Select-Object -Unique
            
            foreach ($code in $errorCodes) {
                $count = ($errors | Where-Object { $_ -match $code }).Count
                Write-Host "  $code : $count occurrences" -ForegroundColor Red
            }
        }
    }
} catch {
    Write-Host "An error occurred during build: $_" -ForegroundColor Red
}

Write-Host "`nPress any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") 
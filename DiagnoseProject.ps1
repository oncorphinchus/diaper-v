Write-Host "Hyper-V Creator Project Diagnostics" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan

# Set working directory to script location
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
Set-Location $scriptPath

# Check .NET SDK version
Write-Host "`nChecking .NET SDK:" -ForegroundColor Yellow
try {
    $dotnetVersion = & dotnet --version
    Write-Host "  .NET SDK Version: $dotnetVersion" -ForegroundColor Green
    
    $dotnetInfo = & dotnet --info
    Write-Host "  .NET SDK Info:" -ForegroundColor Green
    $dotnetInfo | ForEach-Object { Write-Host "    $_" -ForegroundColor Gray }
} catch {
    Write-Host "  Error getting .NET SDK info: $_" -ForegroundColor Red
}

# Check project files
Write-Host "`nChecking Project Files:" -ForegroundColor Yellow
$appProjPath = "src\HyperVCreator.App\HyperVCreator.App.csproj"
$coreProjPath = "src\HyperVCreator.Core\HyperVCreator.Core.csproj"

if (Test-Path $appProjPath) {
    $appProjContent = Get-Content $appProjPath -Raw
    Write-Host "  App Project: Found" -ForegroundColor Green
    
    # Extract target framework
    if ($appProjContent -match "<TargetFramework>(.*?)</TargetFramework>") {
        Write-Host "    Target Framework: $($matches[1])" -ForegroundColor Green
    }
    
    # Extract nullable setting
    if ($appProjContent -match "<Nullable>(.*?)</Nullable>") {
        Write-Host "    Nullable Setting: $($matches[1])" -ForegroundColor Green
    }
} else {
    Write-Host "  App Project: Not Found" -ForegroundColor Red
}

if (Test-Path $coreProjPath) {
    $coreProjContent = Get-Content $coreProjPath -Raw
    Write-Host "  Core Project: Found" -ForegroundColor Green
    
    # Extract target framework
    if ($coreProjContent -match "<TargetFramework>(.*?)</TargetFramework>") {
        Write-Host "    Target Framework: $($matches[1])" -ForegroundColor Green
    }
    
    # Extract nullable setting
    if ($coreProjContent -match "<Nullable>(.*?)</Nullable>") {
        Write-Host "    Nullable Setting: $($matches[1])" -ForegroundColor Green
    }
} else {
    Write-Host "  Core Project: Not Found" -ForegroundColor Red
}

# Check directory structure
Write-Host "`nChecking Directory Structure:" -ForegroundColor Yellow
$directories = @(
    "src\HyperVCreator.App",
    "src\HyperVCreator.Core",
    "src\HyperVCreator.Scripts"
)

foreach ($dir in $directories) {
    if (Test-Path $dir) {
        $fileCount = (Get-ChildItem $dir -Recurse -File).Count
        Write-Host "  $dir : Found ($fileCount files)" -ForegroundColor Green
    } else {
        Write-Host "  $dir : Not Found" -ForegroundColor Red
    }
}

# Try to build with diagnostic output
Write-Host "`nAttempting a diagnostic build of Core project:" -ForegroundColor Yellow
try {
    $buildOutput = & dotnet build $coreProjPath -v diagnostic 2>&1
    $buildOutput | Out-File "CoreBuildDiagnostic.log" -Encoding utf8
    
    Write-Host "  Build diagnostic output saved to CoreBuildDiagnostic.log" -ForegroundColor Green
    
    # Check for specific error patterns
    $errorLines = $buildOutput | Where-Object { $_ -match "error " }
    
    if ($errorLines.Count -gt 0) {
        Write-Host "  Found $($errorLines.Count) errors" -ForegroundColor Red
        
        # Extract unique error codes
        $errorCodes = @()
        foreach ($line in $errorLines) {
            if ($line -match "error (CS\d+)") {
                $errorCodes += $matches[1]
            }
        }
        $uniqueErrorCodes = $errorCodes | Select-Object -Unique
        
        Write-Host "  Unique error codes:" -ForegroundColor Red
        foreach ($code in $uniqueErrorCodes) {
            $count = ($errorCodes | Where-Object { $_ -eq $code }).Count
            Write-Host "    $code : $count occurrences" -ForegroundColor Red
            
            # Show example for each error type
            $example = $errorLines | Where-Object { $_ -match "error $code" } | Select-Object -First 1
            Write-Host "      Example: $example" -ForegroundColor Gray
        }
    } else {
        Write-Host "  No errors found in build output" -ForegroundColor Green
    }
} catch {
    Write-Host "  Error running diagnostic build: $_" -ForegroundColor Red
}

Write-Host "`nDiagnostics completed. Check results above and review CoreBuildDiagnostic.log for details." -ForegroundColor Cyan
Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") 
# Script to build HyperVCreator.Core project and verify it builds successfully
$ErrorActionPreference = "Stop"

Write-Host "Building HyperVCreator.Core..." -ForegroundColor Green

try {
    # Clean bin and obj directories to ensure clean build
    $corePath = Join-Path -Path $PSScriptRoot -ChildPath "src\HyperVCreator.Core"
    $binPath = Join-Path -Path $corePath -ChildPath "bin"
    $objPath = Join-Path -Path $corePath -ChildPath "obj"
    
    if (Test-Path $binPath) {
        Write-Host "Cleaning bin directory..." -ForegroundColor Yellow
        Remove-Item -Path $binPath -Recurse -Force
    }
    
    if (Test-Path $objPath) {
        Write-Host "Cleaning obj directory..." -ForegroundColor Yellow
        Remove-Item -Path $objPath -Recurse -Force
    }
    
    # Run the build
    Write-Host "Executing dotnet build..." -ForegroundColor Yellow
    $buildOutput = & dotnet build "$corePath\HyperVCreator.Core.csproj" -v:d 2>&1
    
    # Check the exit code
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✓ Build succeeded!" -ForegroundColor Green
        Write-Host "All nullable reference fixes are working correctly." -ForegroundColor Green
    } else {
        Write-Host "`n❌ Build failed with exit code $LASTEXITCODE" -ForegroundColor Red
        
        # Filter for nullable reference issues
        $nullableErrors = $buildOutput | Where-Object { $_ -match "CS0019" -or $_ -match "Operator '&&' cannot be applied to operands of type 'bool\?' and 'bool'" }
        
        if ($nullableErrors) {
            Write-Host "`nFound nullable boolean errors:" -ForegroundColor Red
            $nullableErrors | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        }
        
        # Output all errors
        Write-Host "`nAll errors:" -ForegroundColor Yellow
        $buildOutput | Where-Object { $_ -match "error" } | ForEach-Object { Write-Host $_ -ForegroundColor Yellow }
    }
    
} catch {
    Write-Host "An error occurred: $_" -ForegroundColor Red
}

Write-Host "`nPress any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") 
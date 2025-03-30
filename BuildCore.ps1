Write-Host "Cleaning and Building HyperVCreator.Core..." -ForegroundColor Cyan

# Set working directory to script location
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
Set-Location $scriptPath

Write-Host "Cleaning..." -ForegroundColor Yellow
if (Test-Path "src\HyperVCreator.Core\bin") {
    Remove-Item -Recurse -Force "src\HyperVCreator.Core\bin"
}
if (Test-Path "src\HyperVCreator.Core\obj") {
    Remove-Item -Recurse -Force "src\HyperVCreator.Core\obj"
}

try {
    Write-Host "Building..." -ForegroundColor Green
    $buildOutput = & dotnet build "src\HyperVCreator.Core\HyperVCreator.Core.csproj" -v normal 2>&1
    $buildOutput | ForEach-Object { Write-Host $_ }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Build completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "Build failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    }
} catch {
    Write-Host "An error occurred during build: $_" -ForegroundColor Red
}

Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown") 
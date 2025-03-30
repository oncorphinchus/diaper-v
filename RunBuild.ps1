Write-Host "Building HyperVCreator.Core..." -ForegroundColor Cyan

try {
    $output = (dotnet build src\HyperVCreator.Core\HyperVCreator.Core.csproj -v normal) | Out-String
    Write-Host $output
    
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
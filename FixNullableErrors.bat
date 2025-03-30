@echo off
echo Building HyperVCreator.Core after fixing nullable bool errors...
cd /d %~dp0
dotnet build src\HyperVCreator.Core\HyperVCreator.Core.csproj
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Build failed with exit code: %ERRORLEVEL%
) else (
    echo.
    echo Build completed successfully!
)
echo.
echo Press any key to exit...
pause > nul 
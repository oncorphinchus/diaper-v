@echo off
echo Building and running HyperV Creator Test Harness...

cd src\HyperVCreator.App
dotnet build -c Debug

if %ERRORLEVEL% NEQ 0 (
    echo Build failed with error code %ERRORLEVEL%
    echo Press any key to exit...
    pause > nul
    exit /b %ERRORLEVEL%
)

echo Build successful.
echo.
echo Starting Test Runner...
echo.

dotnet run -c Debug --no-build --startup-object HyperVCreator.App.TestRunner

cd ..\..

echo.
echo Test run complete.
echo Press any key to exit...
pause > nul 
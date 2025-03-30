@echo off
echo Running HyperV Creator Tests and Generating Reports...
echo.

REM Change to the root directory first to ensure we're in the right location
cd /d "%~dp0"

REM Check if project exists before proceeding
if not exist "src\HyperVCreator.App\HyperVCreator.App.csproj" (
    echo ERROR: Project file not found at src\HyperVCreator.App\HyperVCreator.App.csproj
    echo Current directory: %CD%
    echo Please run this batch file from the root of the project directory.
    echo Press any key to exit...
    pause > nul
    exit /b 1
)

echo Step 1: Building test harness...
cd src\HyperVCreator.App
dotnet build -c Debug

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Build failed with error code %ERRORLEVEL%
    echo Press any key to exit...
    pause > nul
    exit /b %ERRORLEVEL%
)

echo Build successful.
echo.
echo Step 2: Running Test Harness...
echo.

dotnet run -c Debug --no-build --startup-object HyperVCreator.App.TestRunner

echo.
echo Step 3: Running PowerShell diagnostics...
cd ..\..
powershell -ExecutionPolicy Bypass -File DiagnosePowerShell.ps1

echo.
echo Step 4: Running unit tests...
cd src\HyperVCreator.Core
dotnet test --filter Category=UnitTest --logger "console;verbosity=detailed"
cd ..\..

echo.
echo All tests and diagnostics complete!
echo Reports have been generated in the Documents\HyperVCreator\TestReports folder.
echo.

echo Press any key to exit...
pause > nul 
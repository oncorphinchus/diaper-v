@echo off
echo Building and running HyperV Creator Test Console...

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
echo Starting Test Console...
echo.

dotnet run -c Debug --no-build --startup-object HyperVCreator.App.TestConsole

cd ..\..

echo.
echo Test console session complete.
echo Press any key to exit...
pause > nul 
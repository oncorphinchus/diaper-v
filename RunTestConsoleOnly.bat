@echo off
echo Running HyperV Creator Test Console...
echo.

REM Change to the root directory
cd /d "%~dp0"
echo Current directory: %CD%
echo.

if exist "src\HyperVCreator.App\HyperVCreator.App.csproj" (
    echo Running TestConsole...
    dotnet run -c Debug --project src\HyperVCreator.App\HyperVCreator.App.csproj --startup-object HyperVCreator.App.TestConsole
    
    if %ERRORLEVEL% NEQ 0 (
        echo ERROR: TestConsole execution failed with error code %ERRORLEVEL%
        echo Press any key to exit...
        pause > nul
        exit /b %ERRORLEVEL%
    )
) else (
    echo ERROR: Cannot find application project.
    echo Press any key to exit...
    pause > nul
    exit /b 1
)

echo.
echo Test console session complete.
echo Press any key to exit...
pause > nul 
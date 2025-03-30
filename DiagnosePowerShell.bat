@echo off
echo Running PowerShell Environment Diagnostic Tool...
echo.

:: Check if PowerShell is available
where powershell >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: PowerShell is not available on this system!
    echo Press any key to exit...
    pause > nul
    exit /b 1
)

:: Run the diagnostic script with bypass execution policy
powershell -ExecutionPolicy Bypass -File DiagnosePowerShell.ps1

echo.
echo PowerShell diagnostics complete.
echo. 
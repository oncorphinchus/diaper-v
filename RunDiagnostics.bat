@echo off
echo Running Hyper-V Creator Diagnostics...
echo.

echo Checking if PowerShell is available...
where powershell >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: PowerShell is not available on this system!
    goto Error
)

echo PowerShell is available.
echo.

echo Checking PowerShell execution policy...
powershell -Command "Get-ExecutionPolicy"
echo.

echo Running PowerShell diagnostics...
powershell -ExecutionPolicy Bypass -File DiagnosePowerShell.ps1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: PowerShell diagnostics failed with error code %ERRORLEVEL%
    goto Error
)

echo PowerShell diagnostics completed successfully.
echo.

echo Running test harness...
cd src\HyperVCreator.App
dotnet build -c Debug
dotnet run -c Debug --no-build --startup-object HyperVCreator.App.TestRunner
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Test harness failed with error code %ERRORLEVEL%
    goto Error
)
cd ..\..

echo All diagnostics completed successfully!
goto End

:Error
echo An error occurred during diagnostics.
echo Please check the error messages above.

:End
echo.
echo Press any key to exit...
pause > nul 
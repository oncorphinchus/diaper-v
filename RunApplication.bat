@echo off
echo Building and running Hyper-V Creator application...

cd src\HyperVCreator.App
dotnet build --configuration Debug

if %ERRORLEVEL% NEQ 0 (
    echo Build failed with error code %ERRORLEVEL%
    echo Press any key to exit...
    pause > nul
    exit /b %ERRORLEVEL%
)

echo Build successful. Starting application...
start "" "bin\Debug\net6.0-windows\HyperVCreator.App.exe"
cd ..\..

echo Application started.
echo Press any key to exit...
pause > nul 
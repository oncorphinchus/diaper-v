@echo off
echo Building HyperV Creator Core project...

REM Change to the root directory
cd /d "%~dp0"

REM Clean build outputs first
echo Cleaning Core build outputs...
if exist "src\HyperVCreator.Core\bin" rmdir /s /q "src\HyperVCreator.Core\bin"
if exist "src\HyperVCreator.Core\obj" rmdir /s /q "src\HyperVCreator.Core\obj"

REM Build the Core project
echo Building Core project...
dotnet build "src\HyperVCreator.Core\HyperVCreator.Core.csproj"

echo.
echo Build completed.
echo Press any key to exit...
pause > nul 
@echo off
echo Building HyperV Creator App project...
cd /d "%~dp0"

echo Cleaning App project...
rmdir /s /q "src\HyperVCreator.App\bin" 2>nul
rmdir /s /q "src\HyperVCreator.App\obj" 2>nul

echo Building App project...
dotnet build src\HyperVCreator.App\HyperVCreator.App.csproj --no-incremental

echo.
echo Build process completed.
echo Press any key to exit...
pause 
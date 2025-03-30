@echo off
echo Cleaning and rebuilding HyperV Creator solution...
echo.

REM Change to the root directory
cd /d "%~dp0"
echo Current directory: %CD%
echo.

REM Clean the build outputs
echo Cleaning build outputs...
if exist "src\HyperVCreator.App\bin" (
    rmdir /s /q "src\HyperVCreator.App\bin"
    echo Cleaned App\bin
)
if exist "src\HyperVCreator.App\obj" (
    rmdir /s /q "src\HyperVCreator.App\obj"
    echo Cleaned App\obj
)
if exist "src\HyperVCreator.Core\bin" (
    rmdir /s /q "src\HyperVCreator.Core\bin"
    echo Cleaned Core\bin
)
if exist "src\HyperVCreator.Core\obj" (
    rmdir /s /q "src\HyperVCreator.Core\obj"
    echo Cleaned Core\obj
)
echo.

REM Rebuild the solution
echo Rebuilding solution...
if exist "HyperVCreator.sln" (
    dotnet build -c Debug
) else (
    echo Building individual projects...
    
    if exist "src\HyperVCreator.Core\HyperVCreator.Core.csproj" (
        echo Building Core project...
        dotnet build src\HyperVCreator.Core\HyperVCreator.Core.csproj -c Debug
    )
    
    if exist "src\HyperVCreator.App\HyperVCreator.App.csproj" (
        echo Building App project...
        dotnet build src\HyperVCreator.App\HyperVCreator.App.csproj -c Debug
    )
)

echo.
echo Clean and rebuild completed.
echo Press any key to exit...
pause > nul 
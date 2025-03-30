@echo off
echo Verifying HyperV Creator project structure...
echo.

REM Change to the root directory
cd /d "%~dp0"
echo Current directory: %CD%
echo.

echo Checking solution file...
if exist "HyperVCreator.sln" (
    echo [FOUND] HyperVCreator.sln
) else (
    echo [MISSING] HyperVCreator.sln
)
echo.

echo Checking App project...
if exist "src\HyperVCreator.App\HyperVCreator.App.csproj" (
    echo [FOUND] src\HyperVCreator.App\HyperVCreator.App.csproj
) else (
    echo [MISSING] src\HyperVCreator.App\HyperVCreator.App.csproj
)
echo.

echo Checking Core project...
if exist "src\HyperVCreator.Core\HyperVCreator.Core.csproj" (
    echo [FOUND] src\HyperVCreator.Core\HyperVCreator.Core.csproj
) else (
    echo [MISSING] src\HyperVCreator.Core\HyperVCreator.Core.csproj
)
echo.

echo Checking directory structure...
echo.
echo Directory listing for project root:
dir /b
echo.

echo Directory listing for src folder:
if exist "src" (
    dir /b src
) else (
    echo [MISSING] src directory
)
echo.

echo Directory listing for src\HyperVCreator.App:
if exist "src\HyperVCreator.App" (
    dir /b src\HyperVCreator.App
) else (
    echo [MISSING] src\HyperVCreator.App directory
)
echo.

echo Directory listing for src\HyperVCreator.Core:
if exist "src\HyperVCreator.Core" (
    dir /b src\HyperVCreator.Core
) else (
    echo [MISSING] src\HyperVCreator.Core directory
)
echo.

echo Testing dotnet CLI...
dotnet --version

echo.
echo Verify complete. Press any key to exit...
pause > nul 
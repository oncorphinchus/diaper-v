@echo off
echo Building and running HyperV Creator...
echo.

REM Change to the root directory
cd /d "%~dp0"
echo Current directory: %CD%
echo.

REM First try to build the solution itself
if exist "HyperVCreator.sln" (
    echo Building solution...
    dotnet build HyperVCreator.sln -c Debug
    
    if %ERRORLEVEL% NEQ 0 (
        echo ERROR: Solution build failed with error code %ERRORLEVEL%
        echo Press any key to exit...
        pause > nul
        exit /b %ERRORLEVEL%
    )
    
    echo Solution build successful.
) else (
    echo HyperVCreator.sln not found, trying individual projects...
    
    if exist "src\HyperVCreator.Core\HyperVCreator.Core.csproj" (
        echo Building Core project...
        dotnet build src\HyperVCreator.Core\HyperVCreator.Core.csproj -c Debug
        
        if %ERRORLEVEL% NEQ 0 (
            echo ERROR: Core project build failed with error code %ERRORLEVEL%
            echo Press any key to exit...
            pause > nul
            exit /b %ERRORLEVEL%
        )
        
        echo Core project build successful.
    ) else (
        echo Core project not found.
    )
    
    if exist "src\HyperVCreator.App\HyperVCreator.App.csproj" (
        echo Building App project...
        dotnet build src\HyperVCreator.App\HyperVCreator.App.csproj -c Debug
        
        if %ERRORLEVEL% NEQ 0 (
            echo ERROR: App project build failed with error code %ERRORLEVEL%
            echo Press any key to exit...
            pause > nul
            exit /b %ERRORLEVEL%
        )
        
        echo App project build successful.
    ) else (
        echo App project not found.
    )
)

echo.
echo Starting application...
echo.

if exist "src\HyperVCreator.App\HyperVCreator.App.csproj" (
    echo Running HyperVCreator.App...
    dotnet run -c Debug --project src\HyperVCreator.App\HyperVCreator.App.csproj
) else (
    echo ERROR: Cannot find application project to run.
    echo Press any key to exit...
    pause > nul
    exit /b 1
)

echo.
echo Application session complete.
echo Press any key to exit...
pause > nul 
@echo off
echo Building HyperV Creator Projects...
cd /d %~dp0

echo.
echo -----------------------------------
echo Building Core Project...
echo -----------------------------------
dotnet build src\HyperVCreator.Core\HyperVCreator.Core.csproj
if %ERRORLEVEL% NEQ 0 (
    echo Core Project build failed with code: %ERRORLEVEL%
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo -----------------------------------
echo Building App Project...
echo -----------------------------------
dotnet build src\HyperVCreator.App\HyperVCreator.App.csproj
if %ERRORLEVEL% NEQ 0 (
    echo App Project build failed with code: %ERRORLEVEL%
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo -----------------------------------
echo All projects built successfully!
echo -----------------------------------
pause 
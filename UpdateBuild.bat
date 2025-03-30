@echo off
echo Updating Hyper-V Creator Core project Nullable settings and building...
cd /d %~dp0

echo.
echo Checking for Core project file...
if not exist "src\HyperVCreator.Core\HyperVCreator.Core.csproj" (
    echo ERROR: Core project file not found!
    goto :error
)

echo.
echo Updating Nullable setting in Core project...
powershell -Command "(Get-Content 'src\HyperVCreator.Core\HyperVCreator.Core.csproj') -replace '<Nullable>enable</Nullable>', '<Nullable>warnings</Nullable>' | Set-Content 'src\HyperVCreator.Core\HyperVCreator.Core.csproj'"

echo.
echo Building Core project...
dotnet build src\HyperVCreator.Core\HyperVCreator.Core.csproj -c Debug

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Build failed with code %ERRORLEVEL%
    goto :error
) else (
    echo.
    echo Core project built successfully!
)

echo.
echo Building App project...
dotnet build src\HyperVCreator.App\HyperVCreator.App.csproj -c Debug

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: App project build failed with code %ERRORLEVEL%
    goto :error
) else (
    echo.
    echo App project built successfully!
)

echo.
echo All projects built successfully!
goto :end

:error
echo.
echo Build process encountered errors. Please check the output above.

:end
echo.
echo Press any key to exit...
pause > nul 
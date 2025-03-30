@echo off
echo Building HyperVCreator solution with fixed implicit operator...
cd /d %~dp0

echo.
echo Building Core project...
dotnet build src\HyperVCreator.Core\HyperVCreator.Core.csproj

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Core project build failed with code %ERRORLEVEL%
    goto :error
) else (
    echo.
    echo Core project built successfully!
)

echo.
echo Building App project...
dotnet build src\HyperVCreator.App\HyperVCreator.App.csproj

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo App project build failed with code %ERRORLEVEL%
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
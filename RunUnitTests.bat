@echo off
echo Running HyperV Creator Unit Tests...

cd src\HyperVCreator.Core
dotnet test --filter Category=UnitTest --logger "console;verbosity=detailed"

if %ERRORLEVEL% NEQ 0 (
    echo Unit tests failed with error code %ERRORLEVEL%
) else (
    echo All unit tests passed!
)

cd ..\..

echo.
echo Press any key to exit...
pause > nul 
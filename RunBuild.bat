@echo off
echo Building HyperVCreator.Core...
dotnet build src\HyperVCreator.Core\HyperVCreator.Core.csproj -v normal
if %ERRORLEVEL% NEQ 0 (
    echo Build failed with error level %ERRORLEVEL%
) else (
    echo Build succeeded!
)
pause 
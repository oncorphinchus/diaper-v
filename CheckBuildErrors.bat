@echo off
echo Building HyperVCreator.Core with detailed errors...
cd /d %~dp0
dotnet build src\HyperVCreator.Core\HyperVCreator.Core.csproj -v detailed
echo.
echo Build process completed with exit code: %ERRORLEVEL%
echo.
pause 
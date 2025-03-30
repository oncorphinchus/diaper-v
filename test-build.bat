@echo off
cd src\HyperVCreator.Core
dotnet build -verbosity:normal
echo Build completed with error level %ERRORLEVEL%
pause 
@echo off
echo Cleaning and Building HyperVCreator.Core...

echo Cleaning...
if exist "src\HyperVCreator.Core\bin" rmdir /s /q "src\HyperVCreator.Core\bin"
if exist "src\HyperVCreator.Core\obj" rmdir /s /q "src\HyperVCreator.Core\obj"

echo Building...
dotnet build "src\HyperVCreator.Core\HyperVCreator.Core.csproj" -v normal

echo Done.
pause 
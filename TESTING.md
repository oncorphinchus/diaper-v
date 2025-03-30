# HyperV Creator - Testing Guide

This document provides instructions for testing the HyperV Creator application.

## Prerequisites

1. Windows 10 or 11 with Hyper-V feature enabled
2. .NET 9.0 SDK or runtime installed
3. PowerShell 5.1 or higher
4. Administrative privileges (for Hyper-V operations)

## Quick Start

To quickly test the application, run one of the following batch files from the root directory:

- `RunTestConsole.bat` - Launches the interactive test console UI
- `TestAndReport.bat` - Runs all automated tests and generates reports
- `RunTests.bat` - Runs only the test harness
- `RunUnitTests.bat` - Runs only the unit tests
- `DiagnosePowerShell.bat` - Checks PowerShell environment setup

## Test Console

The test console provides an interactive menu-driven interface for testing core functionality:

1. **PowerShell Test** - Tests basic PowerShell integration
2. **Create Test VM** - Tests VM creation using a default template
3. **List Templates** - Tests template loading and shows available templates
4. **Run Test Harness** - Runs the full test harness
5. **Exit** - Exits the test console

## Test Reports

Test reports are generated in HTML format and saved to:
`%userprofile%\Documents\HyperVCreator\TestReports`

These reports include:
- Overall test summary
- Detailed test results by component
- System information
- Performance metrics
- Error details (if any)

## Manual Testing Checklist

1. **PowerShell Environment**
   - [ ] PowerShell execution policy allows script execution
   - [ ] PowerShell has access to required modules
   - [ ] Administrative permissions are available

2. **Template Management**
   - [ ] Default templates are available
   - [ ] Templates can be loaded and saved
   - [ ] Template customization works correctly

3. **VM Creation**
   - [ ] Basic VM can be created
   - [ ] Role-specific configuration works correctly
   - [ ] Progress reporting functions properly

4. **UI Components**
   - [ ] Themes can be changed
   - [ ] UI elements are properly styled
   - [ ] Forms and controls function as expected

## Troubleshooting

### Common Issues

1. **PowerShell Execution Policy**
   ```powershell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

2. **Hyper-V Not Enabled**
   Run PowerShell as Administrator:
   ```powershell
   Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V -All
   ```
   
3. **Missing Hyper-V Module**
   Run PowerShell as Administrator:
   ```powershell
   Install-WindowsFeature -Name Hyper-V-PowerShell
   ```

### Diagnostic Files

- **PowerShell Diagnostics**: Check `%temp%\HyperVCreatorDiagnostics_<timestamp>.log`
- **Application Logs**: Check `%appdata%\HyperVCreator\Logs`
- **Test Reports**: See `%userprofile%\Documents\HyperVCreator\TestReports`

## Feedback and Issue Reporting

For issues or feedback from testing, please:
1. Take screenshots of any errors
2. Save the test report
3. Note the steps to reproduce
4. Include system information from the diagnostic logs 
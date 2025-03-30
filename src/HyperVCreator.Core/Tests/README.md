# HyperVCreator Test Framework

This folder contains the unit tests for the HyperVCreator.Core library.

## Overview

The test framework uses MSTest to verify the functionality of the Core services and components. It includes tests for:

- PowerShell integration
- Template management
- VM creation and configuration
- Theme management
- Role-specific configuration

## Running Tests

Tests can be run in several ways:

1. Using the TestRunner console application:
   ```
   RunTests.bat
   ```

2. Using the MSTest runner:
   ```
   RunUnitTests.bat
   ```

3. From Visual Studio Test Explorer

## Test Structure

### Test Base Class

The `TestBase` class provides common functionality for all tests, including:

- Directory and file management for test data
- Helper methods for creating test files
- Clean-up of test resources

### Test Categories

Tests are organized into the following categories:

- **Unit Tests**: Focus on individual components in isolation
- **Integration Tests**: Test the interaction between components
- **PowerShell Tests**: Specifically test PowerShell script execution
- **Template Tests**: Verify template creation, loading, and saving
- **Configuration Tests**: Test server role configuration

## Adding New Tests

To add a new test class:

1. Create a new file in the Tests folder
2. Inherit from `TestBase` class
3. Add the `[TestClass]` attribute to your class
4. Add test methods with the `[TestMethod]` attribute
5. Consider adding appropriate `[TestCategory]` attributes

Example:

```csharp
[TestClass]
public class MyServiceTests : TestBase
{
    [TestMethod]
    [TestCategory("UnitTest")]
    public void MyMethod_ShouldDoSomething()
    {
        // Arrange
        var service = new MyService();
        
        // Act
        bool result = service.MyMethod();
        
        // Assert
        Assert.IsTrue(result);
    }
}
```

## Test Reports

The test framework generates detailed HTML reports for each test run. These reports include:

- Overall test summary
- Pass/fail status for each test
- Detailed error information for failed tests
- System information for diagnostics

Reports are saved in the user's Documents folder under `HyperVCreator\TestReports`.

## PowerShell Environment Testing

The framework includes specialized tests for verifying the PowerShell environment setup. These tests check:

- PowerShell version
- Execution policy
- Hyper-V module availability
- Required cmdlets presence
- Virtual switch configuration

To diagnose PowerShell-related issues, run:

```
DiagnosePowerShell.bat
```

This will generate a detailed report of your PowerShell environment configuration. 
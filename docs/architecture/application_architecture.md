# Application Architecture

This document outlines the high-level architecture for the Hyper-V VM Creation Application.

## Overview

The application is designed with a layered architecture following MVVM (Model-View-ViewModel) pattern, which is well-suited for WPF applications. This design provides separation of concerns, testability, and maintainability.

## Architecture Layers

![Architecture Diagram (To Be Created)]()

### Presentation Layer

The presentation layer consists of the user interface components and implements the View part of MVVM.

#### Components
- **Main Application Window**: The entry point and container for all application views
- **Navigation System**: Controls view switching and maintains application state
- **Server Role Views**: Specialized forms for each supported server role
- **Template Management Views**: Interfaces for creating and managing templates
- **Theme System**: Resource dictionaries and visual assets for different themes
- **Progress Monitoring**: Real-time feedback on VM creation status

#### Responsibilities
- Present information to the user
- Capture user input
- Validate input at the UI level
- Apply visual themes
- Provide feedback on operations

### ViewModel Layer

The ViewModel layer serves as an abstraction of the View and handles the View's display logic.

#### Components
- **MainViewModel**: Controls the overall application state
- **ServerRoleViewModels**: One ViewModel per supported server role
- **TemplateViewModel**: Manages template operations
- **ProgressViewModel**: Tracks and reports operation progress
- **SettingsViewModel**: Manages application settings including themes

#### Responsibilities
- Process user input from the View
- Coordinate with the Business Logic Layer to execute operations
- Maintain View state
- Provide data for View binding
- Implement input validation logic
- Handle view-specific errors

### Business Logic Layer

The Business Logic Layer implements the core application functionality and business rules.

#### Components
- **VMCreationService**: Orchestrates the VM creation process
- **TemplateService**: Manages VM templates
- **PowerShellService**: Handles PowerShell script execution
- **ValidationService**: Validates configurations against business rules
- **ThemeService**: Manages theme selection and application

#### Responsibilities
- Implement business rules and validations
- Coordinate Hyper-V operations
- Manage templates
- Control PowerShell script execution
- Handle application-level errors

### Data Access Layer

The Data Access Layer manages data persistence and retrieval.

#### Components
- **TemplateRepository**: Stores and retrieves VM templates
- **SettingsRepository**: Manages application settings
- **PowerShellScriptRepository**: Accesses PowerShell scripts
- **UnattendXMLRepository**: Manages Unattend.xml templates

#### Responsibilities
- Read and write application data
- Serialize and deserialize objects
- Manage file system access
- Handle data access errors

### Infrastructure Layer

The Infrastructure Layer provides cross-cutting concerns and utilities.

#### Components
- **LoggingService**: Application-wide logging
- **ErrorHandlingService**: Centralized error management
- **ConfigurationService**: Application configuration
- **SecurityService**: Handles security concerns

#### Responsibilities
- Provide logging across the application
- Manage errors and exceptions
- Handle application configuration
- Implement security features

## Core Interactions and Workflows

### VM Creation Workflow

1. **User Input**: User selects a server role and provides required parameters
2. **Validation**: Parameters are validated against business rules
3. **VM Creation**:
   - Hyper-V VM is created with specified hardware
   - Virtual disk is created and configured
   - Network adapters are configured
4. **OS Installation**:
   - Appropriate OS ISO is mounted
   - Unattend.xml is generated and attached
   - VM is started for unattended OS installation
5. **Role Configuration**:
   - PowerShell DSC scripts are executed to configure the server role
   - Post-installation tasks are performed
6. **Completion**: User is notified of successful creation

### Template Management Workflow

1. **Template Creation**: User creates a new template based on predefined or custom parameters
2. **Template Storage**: Template is serialized and stored in the repository
3. **Template Selection**: User selects a template for VM creation
4. **Template Application**: Template parameters are applied to the VM creation process

### Theme Management Workflow

1. **Theme Selection**: User selects a theme from available options
2. **Theme Application**: Selected theme's resource dictionary is loaded
3. **Theme Persistence**: Selected theme is saved in application settings

## PowerShell Integration

The application uses PowerShell extensively for VM creation and configuration:

### PowerShell Execution Approaches

1. **Runspace Management**:
   - Create and manage PowerShell runspaces
   - Execute scripts in isolated runspaces
   - Capture and parse script output

2. **Script Organization**:
   - Modular script design with clear responsibilities
   - Parameter validation in scripts
   - Proper error handling and reporting

3. **Security Considerations**:
   - Execute with appropriate privileges
   - Parameter sanitization
   - Script signing (optional)

## Data Storage

### Template Storage

Templates are stored as structured data files:

```json
{
  "TemplateName": "Standard Domain Controller",
  "ServerRole": "DomainController",
  "HardwareConfiguration": {
    "ProcessorCount": 2,
    "MemoryGB": 4,
    "StorageGB": 80
  },
  "NetworkConfiguration": {
    "VirtualSwitch": "Default Switch",
    "StaticIP": false
  },
  "DomainConfiguration": {
    "DomainName": "contoso.local",
    "NetBIOSName": "CONTOSO",
    "ForestFunctionalLevel": "WinThreshold"
  }
}
```

### Settings Storage

Application settings are stored in a similar structured format:

```json
{
  "Theme": "Classic",
  "DefaultTemplatesPath": "C:\\ProgramData\\VMCreator\\Templates",
  "ISOPath": "C:\\ISOs",
  "DefaultVirtualHardDiskPath": "C:\\Users\\Public\\Documents\\Hyper-V\\Virtual Hard Disks"
}
```

## Error Handling Strategy

The application implements a comprehensive error handling strategy:

1. **Error Categories**:
   - UI Validation Errors
   - PowerShell Execution Errors
   - Hyper-V API Errors
   - File System Errors
   - Unexpected Exceptions

2. **Error Handling Approach**:
   - Preventative validation
   - Try-catch blocks with specific exception handling
   - Centralized error logging
   - User-friendly error messages
   - Recovery suggestions where possible

## Security Considerations

1. **Credential Handling**:
   - Secure storage of saved credentials
   - Proper handling of administrator passwords
   - No persistence of sensitive data

2. **Execution Privileges**:
   - Running with appropriate privileges for Hyper-V management
   - Least privilege principle for operations
   - Elevation prompts when required

## Extensibility Points

The application is designed with extensibility in mind:

1. **New Server Roles**:
   - Modular design allows adding new server role templates
   - Clear interfaces for implementing new role requirements

2. **Additional Themes**:
   - Theme system designed for easy addition of new themes
   - Separation of theme resources from application logic

3. **Enhanced Functionality**:
   - Plugin architecture for future extensions
   - Well-defined interfaces for core components 
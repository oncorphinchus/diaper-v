# Deployment Guide

This document outlines the process for packaging, distributing, and installing the Hyper-V VM Creation Application.

## Prerequisites

### Development Environment Requirements

- **Visual Studio 2022** (any edition)
- **.NET 6.0 SDK** or later
- **WiX Toolset v3.11** or later
- **PowerShell 7.1** or later
- **Git** for version control

### Build Server Requirements

- **Windows Server 2019/2022** or **Windows 10/11**
- **.NET 6.0 SDK** or later
- **WiX Toolset v3.11** or later
- **PowerShell 7.1** or later
- **MSBuild** 17.0 or later
- **Visual Studio Build Tools 2022**

### Target System Requirements

- **Operating System**: Windows 10/11 or Windows Server 2019/2022
- **Hyper-V Feature**: Enabled
- **PowerShell**: Version 5.1 or later
- **.NET Runtime**: 6.0 or later
- **Minimum Hardware**:
  - 4GB RAM (8GB recommended)
  - 50MB free disk space for application
  - Additional space for VM storage
  - CPU with virtualization capabilities
- **User Permissions**: Administrator access for Hyper-V management

## Application Versioning

### Version Number Format

The application follows Semantic Versioning (SemVer) with the format: `MAJOR.MINOR.PATCH`

- **MAJOR**: Incremented for incompatible API changes
- **MINOR**: Incremented for new functionality in a backward-compatible manner
- **PATCH**: Incremented for backward-compatible bug fixes

### Version Management

- Version numbers are maintained in:
  - Assembly information
  - Installer package
  - Application About screen
  - Release notes

## Build Process

### Local Development Build

1. **Restore dependencies**:
   ```
   dotnet restore HyperVCreator.sln
   ```

2. **Build the project**:
   ```
   dotnet build HyperVCreator.sln --configuration Release
   ```

3. **Run tests**:
   ```
   dotnet test HyperVCreator.sln --configuration Release
   ```

### Continuous Integration Build

The application uses a CI/CD pipeline with the following steps:

1. **Code Checkout**:
   - Retrieve latest code from repository

2. **Version Update**:
   - Update version numbers in assembly info
   - Generate build number

3. **Dependency Restoration**:
   ```
   dotnet restore HyperVCreator.sln
   ```

4. **Build**:
   ```
   dotnet build HyperVCreator.sln --configuration Release
   ```

5. **Unit Tests**:
   ```
   dotnet test HyperVCreator.sln --configuration Release
   ```

6. **Create Installer**:
   ```
   msbuild HyperVCreator.Setup.wixproj /p:Configuration=Release
   ```

7. **Sign Application and Installer** (if applicable):
   ```
   signtool sign /tr http://timestamp.digicert.com /td sha256 /fd sha256 /a "HyperVCreator.Setup.msi"
   ```

8. **Archive Artifacts**:
   - Store installer package
   - Store PDB files for debugging
   - Store build logs

## Installer Creation

### WiX Toolset Configuration

The application installer is created using WiX Toolset with the following configuration:

1. **Product Information**:
   ```xml
   <Product Id="*" Name="Hyper-V VM Creation Tool" 
            Language="1033" Version="$(var.ProductVersion)" 
            Manufacturer="Your Company" 
            UpgradeCode="PUT-GUID-HERE">
   ```

2. **Installation Directory**:
   ```xml
   <Directory Id="TARGETDIR" Name="SourceDir">
     <Directory Id="ProgramFilesFolder">
       <Directory Id="INSTALLFOLDER" Name="HyperV VM Creator" />
     </Directory>
   </Directory>
   ```

3. **Features**:
   ```xml
   <Feature Id="ProductFeature" Title="Hyper-V VM Creator" Level="1">
     <ComponentGroupRef Id="ProductComponents" />
     <ComponentGroupRef Id="PowerShellScripts" />
   </Feature>
   ```

4. **Custom Actions**:
   - Check for Hyper-V feature
   - Verify .NET runtime
   - Verify PowerShell version

### Installer Package Components

The installer package includes:

1. **Main Application**:
   - Executable and DLLs
   - Configuration files
   - UI resources

2. **PowerShell Scripts**:
   - VM creation scripts
   - Role configuration scripts
   - Common utility scripts

3. **Default Templates**:
   - Pre-configured templates for each server role

4. **Documentation**:
   - User manual
   - Quick start guide

## Distribution

### Distribution Channels

1. **Direct Download**:
   - Company website
   - Release notes page
   - Documentation link

2. **Enterprise Deployment**:
   - SCCM package
   - Group Policy installation
   - Network share

### Digital Signing

For production releases:

1. **Code Signing**:
   - All executables and DLLs are signed with a trusted certificate
   - PowerShell scripts are signed

2. **Installer Signing**:
   - MSI package is signed
   - Verification instructions provided to users

## Installation

### Interactive Installation

1. **Run Installer**:
   - Double-click the MSI file
   - Follow the installation wizard

2. **Installation Options**:
   - Installation location
   - Start menu shortcuts
   - Desktop shortcut
   - Auto-start option

### Silent Installation

For automated deployment:

```
msiexec /i HyperVCreator.msi /quiet INSTALLLOCATION="C:\Program Files\HyperV VM Creator" ADDDESKTOPSHORTCUT=1
```

### Installation Verification

Post-installation checks:

1. **File Verification**:
   - All files installed correctly
   - Correct file versions

2. **Runtime Verification**:
   - Application launches successfully
   - No error dialogs

3. **Feature Verification**:
   - Basic functionality check
   - Template access

## Updates

### Update Mechanism

The application checks for updates:

1. **Update Check**:
   - On application startup (configurable)
   - Manual check through Help menu

2. **Update Process**:
   - Notification to user
   - Download option
   - Installation guidance

### Update Deployment

1. **Full Installer**:
   - MSI package with new version
   - Automatic removal of previous version

2. **Patch Updates** (future):
   - MSP files for minor updates
   - Smaller download size

## Uninstallation

### Interactive Uninstallation

1. **Windows Control Panel**:
   - Programs and Features
   - Select "Hyper-V VM Creator"
   - Click "Uninstall"

2. **Uninstallation Options**:
   - Keep user data (templates, settings)
   - Complete removal

### Silent Uninstallation

For automated removal:

```
msiexec /x {PRODUCT-GUID} /quiet KEEPUSERDATA=1
```

### Cleanup Process

The uninstaller performs:

1. **Application Removal**:
   - Executables and DLLs
   - Start menu shortcuts
   - Desktop shortcuts

2. **Optional Cleanup**:
   - User settings
   - Templates
   - Logs

## Deployment Checklist

### Pre-Deployment

- [ ] All unit tests pass
- [ ] Integration tests pass
- [ ] Code review completed
- [ ] Documentation updated
- [ ] Version numbers updated
- [ ] Release notes prepared

### Deployment Package

- [ ] Installer builds successfully
- [ ] Digital signatures applied
- [ ] Installation tested on clean system
- [ ] Upgrade tested from previous version
- [ ] Silent installation verified
- [ ] Uninstallation verified

### Post-Deployment

- [ ] Deployment package archived
- [ ] Source code tagged with version
- [ ] Release announced to users
- [ ] Support documentation available
- [ ] Monitoring for installation issues 
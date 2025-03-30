# HyperV Creator

A powerful Windows application for creating and managing Hyper-V virtual machines with support for predefined server roles.

## Overview

HyperV Creator simplifies the process of creating and configuring virtual machines in Hyper-V. It provides templates for common server roles such as Domain Controllers, File Servers, Web Servers, and more.

## Features

- Create virtual machines with a few clicks
- Choose from predefined server role templates:
  - Domain Controller
  - RDSH (Remote Desktop Session Host)
  - File Server
  - Web Server (IIS)
  - SQL Server
  - DHCP Server
  - DNS Server
  - Custom VM
- Customize hardware settings
- Configure networking
- Automated OS installation
- Three theme options:
  - Classic
  - Dark
  - Sanrio

## Prerequisites

- Windows 10 or 11
- Hyper-V feature enabled
- .NET 9.0 SDK or runtime
- PowerShell 5.1 or higher
- Administrative privileges

## Quick Start

1. Clone the repository
2. Build the solution
3. Run the application

For quick testing:
- Run `TestAndReport.bat` to run all tests and generate a report
- Run `RunTestConsole.bat` to launch the interactive test console

## Development

### Project Structure

- `HyperVCreator.App` - WPF application with UI
- `HyperVCreator.Core` - Core functionality library
- `HyperVCreator.Scripts` - PowerShell scripts for VM creation and configuration

### Testing

See [TESTING.md](TESTING.md) for detailed testing instructions.

## Roadmap

See [Detailed_Roadmap_Hyper-V_VM_Creation.md](Detailed_Roadmap_Hyper-V_VM_Creation.md) for the project roadmap.

## License

MIT License
# Hyper-V VM Creation Application

A Windows application for creating pre-configured Hyper-V virtual machines with minimal user input.

## Overview

The Hyper-V VM Creation Application helps system administrators quickly deploy virtual machines with specific server roles. The application automates OS installation and role configuration, saving time and reducing configuration errors.

### Key Features

- **Role-Based VM Creation**: Pre-configured templates for common server roles
- **Simplified Configuration**: Automate OS installation and server setup
- **Template Management**: Store and manage VM templates
- **User-Friendly Interface**: Intuitive UI with multiple visual themes
- **PowerShell Automation**: Utilizing PowerShell for Hyper-V and OS configuration
- **Custom VM Creation**: Flexible VM provisioning without role-specific configurations

## Supported Server Roles

The application supports creating virtual machines with the following pre-configured roles:

- **Domain Controller**: Active Directory domain controllers with DNS
- **Remote Desktop Session Host (RDSH)**: Terminal servers for remote application access
- **File Server**: Dedicated file sharing servers with configurable shares
- **Web Server (IIS)**: Internet Information Services web servers
- **SQL Server**: Microsoft SQL Server database instances
- **DHCP Server**: Dynamic Host Configuration Protocol servers
- **DNS Server**: Domain Name System servers
- **Custom VM**: Flexible VMs with user-defined parameters without role-specific features

## Documentation

### Project Management

- [Detailed Roadmap](./Detailed_Roadmap_Hyper-V_VM_Creation.md) - Comprehensive project roadmap with milestones and tasks

### Requirements and Planning

- [Project Overview](./docs/project_overview.md) - High-level overview of the project
- [Server Roles](./docs/requirements/server_roles.md) - Detailed requirements for supported server roles

### Architecture and Design

- [Application Architecture](./docs/architecture/application_architecture.md) - System architecture and component design

### Development

- [Theme Design](./docs/development/theme_design.md) - Visual theme specifications and implementation details
- [PowerShell Scripting](./docs/development/powershell_scripting.md) - PowerShell integration and script design

### Testing

- [Testing Strategy](./docs/testing/testing_strategy.md) - Comprehensive testing approach and methodologies

### Deployment

- [Deployment Guide](./docs/deployment/deployment_guide.md) - Application packaging, distribution, and installation

## Getting Started

### Prerequisites

- Windows 10/11 or Windows Server 2019/2022
- Hyper-V feature enabled
- PowerShell 5.1 or later
- .NET Runtime 6.0 or later
- Administrator access

### Development Setup

1. Clone the repository
2. Open the solution in Visual Studio 2022
3. Restore NuGet packages
4. Build the solution

## Template Management

The application uses JSON-based templates to store VM configurations. You can create, edit, and manage templates through the user interface or directly modify the JSON files.

### Template Examples

The repository includes several example templates:
- Default role-specific templates for each supported server role
- High-performance custom VM template
- Minimal resource template for development/testing

## License

[License information to be determined]

## Contact

[Contact information to be determined]
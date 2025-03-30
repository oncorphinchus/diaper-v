# Hyper-V VM Creation Application Documentation

## Documentation Index

This index provides navigation to all documentation for the Hyper-V VM Creation Application project.

### Project Overview

- [Project Overview](./project_overview.md): High-level description of the project, goals, and value proposition

### Requirements Documentation

- [Server Roles](./requirements/server_roles.md): Detailed requirements for each supported server role
  - Common requirements for all server roles
  - Domain Controller (DC) specifications
  - Remote Desktop Session Host (RDSH) specifications
  - File Server specifications
  - Web Server (IIS) specifications
  - SQL Server specifications
  - DHCP Server specifications
  - DNS Server specifications
  - Custom VM specifications

### Architecture Documentation

- [Application Architecture](./architecture/application_architecture.md): Detailed system architecture
  - Architecture layers (MVVM)
  - Component responsibilities
  - Core interactions and workflows
  - PowerShell integration
  - Data storage approach
  - Error handling strategy
  - Security considerations
  - Extensibility points

### Development Documentation

- [Theme Design](./development/theme_design.md): Visual theme specifications
  - Theming architecture
  - Classic theme design
  - Sanrio theme design
  - Dark theme design
  - Implementation details
  - Design considerations
  - Animation and transitions

- [PowerShell Scripting](./development/powershell_scripting.md): PowerShell integration guide
  - Script organization
  - Design principles
  - PowerShell execution from C#
  - Script templates
  - Testing approach
  - Best practices

### Testing Documentation

- [Testing Strategy](./testing/testing_strategy.md): Comprehensive testing approach
  - Testing goals
  - Testing levels (Unit, Integration, System, UAT, Performance)
  - Testing tools and infrastructure
  - Test data management
  - Defect management process
  - Test execution strategy
  - Testing checklist

### Deployment Documentation

- [Deployment Guide](./deployment/deployment_guide.md): Packaging and distribution
  - Prerequisites
  - Application versioning
  - Build process
  - Installer creation
  - Distribution methods
  - Installation process
  - Update mechanisms
  - Uninstallation procedures

## Additional Resources

- [Project Roadmap](../Detailed_Roadmap_Hyper-V_VM_Creation.md): Detailed project timeline and milestones

## Contribution Guidelines

When contributing to the documentation:

1. Follow the established format and style
2. Update the index when adding new documents
3. Maintain cross-references between related documents
4. Use Markdown formatting consistently
5. Include diagrams where appropriate (stored in an `assets` folder)

## Documentation Maintenance

The documentation should be reviewed and updated:

- When requirements change
- When the architecture is modified
- When new features are implemented
- Before each major release
- When issues are found in existing documentation

## Document Status

| Document | Status | Last Updated |
|----------|--------|--------------|
| Project Overview | Complete | [Date] |
| Server Roles | Complete | [Date] |
| Application Architecture | Complete | [Date] |
| Theme Design | Complete | [Date] |
| PowerShell Scripting | Complete | [Date] |
| Testing Strategy | Complete | [Date] |
| Deployment Guide | Complete | [Date] | 
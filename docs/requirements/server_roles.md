# Server Role Requirements

This document outlines the detailed requirements for each server role supported by the Hyper-V VM Creation Application.

## Common Requirements for All Server Roles

### Hardware Configuration
- **CPU**: Configurable, minimum 1 vCPU, recommended 2+ vCPUs
- **RAM**: Configurable, minimum 2GB, recommended based on role
- **Storage**: Configurable, minimum 60GB for OS disk
- **Network**: At least one virtual network adapter

### Base OS
- **OS Options**: 
  - Windows Server 2019 Standard/Datacenter
  - Windows Server 2022 Standard/Datacenter
- **Installation Method**: Unattended installation using Unattend.xml
- **Language/Locale**: Configurable, default to system settings
- **Time Zone**: Configurable, default to system settings

### Common Configuration
- **Computer Name**: User-defined or auto-generated based on role
- **Administrator Password**: User-provided with complexity requirements
- **Windows Updates**: Option to apply updates during installation
- **Firewall Rules**: Role-appropriate firewall configurations

## Domain Controller (DC)

### Specific Requirements
- **Recommended Hardware**: 2+ vCPUs, 4+ GB RAM, 80+ GB storage
- **Domain Information**:
  - Domain Name (FQDN)
  - NetBIOS Name
  - Forest Functional Level (Default: Windows Server 2016)
  - Domain Functional Level (Default: Windows Server 2016)
- **DNS Settings**:
  - Auto-configure DNS for domain
  - Option for configuring forwarders
- **SYSVOL Settings**: Default replication settings

### Post-Installation Configuration
- Install Active Directory Domain Services role
- Configure as first domain controller in a new forest
- Set DNS server to use itself as primary DNS
- Create basic OU structure (optional)
- Configure password policies (optional)

## Remote Desktop Session Host (RDSH)

### Specific Requirements
- **Recommended Hardware**: 4+ vCPUs, 8+ GB RAM, 100+ GB storage
- **RDS Configuration**:
  - License Server (optional)
  - Session Host Settings
  - Collection Name (if applicable)
- **User Access**:
  - User groups that can access RDSH
  - Connection restrictions

### Post-Installation Configuration
- Install Remote Desktop Services role
- Configure user limits and permissions
- Set up RDP certificate (self-signed or provided)
- Configure RemoteApp programs (optional)
- Optimize for RDS workloads

## File Server

### Specific Requirements
- **Recommended Hardware**: 2+ vCPUs, 4+ GB RAM, variable storage based on shares
- **Storage Configuration**:
  - Number and size of data disks
  - RAID/Storage Spaces configuration (if applicable)
  - Drive formatting options (allocation unit size, etc.)
- **Share Configuration**:
  - Share names and paths
  - Share permissions
  - NTFS permissions

### Post-Installation Configuration
- Install File Server role
- Configure disk layout and format data drives
- Create folder structure
- Configure shares with appropriate permissions
- Set up quota management (optional)
- Configure file screening (optional)

## Web Server (IIS)

### Specific Requirements
- **Recommended Hardware**: 2+ vCPUs, 4+ GB RAM, 80+ GB storage
- **IIS Configuration**:
  - Website name(s)
  - Application pools
  - Bindings (ports, host headers)
  - Default documents
- **Feature Selection**:
  - Static content
  - .NET Framework versions
  - CGI/ISAPI
  - Windows Authentication

### Post-Installation Configuration
- Install Web Server (IIS) role
- Configure application pools with appropriate identity
- Set up default website or custom websites
- Configure necessary IIS modules
- Set up logging and monitoring
- Configure SSL certificates (if applicable)

## SQL Server

### Specific Requirements
- **Recommended Hardware**: 4+ vCPUs, 8+ GB RAM, 100+ GB storage with separate data/log disks
- **SQL Configuration**:
  - SQL Server version and edition
  - Instance name
  - Authentication mode (Mixed or Windows)
  - SA password (if Mixed mode)
  - Collation settings
- **Service Accounts**:
  - Database Engine service account
  - Agent service account
- **Database Files Location**:
  - Data files location
  - Log files location
  - Temp DB location

### Post-Installation Configuration
- Install SQL Server with selected components
- Configure memory limits
- Set up database mail (optional)
- Configure backups (optional)
- Set up maintenance plans (optional)

## DHCP Server

### Specific Requirements
- **Recommended Hardware**: 2+ vCPUs, 4+ GB RAM, 80+ GB storage
- **DHCP Configuration**:
  - Scope name
  - Scope range (start/end IP)
  - Subnet mask
  - Lease duration
  - Exclusions
- **DHCP Options**:
  - Default gateway
  - DNS servers
  - WINS servers (if applicable)
  - Other scope options

### Post-Installation Configuration
- Install DHCP Server role
- Configure DHCP server options
- Create and authorize scopes
- Set up reservations (optional)
- Configure failover (optional)

## DNS Server

### Specific Requirements
- **Recommended Hardware**: 2+ vCPUs, 4+ GB RAM, 80+ GB storage
- **DNS Configuration**:
  - Primary zones
  - Reverse lookup zones
  - Zone transfer settings
  - Forwarders
- **Record Types**:
  - A records
  - CNAME records
  - MX records
  - Other record types as needed

### Post-Installation Configuration
- Install DNS Server role
- Configure primary/secondary zones
- Set up conditional forwarders
- Configure zone delegation (if applicable)
- Set up DNS logging
- Configure DNSSEC (optional)

## Custom VM

### Specific Requirements
- **Hardware Configuration**: Fully customizable
- **OS Selection**: Any supported Windows Server version
- **Role/Feature Selection**: User-selected Windows roles and features
- **Additional Software**: Option to install user-provided software packages
- **Custom Scripts**: Option to run user-provided configuration scripts

### Post-Installation Configuration
- Basic OS installation
- Apply user-specified configurations
- Execute custom scripts if provided
- Apply any additional settings 

## SQL Server Role

### Description
The SQL Server role creates a virtual machine with Microsoft SQL Server installed and configured according to best practices. It provides options for various SQL Server editions, features, and storage configurations to optimize database performance.

### Key Features
- Support for SQL Server 2019 and 2022 editions
- Configurable instance names and authentication modes
- Selectable SQL Server features (Database Engine, SSMS, Reporting Services, etc.)
- Optimized storage configuration with separate disks for data, logs, TempDB, and backups
- Network configuration options including firewall rules for SQL Server ports
- Domain join capability for integrated authentication
- Optional maintenance job creation for database maintenance tasks

### Requirements
- Windows Server 2019 or 2022
- Minimum 4 CPU cores recommended
- Minimum 8 GB RAM recommended
- Multiple virtual disks for optimized performance:
  - System disk: 80 GB minimum
  - Data disk: 100 GB minimum
  - Log disk: 50 GB minimum
  - TempDB disk: 50 GB (optional)
  - Backup disk: 100 GB (optional)

### Configuration Options
- SQL Server Edition (Express, Standard, Enterprise, Developer)
- Authentication Mode (Windows or Mixed Mode)
- SQL Features (Database Engine, SSMS, RS, AS, IS)
- Storage configuration for data, logs, TempDB, and backups
- Network settings (IP configuration, firewall rules)
- Domain integration
- SQL maintenance jobs

### Post-Installation Configuration
- Optimized SQL Server memory settings
- Maximum degree of parallelism configuration
- Database maintenance jobs for backups, index maintenance, and integrity checks
- SQL Server Agent configuration
- Firewall rules for SQL Server ports

## Other Roles

[Information about other server roles will be added here] 
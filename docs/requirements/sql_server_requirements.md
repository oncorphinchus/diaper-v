# SQL Server Requirements

## Overview
This document outlines the requirements for creating and configuring a SQL Server using the Hyper-V VM Creation Application.

## VM Requirements

### Hardware Requirements
- **CPU**: Minimum 4 vCPUs recommended
- **Memory**: Minimum 8 GB RAM recommended
- **Storage**:
  - System disk: Minimum 80 GB
  - Data disk: At least one additional VHD for SQL database files (minimum 100 GB)
  - Log disk: At least one additional VHD for SQL log files (minimum 50 GB)
  - TempDB disk: Optional additional VHD for TempDB (minimum 50 GB)
  - Backup disk: Optional additional VHD for backups (minimum 100 GB)

### Software Requirements
- **Operating System**: Windows Server 2019/2022
- **SQL Server Version**: SQL Server 2019/2022 Standard or Enterprise
- **SQL Server Features**: 
  - Database Engine Services (required)
  - SQL Server Replication (optional)
  - Full-Text and Semantic Extractions for Search (optional)
  - Machine Learning Services (optional)
  - Data Quality Services (optional)

## Configuration Parameters

### Basic VM Parameters
- **VM Name**: User-defined name for the virtual machine
- **Generation**: Generation 2 recommended
- **Network**: At least one network adapter connected to a virtual switch

### Storage Parameters
- **Boot Disk Size**: Configurable, minimum 80 GB recommended
- **Data Disks**:
  - Data disk: Drive letter and size (default D:)
  - Log disk: Drive letter and size (default L:)
  - TempDB disk: Drive letter and size (default T:)
  - Backup disk: Drive letter and size (default B:)

### Network Parameters
- **Switch Type**: Virtual switch name
- **VLAN ID**: Optional
- **IP Configuration**:
  - Static IP or DHCP
  - Subnet mask
  - Default gateway
  - DNS servers

### SQL Server Installation Parameters
- **Instance Name**: Default or named instance
- **Installation Media Path**: Path to SQL Server installation ISO or setup files
- **SQL Server Edition**: Standard, Enterprise, Developer, or Express
- **Service Accounts**:
  - SQL Server service account
  - SQL Agent service account
- **Authentication Mode**: Windows Authentication or Mixed Mode
- **SA Password**: For Mixed Mode authentication
- **Data Directory**: Path for database files (default: D:\MSSQL\DATA)
- **Log Directory**: Path for log files (default: L:\MSSQL\LOGS)
- **Backup Directory**: Path for backups (default: B:\MSSQL\BACKUP)
- **TempDB Directory**: Path for TempDB files (default: T:\MSSQL\TEMPDB)
- **Port**: SQL Server port (default: 1433)

### Security Parameters
- **SQL Admin Users**: List of Windows users/groups to add as SQL administrators
- **Firewall Rules**: Enable/disable SQL Server port in firewall
- **Enable TCP/IP**: Enable TCP/IP protocol for SQL Server
- **Remote Connections**: Allow/deny remote connections

### Domain Integration
- **Domain Join**: Option to join the server to a domain
- **Domain Name**: FQDN of the domain to join
- **OU Path**: Optional organizational unit to place the computer account
- **Credentials**: Domain credentials for joining the domain

## Post-Deployment Configuration

### SQL Server Configuration
- Configure memory settings (min/max memory)
- Configure maximum degree of parallelism
- Configure cost threshold for parallelism
- Enable/disable contained databases
- Configure SQL Server Agent

### Database Maintenance
- Create maintenance jobs for:
  - Database backups
  - Index reorganization/rebuilds
  - Statistics updates
  - Database integrity checks

### Performance Settings
- Configure recommended settings for SQL Server performance
- Set up default trace for auditing
- Configure SQL Server for optimized disk I/O

### High Availability (Optional)
- Configure Always On Availability Groups
- Configure database mirroring
- Configure log shipping

## Success Criteria
- VM is successfully created with specified hardware
- Operating system is installed with correct parameters
- SQL Server is installed and operational
- SQL Server databases are configured on the correct drives
- Server is joined to domain (if specified)
- SQL Server is accessible on the network
- SQL Server Agent is running (if installed)
- Database maintenance jobs are created 
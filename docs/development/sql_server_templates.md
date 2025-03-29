# SQL Server Templates

This document provides guidelines for creating and customizing SQL Server templates in the Hyper-V VM Creation Application.

## Template Structure

SQL Server templates follow the standard template format with additional SQL Server-specific configurations:

```json
{
  "TemplateName": "SQL Server 2022 Enterprise",
  "ServerRole": "SQLServer",
  "Description": "Enterprise-grade SQL Server for high-performance workloads",
  "HardwareConfiguration": {
    "ProcessorCount": 8,
    "MemoryGB": 32,
    "StorageGB": 100,
    "Generation": 2,
    "EnableSecureBoot": true
  },
  "NetworkConfiguration": {
    "VirtualSwitch": "Default Switch",
    "DynamicIP": true,
    "IPAddress": "",
    "SubnetMask": "",
    "DefaultGateway": "",
    "DNSServers": ""
  },
  "OSConfiguration": {
    "OSVersion": "Windows Server 2022",
    "TimeZone": 85,
    "AdminPassword": "P@ssw0rd",
    "ComputerName": "SQL-SERVER"
  },
  "SQLServerConfiguration": {
    "SQLVersion": "2022",
    "SQLEdition": "Enterprise",
    "SQLFeatures": "SQLENGINE,SSMS,RS",
    "InstanceName": "MSSQLSERVER",
    "Authentication": "Windows",
    "SAPassword": "",
    "Storage": {
      "DataDiskGB": 300,
      "LogDiskGB": 100,
      "TempDBDiskGB": 100,
      "BackupDiskGB": 300,
      "DataPath": "D:\\SQLData",
      "LogPath": "L:\\SQLLogs",
      "TempDBPath": "T:\\SQLTempDB",
      "BackupPath": "B:\\SQLBackup"
    },
    "Performance": {
      "MaxDOP": 0,
      "CostThreshold": 5,
      "MinMemoryMB": 4096,
      "MaxMemoryMB": 28672
    },
    "Maintenance": {
      "CreateMaintenanceJobs": true,
      "BackupSchedule": "Daily",
      "IntegrityCheckSchedule": "Weekly",
      "IndexMaintenanceSchedule": "Weekly"
    }
  },
  "AdditionalConfiguration": {
    "JoinDomain": false,
    "DomainName": "",
    "OrganizationalUnit": "",
    "DomainUsername": "",
    "DomainPassword": "",
    "AutoStartVM": true,
    "EnableRDP": true
  },
  "Metadata": {
    "Author": "System Administrator",
    "CreatedDate": "2023-03-29T10:00:00",
    "LastModifiedDate": "2023-03-29T10:00:00",
    "Version": "1.0"
  }
}
```

## Default Templates

The application includes the following default SQL Server templates:

1. **SQL Server Developer** - For development and testing environments
2. **SQL Server Express** - For small applications with limited resource requirements
3. **SQL Server Standard** - For medium-sized business applications
4. **SQL Server Enterprise** - For mission-critical applications requiring high performance

## Creating Custom Templates

To create a custom SQL Server template:

1. Navigate to the Template Management section of the application
2. Select "Create New Template"
3. Choose "SQL Server" as the server role
4. Configure the template settings according to your requirements
5. Save the template with a descriptive name

## Template Customization Guide

### Hardware Configuration

- **CPU**: Choose based on workload requirements. OLTP workloads benefit from faster CPUs, while data warehousing benefits from more cores.
- **Memory**: SQL Server performs best with ample memory for buffer pool. Allocate at least 8 GB for production servers.
- **Storage**: System drive should be at least 80 GB. Consider separate disks for user databases, TempDB, and transaction logs.

### SQL Server Configuration

- **Edition**: Choose based on feature requirements and licensing considerations:
  - **Developer**: Full-featured for non-production use
  - **Express**: Free edition with database size limitations
  - **Standard**: Mid-range edition for departmental applications
  - **Enterprise**: Full-featured edition for mission-critical workloads

- **Authentication Mode**:
  - **Windows Authentication**: More secure, recommended for domain environments
  - **Mixed Mode**: Required for applications that need SQL authentication

- **Features**:
  - **Database Engine**: Required component
  - **SSMS**: Management tools
  - **Reporting Services**: For report hosting
  - **Analysis Services**: For OLAP workloads
  - **Integration Services**: For ETL processes

### Storage Configuration

For optimal performance, configure multiple disks:

- **Data Disk**: Stores user database files (.mdf)
- **Log Disk**: Stores transaction log files (.ldf)
- **TempDB Disk**: Dedicated disk for TempDB
- **Backup Disk**: Separate disk for backups

### Performance Settings

- **MaxDOP**: Set based on CPU core count (0 for auto-configuration)
- **Cost Threshold for Parallelism**: 5-50 depending on workload (default is 5)
- **Min/Max Memory**: Configure to leave some memory for OS (about 4-8 GB for Windows)

### Maintenance Jobs

Enable maintenance jobs to automatically:
- Create database backups on a schedule
- Check database integrity
- Rebuild or reorganize indexes
- Update statistics

## Best Practices

1. **Memory Configuration**: Configure maximum memory to leave at least 4 GB for the OS.
2. **TempDB Configuration**: Create one TempDB data file per processor core (up to 8).
3. **Disk Configuration**: Use separate disks for data, logs, TempDB, and backups.
4. **Instant File Initialization**: Enable to improve data file operations.
5. **Network Configuration**: Configure firewall rules to allow SQL Server traffic (port 1433 by default).
6. **Maintenance**: Create maintenance jobs for backups, index maintenance, and integrity checks.

## Testing Templates

After creating a template, it's recommended to test it by:

1. Creating a VM using the template
2. Verifying SQL Server installation and configuration
3. Testing basic database operations
4. Validating that all specified features are installed and working correctly
5. Checking that maintenance jobs are created as specified 
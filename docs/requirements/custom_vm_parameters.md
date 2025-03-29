# Custom VM Parameters

This document lists the custom parameters needed for creating virtual machines with specific server roles.

## Common Parameters

These parameters are common to all VM types:

| Parameter | Description | Type | Default Value | Required |
|-----------|-------------|------|---------------|----------|
| VMName | Name of the virtual machine | String | - | Yes |
| CPUCount | Number of virtual CPUs | Integer | 2 | Yes |
| MemoryGB | Amount of RAM in GB | Integer | 4 | Yes |
| StorageGB | Size of the system disk in GB | Integer | 60 | Yes |
| VirtualSwitch | Name of the virtual switch to connect to | String | "Default Switch" | Yes |
| Generation | VM generation (1 or 2) | Integer | 2 | Yes |
| EnableSecureBoot | Whether to enable Secure Boot (Gen 2 only) | Boolean | true | No |
| DynamicMemory | Whether to enable dynamic memory | Boolean | true | No |
| EnableNestedVirtualization | Whether to enable nested virtualization | Boolean | false | No |

## SQL Server Parameters

| Parameter | Description | Type | Default Value | Required |
|-----------|-------------|------|---------------|----------|
| SQLEdition | SQL Server edition | String | "Developer" | Yes |
| SQLFeatures | SQL Server features to install | String | "SQLENGINE,SSMS" | Yes |
| InstanceName | SQL Server instance name | String | "MSSQLSERVER" | No |
| EnableMixedMode | Enable mixed authentication mode | Boolean | false | No |
| SAPassword | SA account password (Required if EnableMixedMode is true) | SecureString | - | Conditional |
| SQLServiceAccount | Service account for SQL Server | String | "NT Service\MSSQLSERVER" | No |
| SQLServicePassword | Password for service account | SecureString | - | Conditional |
| SQLSysAdminAccounts | List of accounts to add as sysadmin | String[] | @("BUILTIN\Administrators") | No |
| SQLDataDiskGB | Size of SQL data disk in GB | Integer | 100 | No |
| SQLLogDiskGB | Size of SQL log disk in GB | Integer | 50 | No |
| SQLTempDBDiskGB | Size of SQL TempDB disk in GB | Integer | 50 | No |
| SQLBackupDiskGB | Size of SQL backup disk in GB | Integer | 100 | No |
| SQLDataPath | Path for SQL data files | String | "D:\SQLData" | No |
| SQLLogPath | Path for SQL log files | String | "L:\SQLLogs" | No |
| SQLTempDBPath | Path for SQL TempDB files | String | "T:\SQLTempDB" | No |
| SQLBackupPath | Path for SQL backups | String | "B:\SQLBackup" | No |
| CreateMaintenanceJobs | Create SQL maintenance jobs | Boolean | true | No |
| MaxDOP | Maximum Degree of Parallelism | Integer | 0 | No |
| CostThreshold | Cost Threshold for Parallelism | Integer | 5 | No |
| MinMemoryMB | Minimum SQL Server memory (MB) | Integer | 1024 | No |
| MaxMemoryMB | Maximum SQL Server memory (MB) | Integer | 0 (auto) | No |

## Domain Controller Parameters

[Domain Controller parameters will be added here]

## File Server Parameters

[File Server parameters will be added here]

## Web Server Parameters

[Web Server parameters will be added here]

## DHCP Server Parameters

[DHCP Server parameters will be added here]

## DNS Server Parameters

[DNS Server parameters will be added here]

## RDSH Parameters

[RDSH parameters will be added here] 
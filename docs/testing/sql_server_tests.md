# SQL Server Testing Plan

This document outlines the test cases for SQL Server VM creation functionality.

## Test Environment

- **Development System**: Windows 10/11 Pro or Enterprise with Hyper-V enabled
- **Target OS for VMs**: Windows Server 2019/2022
- **SQL Server Versions**: SQL Server 2019/2022 (Developer/Standard/Enterprise editions)
- **Virtual Switch**: Default Switch

## Test Cases

### 1. Basic SQL Server Installation

#### Test ID: SQL-BASIC-001
**Description:** Create a basic SQL Server VM with default settings  
**Prerequisites:** 
- Windows Server ISO available
- SQL Server installation media available
- Hyper-V enabled with Default Switch

**Steps:**
1. Open the Hyper-V VM Creation Application
2. Select "SQL Server" role
3. Use default values for all settings
4. Click "Create VM"

**Expected Results:**
- VM should be created successfully
- Windows Server should be installed
- SQL Server should be installed with Database Engine and SSMS
- SQL Server service should be running
- Windows Firewall rules for SQL Server should be configured

### 2. Custom SQL Server Configuration

#### Test ID: SQL-CUSTOM-001
**Description:** Create a SQL Server VM with custom settings  
**Prerequisites:** Same as SQL-BASIC-001

**Steps:**
1. Open the Hyper-V VM Creation Application
2. Select "SQL Server" role
3. Configure custom settings:
   - CPU: 4 cores
   - Memory: 16 GB
   - System Disk: 100 GB
   - SQL Edition: Enterprise
   - Authentication: Mixed Mode
   - Features: Database Engine, SSMS, Reporting Services
   - Data Disk: 200 GB
   - Log Disk: 100 GB
   - TempDB Disk: 50 GB
   - Backup Disk: 200 GB
4. Click "Create VM"

**Expected Results:**
- VM should be created with specified hardware (4 cores, 16 GB RAM)
- SQL Server Enterprise should be installed with specified features
- Mixed Mode authentication should be enabled
- Additional disks should be created and mounted
- SQL Server directories should be configured on the appropriate disks

### 3. Domain Join with SQL Server

#### Test ID: SQL-DOMAIN-001
**Description:** Create a SQL Server VM joined to a domain  
**Prerequisites:** 
- Same as SQL-BASIC-001
- Domain Controller VM already running

**Steps:**
1. Open the Hyper-V VM Creation Application
2. Select "SQL Server" role
3. Configure basic settings as needed
4. Enable "Join Domain" option
5. Enter domain name and credentials
6. Click "Create VM"

**Expected Results:**
- VM should be created successfully
- VM should be joined to the specified domain
- SQL Server should be installed and configured
- SQL Server should be accessible using domain credentials

### 4. SQL Server with Maintenance Jobs

#### Test ID: SQL-MAINT-001
**Description:** Create a SQL Server VM with maintenance jobs  
**Prerequisites:** Same as SQL-BASIC-001

**Steps:**
1. Open the Hyper-V VM Creation Application
2. Select "SQL Server" role
3. Configure basic settings as needed
4. Enable "Create Maintenance Jobs" option
5. Click "Create VM"

**Expected Results:**
- VM should be created successfully
- SQL Server should be installed and configured
- SQL Server Agent should be running
- Maintenance jobs should be created:
  - Database backup job
  - Index maintenance job
  - DBCC integrity check job
  - Statistics update job

### 5. SQL Server Template Test

#### Test ID: SQL-TEMPL-001
**Description:** Create a SQL Server VM using a saved template  
**Prerequisites:** 
- Same as SQL-BASIC-001
- SQL Server template already created

**Steps:**
1. Open the Hyper-V VM Creation Application
2. Navigate to Templates section
3. Select an existing SQL Server template
4. Click "Create VM from Template"

**Expected Results:**
- VM should be created based on template settings
- SQL Server should be installed and configured according to template
- All settings from the template should be applied correctly

## Performance Tests

### 1. SQL Server Creation Time

#### Test ID: SQL-PERF-001
**Description:** Measure the time taken to create a SQL Server VM  
**Prerequisites:** Same as SQL-BASIC-001

**Steps:**
1. Start a timer
2. Create a SQL Server VM using default settings
3. Stop the timer when VM creation is complete

**Expected Results:**
- Total creation time should be within acceptable limits
- Progress reporting should accurately reflect creation status

### 2. SQL Server Performance Verification

#### Test ID: SQL-PERF-002
**Description:** Verify that the created SQL Server performs according to specifications  
**Prerequisites:** SQL Server VM created with Test ID SQL-BASIC-001 or SQL-CUSTOM-001

**Steps:**
1. Connect to the SQL Server VM
2. Run performance benchmarks:
   - Run DBCC CHECKDB on newly created database
   - Create and query a test table with 1 million rows
   - Execute backup and restore operations

**Expected Results:**
- Operations should complete successfully
- Performance should be appropriate for the allocated resources

## Error Handling Tests

### 1. Invalid SQL Server Configuration

#### Test ID: SQL-ERR-001
**Description:** Attempt to create a SQL Server VM with invalid configuration  
**Prerequisites:** Same as SQL-BASIC-001

**Steps:**
1. Open the Hyper-V VM Creation Application
2. Select "SQL Server" role
3. Enter invalid configuration:
   - Memory: 1 GB (too low for SQL Server)
   - Mixed Mode Authentication without SA password
4. Click "Create VM"

**Expected Results:**
- Application should validate input and prevent VM creation
- Error messages should clearly indicate the issues
- No resources should be created or allocated

### 2. Failed SQL Server Installation

#### Test ID: SQL-ERR-002
**Description:** Simulate SQL Server installation failure  
**Prerequisites:** Same as SQL-BASIC-001

**Steps:**
1. Open the Hyper-V VM Creation Application
2. Select "SQL Server" role
3. Configure settings normally
4. Modify the SQL Server installation path to an invalid location
5. Click "Create VM"

**Expected Results:**
- VM creation should proceed until SQL Server installation
- Application should detect installation failure
- Error should be reported to the user
- Detailed logs should include specific error information
- Option to retry installation or cleanup should be provided

## Test Data

Sample VM configurations for testing:

### Basic SQL Server
- **VM Name**: TEST-SQL-BASIC
- **CPU**: 2
- **Memory**: 8 GB
- **System Disk**: 80 GB
- **SQL Edition**: Developer
- **Features**: Database Engine, SSMS

### Advanced SQL Server
- **VM Name**: TEST-SQL-ADV
- **CPU**: 4
- **Memory**: 16 GB
- **System Disk**: 100 GB
- **SQL Edition**: Enterprise
- **Features**: Database Engine, SSMS, RS, AS
- **Data Disk**: 200 GB
- **Log Disk**: 100 GB
- **TempDB Disk**: 50 GB
- **Backup Disk**: 200 GB

## Test Results Reporting

For each test case, record the following information:

1. Test ID and description
2. Test date and time
3. Test environment details
4. Steps performed
5. Actual results
6. Test status (Pass/Fail)
7. If failed, detailed error information
8. Screenshots or logs as appropriate
9. Tester notes and observations

## Regression Testing

Regression tests should be performed after any significant changes to:

1. PowerShell scripts for SQL Server configuration
2. VM creation workflow
3. SQL Server configuration forms
4. Underlying Hyper-V integration logic 
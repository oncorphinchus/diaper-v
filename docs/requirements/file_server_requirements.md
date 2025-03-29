# File Server Requirements

## Overview
This document outlines the requirements for creating and configuring a File Server using the Hyper-V VM Creation Application.

## VM Requirements

### Hardware Requirements
- **CPU**: Minimum 2 vCPUs recommended
- **Memory**: Minimum 4 GB RAM recommended
- **Storage**:
  - System disk: Minimum 60 GB
  - Data disks: At least one additional VHD for file shares (configurable size)

### Software Requirements
- **Operating System**: Windows Server 2019/2022
- **Roles**: File and Storage Services
- **Features**: File Server Resource Manager (optional)

## Configuration Parameters

### Basic VM Parameters
- **VM Name**: User-defined name for the virtual machine
- **Generation**: Generation 2 recommended
- **Network**: At least one network adapter connected to a virtual switch

### Storage Parameters
- **Boot Disk Size**: Configurable, minimum 60 GB recommended
- **Data Disks**: 
  - Number of disks: Configurable (1-64)
  - Size per disk: Configurable
  - Storage location: Configurable

### Network Parameters
- **Switch Type**: Virtual switch name
- **VLAN ID**: Optional
- **IP Configuration**:
  - Static IP or DHCP
  - Subnet mask
  - Default gateway
  - DNS servers

### File Server Parameters
- **Share Names**: List of shares to create
- **Share Paths**: Location for each share
- **Share Permissions**: NTFS and Share permissions for each share
- **Quota Management**: Optional quotas for shares
- **Access-Based Enumeration**: Enable/disable ABE for shares
- **DFS Integration**: Optional DFS namespace configuration

### Domain Integration
- **Domain Join**: Option to join the server to a domain
- **Domain Name**: FQDN of the domain to join
- **OU Path**: Optional organizational unit to place the computer account
- **Credentials**: Domain credentials for joining the domain

## Post-Deployment Configuration

### File Server Configuration
- Install File Server role
- Configure disk partitioning for data disks
- Set up shares with proper permissions
- Configure File Server Resource Manager (if selected)
- Set up quotas and file screening (if selected)

### Performance Settings
- Configure recommended settings for file server performance
- Optimize disk allocation unit size for file shares
- Enable opportunistic locking for file shares

### Backup Configuration
- Configure Volume Shadow Copy Service
- Set up shadow copies for shared volumes
- Configure shadow copy storage and schedule

## Success Criteria
- VM is successfully created with specified hardware
- Operating system is installed with correct parameters
- File Server role is installed and operational
- All specified shares are created with correct permissions
- Server is joined to domain (if specified)
- Data disks are properly formatted and mounted
- File server is accessible on the network 
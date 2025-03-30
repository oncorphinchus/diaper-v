# Domain Controller Configuration

This document outlines the requirements and parameters for creating a Domain Controller virtual machine using the Hyper-V Creator application.

## Overview

A Domain Controller in Windows Server is a server that responds to security authentication requests within a Windows domain. It is responsible for managing user accounts, computer accounts, and other domain resources, and is a critical component of Active Directory Domain Services (AD DS).

## Hardware Requirements

| Resource | Minimum | Recommended |
|----------|---------|-------------|
| CPU      | 2 cores | 4 cores     |
| Memory   | 2 GB    | 4-8 GB      |
| Storage  | 60 GB   | 100 GB      |
| Network  | 1 NIC   | 1 NIC       |

## Configuration Parameters

### Basic VM Settings

| Parameter     | Description                                      | Default     |
|---------------|--------------------------------------------------|-------------|
| VM Name       | The name of the virtual machine                  | DC01        |
| CPU Cores     | Number of virtual CPUs assigned to the VM        | 2           |
| Memory (GB)   | Amount of RAM in GB assigned to the VM           | 4           |
| Virtual Switch | The Hyper-V virtual switch to connect the VM     | Default Switch |

### Domain Settings

| Parameter              | Description                                       | Default       |
|------------------------|---------------------------------------------------|---------------|
| Domain Name (FQDN)     | Fully qualified domain name for the new domain   | contoso.local |
| NetBIOS Name           | NetBIOS name for the domain (pre-Windows 2000)   | CONTOSO       |
| DSRM Password          | Directory Services Restore Mode administrator password | *(required)* |
| Forest Functional Level | The functional level for the new forest         | Windows Server 2022 |
| Domain Functional Level | The functional level for the new domain         | Windows Server 2022 |

### DNS Settings

| Parameter                | Description                               | Default   |
|--------------------------|-------------------------------------------|-----------|
| Configure DNS Server     | Whether to install and configure DNS Server | Yes       |
| DNS Forwarders           | IP addresses for DNS forwarding           | 8.8.8.8, 8.8.4.4 |
| Create Reverse Lookup Zone | Whether to create a reverse lookup zone    | Yes       |

### Network Configuration

| Parameter          | Description                            | Default       |
|--------------------|----------------------------------------|---------------|
| IP Configuration   | Whether to use DHCP or static IP       | Static IP     |
| IP Address         | Static IP address for the server       | 192.168.1.10  |
| Subnet Mask        | Subnet mask for the network            | 255.255.255.0 |
| Default Gateway    | Default gateway address                | 192.168.1.1   |
| Preferred DNS Server | Preferred DNS server address         | 127.0.0.1     |

### Advanced Options

| Parameter                   | Description                                | Default |
|-----------------------------|--------------------------------------------|---------|
| Create Basic OU Structure   | Create default organizational units        | Yes     |
| Configure Default Group Policies | Configure basic group policy objects  | Yes     |
| Install RSAT Tools          | Install Remote Server Administration Tools | Yes     |

## Deployment Considerations

### Pre-requisites
- Ensure the Hyper-V host has sufficient resources
- Validate that the IP address (if using static IP) is available on the network
- Ensure the domain name does not conflict with existing domains in the network

### Post-Deployment Tasks
- Configure additional domain controllers for redundancy
- Implement backup and recovery solutions
- Set up monitoring for the domain controller
- Configure additional security settings as required by organizational policies

## Best Practices

1. **DNS Configuration**
   - Always configure the DNS server on a domain controller
   - Use multiple DNS forwarders for redundancy

2. **Security Best Practices**
   - Use a strong and complex password for the Directory Services Restore Mode
   - Implement the principle of least privilege
   - Enable and configure auditing

3. **Networking**
   - Always use a static IP address for domain controllers
   - Configure the domain controller to use itself as the primary DNS server

4. **Redundancy**
   - For production environments, deploy at least two domain controllers for redundancy
   - Place domain controllers in different physical locations if possible

## Template Customization

The Domain Controller template can be customized through the following mechanisms:

1. **Pre-deployment PowerShell scripts**
   - Scripts executed before the VM deployment process begins
   - Can be used to validate prerequisites or prepare the environment

2. **Post-deployment PowerShell scripts**
   - Scripts executed after the VM is created but before Active Directory is installed
   - Can be used to configure additional settings or install additional roles

3. **Domain Configuration Scripts**
   - Scripts executed after Active Directory is installed
   - Can be used to create additional OUs, users, or configure advanced domain settings

## Default Organizational Units

When selecting the option to create a basic OU structure, the following OUs will be created:

- Users
- Computers
- Groups
- Service Accounts
- Servers
- Administrative Units 
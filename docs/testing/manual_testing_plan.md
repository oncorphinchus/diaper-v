# Manual Testing Plan for Custom VM Creation

## Overview
This document outlines the manual testing procedures for the Custom VM creation functionality in the Hyper-V Creator application. These tests should be performed before releasing the application to ensure all features work as expected.

## Prerequisites
- Windows 10 or 11 with Hyper-V enabled
- Administrator privileges
- Sufficient disk space (at least 20GB free)
- ISO file for Windows Server (for testing OS installation)

## Test Cases

### 1. Basic Navigation
| Test ID | Test Description | Expected Result |
|---------|-----------------|----------------|
| NAV-01 | Navigate to Custom VM from main menu | Custom VM configuration form opens |
| NAV-02 | Navigate to Custom VM from left sidebar | Custom VM configuration form opens |

### 2. VM Creation with Minimal Settings
| Test ID | Test Description | Expected Result |
|---------|-----------------|----------------|
| CVM-01 | Create a basic VM with only VM name specified | VM is created successfully with default settings |
| CVM-02 | Create a VM with name, custom CPU, and memory settings | VM is created with specified CPU and memory values |
| CVM-03 | Create a VM with dynamic IP configuration | VM is created with network adapter set to DHCP |

### 3. Advanced VM Creation Settings
| Test ID | Test Description | Expected Result |
|---------|-----------------|----------------|
| CVM-10 | Create a VM with Static IP configuration | VM is created with specified IP settings |
| CVM-11 | Create a VM with additional disk | VM is created with system disk and additional data disk |
| CVM-12 | Create a VM with Generation 1 settings | Generation 1 VM is created successfully |
| CVM-13 | Create a VM with Generation 2 settings | Generation 2 VM is created successfully |
| CVM-14 | Create a VM with SecureBoot disabled | VM is created with SecureBoot disabled |
| CVM-15 | Create a VM with ISO attached | VM is created with specified ISO mounted |

### 4. Template Management
| Test ID | Test Description | Expected Result |
|---------|-----------------|----------------|
| TPL-01 | Load default template | Form is populated with default values |
| TPL-02 | Load custom template | Form is populated with values from selected template |
| TPL-03 | Load high-performance template | Form is populated with high-performance values |

### 5. Error Handling
| Test ID | Test Description | Expected Result |
|---------|-----------------|----------------|
| ERR-01 | Try to create VM without name | Create button is disabled |
| ERR-02 | Try to create VM with invalid IP address | Error message is displayed |
| ERR-03 | Try to create VM with non-existent virtual switch | Error message is displayed |
| ERR-04 | Cancel VM creation during process | Creation is canceled and form returns to input state |

### 6. Progress Monitoring
| Test ID | Test Description | Expected Result |
|---------|-----------------|----------------|
| PRG-01 | Monitor creation progress | Progress bar updates correctly during VM creation |
| PRG-02 | View detailed logs | Log display shows detailed steps during creation |
| PRG-03 | Check final status message | Success message shown after completion |

## Test Procedure
1. Launch the Hyper-V Creator application
2. Navigate to the Custom VM configuration screen
3. Enter the required parameters as specified in the test case
4. Click the "Create VM" button
5. Monitor progress and wait for completion
6. Verify the VM was created correctly in Hyper-V Manager
7. Document any issues encountered

## Test Environment
- Operating System: Windows 10/11
- Hardware: 16GB RAM, 4 CPU cores minimum
- Software: Hyper-V enabled, Hyper-V Creator application

## Reporting
For each test case, document:
- Pass/Fail status
- Any errors encountered
- Screenshots of issues (if applicable)
- Steps to reproduce any failures

Submit the completed test results to the development team for review. 
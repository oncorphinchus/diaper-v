# Domain Controller Testing Guide

This document outlines the testing procedures for validating the Domain Controller configuration functionality in the Hyper-V Creator application.

## Test Environment Requirements

- Windows 10/11 Pro or Enterprise, or Windows Server with Hyper-V role enabled
- At least 8GB RAM available for testing
- At least 100GB of free disk space
- Network connectivity between the host and VMs

## Test Preparation

1. Ensure Hyper-V is properly configured on the test system
2. Create a virtual switch in Hyper-V for testing (if not already available)
3. Backup any important VMs or data before testing
4. Have a Windows Server ISO available for testing

## Test Cases

### TC-DC-001: Basic UI Validation

**Objective**: Verify that the Domain Controller configuration form loads correctly and displays all required fields.

**Steps**:
1. Launch the Hyper-V Creator application
2. Navigate to the Server Roles selection screen
3. Select "Domain Controller" from the available roles
4. Verify the Domain Controller configuration form loads

**Expected Results**:
- Form loads without errors
- All fields are visible and properly aligned
- Default values are pre-populated where appropriate
- Form is responsive and scrollable if needed

### TC-DC-002: Field Validation

**Objective**: Verify that input validation works correctly for all fields.

**Steps**:
1. Launch the Domain Controller configuration form
2. Test each field with valid and invalid inputs:
   - VM Name: Empty, special characters, very long name
   - Domain Name: Empty, invalid format (no dot), very long name
   - NetBIOS Name: Empty, invalid characters, name longer than 15 characters
   - IP Address (when Static IP is selected): Empty, invalid format, out-of-range values

**Expected Results**:
- Invalid inputs are rejected or highlighted
- Error messages are displayed for invalid inputs
- Submit button is disabled when required fields are invalid
- Valid inputs are accepted without errors

### TC-DC-003: Domain Controller VM Creation with Default Values

**Objective**: Verify that a Domain Controller VM can be created using default values.

**Steps**:
1. Launch the Domain Controller configuration form
2. Keep all default values
3. Click "Create VM" button
4. Monitor the VM creation process

**Expected Results**:
- VM creation starts without errors
- Progress is displayed to the user
- VM is created in Hyper-V with the correct name and configuration
- Deployment script is generated with correct parameters
- VM boots and completes initial configuration

### TC-DC-004: Domain Controller VM Creation with Custom Values

**Objective**: Verify that a Domain Controller VM can be created using custom values.

**Steps**:
1. Launch the Domain Controller configuration form
2. Set custom values for:
   - VM Name: "TestDC01"
   - CPU: 4 cores
   - Memory: 8 GB
   - Domain Name: "test.local"
   - NetBIOS Name: "TEST"
   - Static IP configuration
   - Custom OU structure settings
3. Click "Create VM" button

**Expected Results**:
- VM is created with the specified custom values
- All custom parameters are correctly passed to the deployment script
- VM deploys successfully with the custom configuration

### TC-DC-005: Network Configuration Validation

**Objective**: Verify that network configuration options work correctly.

**Steps**:
1. Launch the Domain Controller configuration form
2. Select "Static IP" radio button
3. Enter valid IP address, subnet mask, gateway, and DNS server
4. Create the VM
5. Repeat the test with "DHCP" option selected

**Expected Results**:
- For Static IP: VM is configured with the specified IP address and network settings
- For DHCP: VM is configured to obtain an IP address automatically
- Network connectivity works in both cases

### TC-DC-006: DNS Server Configuration

**Objective**: Verify that DNS Server role installation and configuration work correctly.

**Steps**:
1. Launch the Domain Controller configuration form
2. Check "Install and configure DNS Server"
3. Enter DNS forwarders
4. Check "Create reverse lookup zone"
5. Create the VM

**Expected Results**:
- DNS Server role is installed on the VM
- DNS forwarders are configured correctly
- Reverse lookup zone is created if selected
- DNS resolution works correctly

### TC-DC-007: Domain Creation Validation

**Objective**: Verify that Active Directory Domain Services is properly installed and the domain is created.

**Steps**:
1. Create a Domain Controller VM with specified domain parameters
2. Wait for deployment to complete
3. Connect to the VM and check domain status

**Expected Results**:
- Active Directory Domain Services is installed
- Domain is created with the specified name
- Forest and domain functional levels are set correctly
- SYSVOL and NETLOGON shares are created and accessible

### TC-DC-008: OU Structure Creation

**Objective**: Verify that the organizational unit structure is created correctly.

**Steps**:
1. Launch the Domain Controller configuration form
2. Check "Create default organizational units"
3. Create the VM
4. Connect to the VM and open Active Directory Users and Computers

**Expected Results**:
- All specified OUs are created in the domain
- OU structure matches the expected hierarchy

### TC-DC-009: Group Policy Configuration

**Objective**: Verify that default Group Policies are configured correctly.

**Steps**:
1. Launch the Domain Controller configuration form
2. Check "Configure GPOs"
3. Create the VM
4. Connect to the VM and open Group Policy Management

**Expected Results**:
- Default Domain Policy is configured with specified password policy settings
- Any additional GPOs defined in the template are created and linked properly

### TC-DC-010: Cancellation and Navigation

**Objective**: Verify that cancellation and navigation work correctly.

**Steps**:
1. Launch the Domain Controller configuration form
2. Fill in some fields
3. Click "Cancel" button
4. Navigate back to the Domain Controller configuration form

**Expected Results**:
- Form is closed when Cancel is clicked
- No VM is created
- When reopening the form, default values are displayed (not the previously entered values)

## Performance Testing

### TP-DC-001: VM Creation Time

**Objective**: Measure the time it takes to create a Domain Controller VM.

**Steps**:
1. Start a timer
2. Launch the Domain Controller configuration form
3. Use default values
4. Click "Create VM" button
5. Stop the timer when the VM is created and the deployment script is injected

**Expected Results**:
- VM creation (excluding AD DS installation) should complete within 5 minutes
- The process should not hang or freeze the application

### TP-DC-002: Memory Usage

**Objective**: Verify that the application's memory usage is reasonable during VM creation.

**Steps**:
1. Monitor the application's memory usage during VM creation
2. Note any significant spikes

**Expected Results**:
- Memory usage should remain stable
- No memory leaks should be observed

## Error Handling Testing

### TE-DC-001: Network Connectivity Error

**Objective**: Verify that the application handles network connectivity issues gracefully.

**Steps**:
1. Disable network connectivity on the host
2. Attempt to create a Domain Controller VM
3. Observe the error handling

**Expected Results**:
- Application displays appropriate error messages
- No crash or hang occurs
- User is given guidance on how to resolve the issue

### TE-DC-002: Insufficient Resources Error

**Objective**: Verify that the application handles insufficient resources gracefully.

**Steps**:
1. Configure the host to have limited available resources
2. Attempt to create a Domain Controller VM with resource requirements exceeding availability
3. Observe the error handling

**Expected Results**:
- Application checks resource availability before attempting VM creation
- Clear error message is displayed if resources are insufficient
- Suggestions for resolving the issue are provided

## Integration Testing

### TI-DC-001: Multiple Domain Controllers

**Objective**: Verify that multiple Domain Controllers can be created and work together.

**Steps**:
1. Create a primary Domain Controller
2. Create a second Domain Controller in the same domain
3. Verify replication and functionality

**Expected Results**:
- Both Domain Controllers function correctly
- AD replication works between the controllers
- DNS service works correctly on both controllers

### TI-DC-002: Domain Controller and Member Server

**Objective**: Verify interaction between a Domain Controller and a member server.

**Steps**:
1. Create a Domain Controller
2. Create a member server and join it to the domain
3. Test domain functionality

**Expected Results**:
- Member server joins the domain successfully
- Group policies apply correctly to the member server
- Domain authentication works correctly

## Security Testing

### TS-DC-001: Password Policy Enforcement

**Objective**: Verify that the configured password policies are enforced.

**Steps**:
1. Create a Domain Controller with specific password policy settings
2. Create a user account and attempt to set passwords that violate the policy

**Expected Results**:
- Weak passwords are rejected
- Password history policy is enforced
- Account lockout policy is enforced after failed attempts

### TS-DC-002: DSRM Password Security

**Objective**: Verify that the Directory Services Restore Mode password is handled securely.

**Steps**:
1. Enter a DSRM password in the configuration form
2. Examine how the password is stored and transmitted to the VM

**Expected Results**:
- Password is not stored in plain text
- Password is securely transmitted to the VM
- Password is not logged in plain text

## Reporting

For each test case, document:
1. Test case ID and name
2. Pass/Fail status
3. Date and time of testing
4. Tester name
5. Any observations or issues
6. Screenshots of failures or unexpected behavior

## Issue Tracking

Any issues found during testing should be:
1. Documented with detailed steps to reproduce
2. Categorized by severity (Critical, High, Medium, Low)
3. Assigned to a developer for resolution
4. Retested after resolution

## Regression Testing

After fixing issues:
1. Rerun the specific test case that uncovered the issue
2. Run related test cases that might be affected
3. Run a subset of core functionality tests to ensure no new issues were introduced 
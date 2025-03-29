**Project: Hyper-V VM Creation Application (Windows Executable)**

**Goal:** Develop a user-friendly Windows application that allows users to easily create pre-configured Hyper-V VMs for specific server roles, minimizing manual configuration.

**Target Audience:** System administrators, developers, IT professionals.

**Key Features:**

* **Role-Based VM Creation:**  
  * Create pre-configured VMs for common server roles with minimal user input.  
  * Support for:  
    * Domain Controller (DC)  
    * Remote Desktop Session Host (RDSH)  
    * File Server  
    * Web Server (IIS)  
    * SQL Server  
    * DHCP Server  
    * DNS Server  
  * Option to create a "Custom" VM with full parameter control.  
* **Simplified Configuration:**  
  * Automate OS installation and initial server configuration.  
  * Use Unattend.xml for Windows Server OS installation.  
  * Use PowerShell DSC or CM tools (Ansible) for post-OS configuration.  
  * Minimize the need for users to interact with Windows Server interfaces.  
* **Template Management:**  
  * Store and manage VM templates (VHDX, ISO, configuration files).  
  * Provide a library of pre-built templates for supported server roles.  
  * Allow users to create and customize their own templates.  
* **User-Friendly Interface:**  
  * Intuitive UI for selecting server roles and providing necessary parameters.  
  * Clear progress monitoring and feedback during VM creation.  
  * Error handling and logging.  
  * **Multiple Themes:**  
    * Support for different visual themes.  
    * Include themes like "Classic," "Sanrio," and "Dark."  
    * Allow users to switch between themes.  
* **Network and Storage Configuration:**  
  * Automate virtual network adapter configuration.  
  * Automate virtual hard disk creation and assignment.  
  * Support for static/dynamic IP address assignment.  
* **PowerShell Automation:**  
  * Utilize PowerShell for all Hyper-V and OS configuration tasks.  
  * Abstract the complexity of PowerShell from the user.

**Technology Stack:**

* **UI:** .NET (C\#) with Windows Presentation Foundation (WPF).  
* **Hyper-V Automation:** PowerShell (executed from C\#).  
* **Data Storage:** Local configuration files (JSON, XML) or a simple SQLite database for storing VM templates and settings.  
* **Configuration Management:** PowerShell Desired State Configuration (DSC).  
* **Theming:** WPF Styles and Resource Dictionaries.

**Roadmap:**

**Phase 1: Planning and Design (4-5 Weeks)**

1. **Requirement Gathering (1-2 Weeks):**  
   * Define the application's features and functionalities.  
   * Identify the target audience and their needs.  
   * Determine the required VM parameters for each server role.  
   * Plan for template management, including default templates.  
   * Plan for error handling, logging, and security.  
   * Detailed planning for unattended OS installs for each server role.  
   * **Theme Planning:**  
     * Research and select target themes (Classic, Sanrio, Dark).  
     * Gather visual assets and design specifications for each theme.  
     * Plan how to structure XAML resources for theme switching.  
2. **UI Design (1-2 Weeks):**  
   * Create wireframes and mockups of the application's UI.  
   * Design the main window with options for role-based or custom VM creation.  
   * Design role-specific forms with pre-defined settings and minimal input fields.  
   * Plan for template selection, progress monitoring, and error display.  
   * Choose a visually appealing and user-friendly design.  
   * **Theme Design:**  
     * Design UI elements (buttons, forms, etc.) for each theme.  
     * Create style guides and resource dictionaries for each theme.  
3. **Architecture Design (1 Week):**  
   * Define the application's architecture and component interactions.  
   * Plan how to execute PowerShell scripts from C\#.  
   * Design the data storage mechanism for templates and settings.  
   * Plan for error handling, logging, and security.  
   * Design modular PowerShell scripts.  
   * **Theming Architecture:**  
     * Plan how to load and switch between theme resource dictionaries.  
     * Decide on a theme selection mechanism (settings menu, etc.).

**Phase 2: Development (10-14 Weeks)**

1. **Core Functionality (2-3 Weeks):**  
   * Set up the .NET project (WPF).  
   * Implement PowerShell script execution from C\#.  
   * Develop the main application window and navigation.  
   * Implement the "Custom" VM creation option with full parameter control.  
   * Implement input validation.  
2. **Role-Based VM Creation (4-6 Weeks):**  
   * Develop role-specific forms for each supported server role (DC, RDSH, etc.).  
   * Create PowerShell scripts for each role, automating OS installation and configuration.  
   * Implement logic to select the correct template and execute the appropriate scripts.  
   * Implement unattended OS installations.  
3. **Template Management (2-3 Weeks):**  
   * Implement data storage for VM templates (JSON, XML, or SQLite).  
   * Develop UI for template creation, editing, and selection.  
   * Integrate template selection into the VM creation process.  
   * Create default templates for each supported server role.  
4. **Theming (2-3 Weeks):**  
   * **Implement theme switching:**  
     * Create XAML resource dictionaries for each theme (styles, colors, etc.).  
     * Write C\# code to load and merge the appropriate resource dictionary at runtime.  
     * Create a settings section in the UI to allow users to select a theme.  
   * **Apply themes to UI:**  
     * Style all UI elements using the defined theme resources.  
     * Ensure consistent look and feel across the application.  
5. **Advanced Features (2-3 Weeks):**  
   * Implement network and storage configuration.  
   * Implement progress monitoring and feedback during VM creation.  
   * Implement error handling and logging.  
   * Implement any additional features.

**Phase 3: Testing and Deployment (3-4 Weeks)**

1. **Unit Testing (1-2 Weeks):**  
   * Write unit tests for C\# code and PowerShell scripts.  
   * Test individual components and functions.  
   * Automated testing.  
2. **Integration Testing (1-2 Weeks):**  
   * Test the application's workflow from UI to PowerShell execution.  
   * Test role-based VM creation for each supported role.  
   * Test VM creation with different parameters and templates.  
   * Test error handling and recovery mechanisms.  
   * **Test themes:**  
     * Test that all UI elements are styled correctly in each theme.  
     * Test theme switching functionality.  
3. **User Acceptance Testing (UAT) (1 Week):**  
   * Involve target users in testing the application.  
   * Gather feedback and make necessary adjustments.  
   * **Gather theme feedback:**  
     * Ask users for their preferences on the themes.  
4. **Deployment (1 Week):**  
   * Create an installer for the application (e.g., MSI using WiX Toolset).  
   * Package the application and its dependencies.  
   * Document the installation process.  
   * Deploy the application.
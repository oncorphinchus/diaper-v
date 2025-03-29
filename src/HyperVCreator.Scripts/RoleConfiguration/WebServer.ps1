<#
.SYNOPSIS
    Configures a Hyper-V VM as a Web Server (IIS).

.DESCRIPTION
    This script configures a Windows Server VM as a Web Server (IIS),
    including installing the IIS role, configuring websites, and setting up
    application pools.

.PARAMETER VMName
    Name of the VM to configure.

.PARAMETER Websites
    Array of website configurations. Each website should have Name, PhysicalPath, 
    Port, and ApplicationPool properties.

.PARAMETER ApplicationPools
    Array of application pool configurations. Each pool should have Name and RuntimeVersion properties.

.PARAMETER EnableWindowsAuth
    Whether to enable Windows Authentication. Default is $false.

.PARAMETER EnableBasicAuth
    Whether to enable Basic Authentication. Default is $false.

.PARAMETER DefaultWebsitePort
    Port for the default website. Default is 80.

.PARAMETER JoinDomain
    Whether to join this server to a domain. Default is $false.

.PARAMETER DomainName
    Domain name to join when JoinDomain is $true.

.PARAMETER DomainCredential
    Credential object for domain join when JoinDomain is $true.

.PARAMETER InstallWebManagementService
    Whether to install Web Management Service. Default is $true.

.PARAMETER InstallURLRewrite
    Whether to install URL Rewrite module. Default is $true.

.PARAMETER InstallARR
    Whether to install Application Request Routing. Default is $false.

.NOTES
    File Name      : WebServer.ps1
    Author         : HyperV Creator Team
    Prerequisite   : PowerShell 5.1 or later
#>

# Source common functions
. "$PSScriptRoot\..\Common\ErrorHandling.ps1"
. "$PSScriptRoot\..\Common\Logging.ps1"
. "$PSScriptRoot\..\Common\Validation.ps1"
. "$PSScriptRoot\..\Monitoring\TrackProgress.ps1"

function Install-WebServer {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$true)]
        [string]$VMName,
        
        [Parameter(Mandatory=$false)]
        [PSObject[]]$Websites = @(),
        
        [Parameter(Mandatory=$false)]
        [PSObject[]]$ApplicationPools = @(),
        
        [Parameter(Mandatory=$false)]
        [bool]$EnableWindowsAuth = $false,
        
        [Parameter(Mandatory=$false)]
        [bool]$EnableBasicAuth = $false,
        
        [Parameter(Mandatory=$false)]
        [int]$DefaultWebsitePort = 80,
        
        [Parameter(Mandatory=$false)]
        [bool]$JoinDomain = $false,
        
        [Parameter(Mandatory=$false)]
        [string]$DomainName = "",
        
        [Parameter(Mandatory=$false)]
        [PSCredential]$DomainCredential = $null,
        
        [Parameter(Mandatory=$false)]
        [bool]$InstallWebManagementService = $true,
        
        [Parameter(Mandatory=$false)]
        [bool]$InstallURLRewrite = $true,
        
        [Parameter(Mandatory=$false)]
        [bool]$InstallARR = $false
    )
    
    begin {
        Write-LogMessage -Level Info -Message "Starting Web Server (IIS) configuration for VM: $VMName"
        $totalSteps = 4
        if ($JoinDomain) { $totalSteps++ }
        
        Start-TrackingOperation -OperationName "ConfigureWebServer" -TotalSteps $totalSteps
        $currentStep = 1
    }
    
    process {
        try {
            # Step 1: Ensure VM is running
            Update-OperationProgress -StepNumber $currentStep -StepDescription "Verifying VM is running"
            $vm = Get-VM -Name $VMName -ErrorAction Stop
            if ($vm.State -ne 'Running') {
                Write-LogMessage -Level Warning -Message "VM $VMName is not running. Starting VM..."
                Start-VM -Name $VMName -ErrorAction Stop
                # Wait for VM to start
                $timeout = 300
                $timer = [Diagnostics.Stopwatch]::StartNew()
                while ($vm.State -ne 'Running' -and $timer.Elapsed.TotalSeconds -lt $timeout) {
                    Start-Sleep -Seconds 5
                    $vm = Get-VM -Name $VMName -ErrorAction Stop
                }
                if ($vm.State -ne 'Running') {
                    throw "Failed to start VM $VMName within timeout period."
                }
            }
            $currentStep++
            
            # Step 2: Join domain if specified
            if ($JoinDomain) {
                Update-OperationProgress -StepNumber $currentStep -StepDescription "Joining domain: $DomainName"
                if ([string]::IsNullOrWhiteSpace($DomainName)) {
                    throw "Domain name is required when JoinDomain is true"
                }
                if ($null -eq $DomainCredential) {
                    throw "Domain credentials are required when JoinDomain is true"
                }
                
                $joinDomainScript = {
                    param ($DomainName, $Credential)
                    
                    try {
                        Add-Computer -DomainName $DomainName -Credential $Credential -Restart
                        return @{
                            Success = $true
                            Message = "Domain join initiated successfully. Server will restart."
                        }
                    }
                    catch {
                        return @{
                            Success = $false
                            Message = "Failed to join domain: $_"
                        }
                    }
                }
                
                $session = New-PSSession -VMName $VMName -Credential $AdminCredential
                $joinResult = Invoke-Command -Session $session -ScriptBlock $joinDomainScript -ArgumentList $DomainName, $DomainCredential
                
                if (-not $joinResult.Success) {
                    throw $joinResult.Message
                }
                
                # Wait for VM to restart after domain join
                Write-LogMessage -Level Info -Message "Waiting for VM to restart after domain join..."
                Start-Sleep -Seconds 60
                Remove-PSSession -Session $session -ErrorAction SilentlyContinue
                
                # Wait for VM to be available again
                $vmAvailable = $false
                $waitAttempts = 0
                $maxWaitAttempts = 20
                
                while (-not $vmAvailable -and $waitAttempts -lt $maxWaitAttempts) {
                    try {
                        $newSession = New-PSSession -VMName $VMName -Credential $AdminCredential -ErrorAction Stop
                        $vmAvailable = $true
                        Remove-PSSession -Session $newSession -ErrorAction SilentlyContinue
                    }
                    catch {
                        Write-LogMessage -Level Warning -Message "VM not yet available after domain join. Waiting..."
                        Start-Sleep -Seconds 15
                        $waitAttempts++
                    }
                }
                
                if (-not $vmAvailable) {
                    throw "VM did not become available after domain join within the timeout period"
                }
                
                $currentStep++
            }
            
            # Step 3: Install IIS role
            Update-OperationProgress -StepNumber $currentStep -StepDescription "Installing IIS role and features"
            
            $installIISScript = {
                param (
                    $EnableWindowsAuth,
                    $EnableBasicAuth,
                    $InstallWebManagementService,
                    $InstallURLRewrite,
                    $InstallARR
                )
                
                # Base features to install
                $features = @(
                    "Web-Server",                      # Base IIS feature
                    "Web-WebServer",                   # Web Server
                    "Web-Common-Http",                 # Common HTTP features
                    "Web-Default-Doc",                 # Default Document
                    "Web-Dir-Browsing",                # Directory Browsing
                    "Web-Http-Errors",                 # HTTP Errors
                    "Web-Static-Content",              # Static Content
                    "Web-Http-Redirect",               # HTTP Redirection
                    "Web-Health",                      # Health and Diagnostics
                    "Web-Http-Logging",                # HTTP Logging
                    "Web-Custom-Logging",              # Custom Logging
                    "Web-Log-Libraries",               # Logging Tools
                    "Web-Request-Monitor",             # Request Monitor
                    "Web-Http-Tracing",                # Tracing
                    "Web-Performance",                 # Performance
                    "Web-Stat-Compression",            # Static Content Compression
                    "Web-Dyn-Compression",             # Dynamic Content Compression
                    "Web-Security",                    # Security
                    "Web-Filtering",                   # Request Filtering
                    "Web-App-Dev",                     # Application Development
                    "Web-Net-Ext45",                   # .NET Extensibility 4.5
                    "Web-Asp-Net45",                   # ASP.NET 4.5
                    "Web-ISAPI-Ext",                   # ISAPI Extensions
                    "Web-ISAPI-Filter",                # ISAPI Filters
                    "Web-Mgmt-Tools",                  # Management Tools
                    "Web-Mgmt-Console",                # IIS Management Console
                    "Web-Scripting-Tools",             # IIS Management Scripts and Tools
                    "Web-Mgmt-Compat"                  # Management Service
                )
                
                # Add Windows Authentication if requested
                if ($EnableWindowsAuth) {
                    $features += "Web-Windows-Auth"
                }
                
                # Add Basic Authentication if requested
                if ($EnableBasicAuth) {
                    $features += "Web-Basic-Auth"
                }
                
                # Add Web Management Service if requested
                if ($InstallWebManagementService) {
                    $features += "Web-Mgmt-Service"
                }
                
                try {
                    # Install IIS and selected features
                    $installResult = Install-WindowsFeature -Name $features -IncludeManagementTools
                    
                    $result = @{
                        Success = $installResult.Success
                        RestartNeeded = ($installResult.RestartNeeded -eq "Yes")
                        InstalledFeatures = ($installResult.FeatureResult | Where-Object { $_.Success -eq $true }).Name -join ", "
                        Message = if ($installResult.Success) { "IIS role and features installed successfully" } else { "Failed to install IIS role" }
                    }
                    
                    # Install URL Rewrite Module if requested (requires WebPI)
                    if ($InstallURLRewrite -or $InstallARR) {
                        # First, check if WebPI is installed
                        $webPIPath = "C:\Program Files\Microsoft\Web Platform Installer\WebpiCmd.exe"
                        if (-not (Test-Path $webPIPath)) {
                            # Download Web Platform Installer
                            $webpiInstallerUrl = "https://go.microsoft.com/fwlink/?LinkId=287166"
                            $webpiInstallerPath = "C:\Temp\WebPlatformInstaller_x64.msi"
                            
                            # Create temp directory if it doesn't exist
                            if (-not (Test-Path "C:\Temp")) {
                                New-Item -Path "C:\Temp" -ItemType Directory | Out-Null
                            }
                            
                            # Download WebPI
                            try {
                                Invoke-WebRequest -Uri $webpiInstallerUrl -OutFile $webpiInstallerPath
                                
                                # Install WebPI
                                Start-Process -FilePath "msiexec.exe" -ArgumentList "/i `"$webpiInstallerPath`" /quiet /norestart" -Wait
                            }
                            catch {
                                $result.Message += ". Failed to download Web Platform Installer: $_"
                                return $result
                            }
                        }
                        
                        # Now install URL Rewrite if requested
                        if ($InstallURLRewrite) {
                            try {
                                Start-Process -FilePath $webPIPath -ArgumentList "/Install /Products:UrlRewrite2 /AcceptEULA /SuppressReboot" -Wait
                                $result.Message += ". URL Rewrite module installed."
                            }
                            catch {
                                $result.Message += ". Failed to install URL Rewrite: $_"
                            }
                        }
                        
                        # Install ARR if requested
                        if ($InstallARR) {
                            try {
                                Start-Process -FilePath $webPIPath -ArgumentList "/Install /Products:ARR /AcceptEULA /SuppressReboot" -Wait
                                $result.Message += ". Application Request Routing installed."
                            }
                            catch {
                                $result.Message += ". Failed to install Application Request Routing: $_"
                            }
                        }
                    }
                    
                    return $result
                }
                catch {
                    return @{
                        Success = $false
                        RestartNeeded = $false
                        InstalledFeatures = ""
                        Message = "Exception during IIS installation: $_"
                    }
                }
            }
            
            $session = New-PSSession -VMName $VMName -Credential $AdminCredential
            $installResult = Invoke-Command -Session $session -ScriptBlock $installIISScript -ArgumentList $EnableWindowsAuth, $EnableBasicAuth, $InstallWebManagementService, $InstallURLRewrite, $InstallARR
            
            if (-not $installResult.Success) {
                throw $installResult.Message
            }
            
            Write-LogMessage -Level Info -Message $installResult.Message
            
            # Restart if needed
            if ($installResult.RestartNeeded) {
                Write-LogMessage -Level Info -Message "Restarting VM after IIS installation..."
                Remove-PSSession -Session $session -ErrorAction SilentlyContinue
                Restart-VM -Name $VMName -Force
                
                # Wait for VM to be available again
                Start-Sleep -Seconds 60
                $vmAvailable = $false
                $waitAttempts = 0
                $maxWaitAttempts = 20
                
                while (-not $vmAvailable -and $waitAttempts -lt $maxWaitAttempts) {
                    try {
                        $newSession = New-PSSession -VMName $VMName -Credential $AdminCredential -ErrorAction Stop
                        $vmAvailable = $true
                        Remove-PSSession -Session $newSession -ErrorAction SilentlyContinue
                    }
                    catch {
                        Write-LogMessage -Level Warning -Message "VM not yet available after restart. Waiting..."
                        Start-Sleep -Seconds 15
                        $waitAttempts++
                    }
                }
                
                if (-not $vmAvailable) {
                    throw "VM did not become available after restart within the timeout period"
                }
            }
            
            $currentStep++
            
            # Step 4: Configure Default Website
            Update-OperationProgress -StepNumber $currentStep -StepDescription "Configuring default website"
            
            $configureDefaultWebsiteScript = {
                param ($DefaultWebsitePort)
                
                try {
                    # Import WebAdministration module
                    Import-Module WebAdministration
                    
                    # Configure default website port
                    Set-ItemProperty "IIS:\Sites\Default Web Site" -Name bindings -Value @{protocol="http";bindingInformation="*:$($DefaultWebsitePort):"}
                    
                    # Create a simple test page
                    $defaultWebRoot = Get-ItemProperty "IIS:\Sites\Default Web Site" -Name physicalPath
                    $testPagePath = Join-Path -Path $defaultWebRoot -ChildPath "test.html"
                    
                    Set-Content -Path $testPagePath -Value @"
<!DOCTYPE html>
<html>
<head>
    <title>IIS Test Page</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 0; padding: 20px; }
        h1 { color: #0066cc; }
        .container { max-width: 800px; margin: 0 auto; }
        .info { background-color: #f8f8f8; border: 1px solid #ddd; padding: 15px; border-radius: 5px; }
    </style>
</head>
<body>
    <div class="container">
        <h1>IIS Server is running successfully!</h1>
        <div class="info">
            <p>This is a test page automatically created during server setup.</p>
            <p>Server name: $env:COMPUTERNAME</p>
            <p>Date: $(Get-Date)</p>
        </div>
    </div>
</body>
</html>
"@
                    
                    return @{
                        Success = $true
                        Message = "Default website configured successfully on port $DefaultWebsitePort"
                    }
                }
                catch {
                    return @{
                        Success = $false
                        Message = "Failed to configure default website: $_"
                    }
                }
            }
            
            $session = New-PSSession -VMName $VMName -Credential $AdminCredential
            $defaultWebsiteResult = Invoke-Command -Session $session -ScriptBlock $configureDefaultWebsiteScript -ArgumentList $DefaultWebsitePort
            
            if (-not $defaultWebsiteResult.Success) {
                throw $defaultWebsiteResult.Message
            }
            
            Write-LogMessage -Level Info -Message $defaultWebsiteResult.Message
            
            $currentStep++
            
            # Step 5: Create Application Pools and Websites
            Update-OperationProgress -StepNumber $currentStep -StepDescription "Creating application pools and websites"
            
            if ($ApplicationPools.Count -gt 0 -or $Websites.Count -gt 0) {
                $configureWebsitesScript = {
                    param ($ApplicationPools, $Websites)
                    
                    # Import WebAdministration module
                    Import-Module WebAdministration
                    
                    $results = @{
                        Success = $true
                        Message = "Application pools and websites configured successfully"
                        ApplicationPools = @()
                        Websites = @()
                    }
                    
                    # Create application pools
                    foreach ($pool in $ApplicationPools) {
                        try {
                            # Check if application pool already exists
                            if (Test-Path "IIS:\AppPools\$($pool.Name)") {
                                $results.ApplicationPools += @{
                                    Name = $pool.Name
                                    Success = $true
                                    Message = "Application pool '$($pool.Name)' already exists"
                                }
                                continue
                            }
                            
                            # Create the application pool
                            $appPool = New-WebAppPool -Name $pool.Name
                            
                            # Set runtime version if specified
                            if ($pool.RuntimeVersion) {
                                Set-ItemProperty "IIS:\AppPools\$($pool.Name)" -Name managedRuntimeVersion -Value $pool.RuntimeVersion
                            }
                            
                            $results.ApplicationPools += @{
                                Name = $pool.Name
                                Success = $true
                                Message = "Application pool '$($pool.Name)' created successfully"
                            }
                        }
                        catch {
                            $results.ApplicationPools += @{
                                Name = $pool.Name
                                Success = $false
                                Message = "Failed to create application pool '$($pool.Name)': $_"
                            }
                            $results.Success = $false
                        }
                    }
                    
                    # Create websites
                    foreach ($site in $Websites) {
                        try {
                            # Check if website already exists
                            if (Test-Path "IIS:\Sites\$($site.Name)") {
                                $results.Websites += @{
                                    Name = $site.Name
                                    Success = $true
                                    Message = "Website '$($site.Name)' already exists"
                                }
                                continue
                            }
                            
                            # Create the physical path if it doesn't exist
                            if (-not (Test-Path $site.PhysicalPath)) {
                                New-Item -Path $site.PhysicalPath -ItemType Directory -Force | Out-Null
                                
                                # Create a simple default page
                                $defaultPagePath = Join-Path -Path $site.PhysicalPath -ChildPath "index.html"
                                Set-Content -Path $defaultPagePath -Value @"
<!DOCTYPE html>
<html>
<head>
    <title>$($site.Name) - IIS Website</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 0; padding: 20px; }
        h1 { color: #0066cc; }
        .container { max-width: 800px; margin: 0 auto; }
        .info { background-color: #f8f8f8; border: 1px solid #ddd; padding: 15px; border-radius: 5px; }
    </style>
</head>
<body>
    <div class="container">
        <h1>$($site.Name) is running successfully!</h1>
        <div class="info">
            <p>This is a default page automatically created during website setup.</p>
            <p>Server name: $env:COMPUTERNAME</p>
            <p>Website name: $($site.Name)</p>
            <p>Date: $(Get-Date)</p>
        </div>
    </div>
</body>
</html>
"@
                            }
                            
                            # Create the website
                            $port = if ($site.Port) { $site.Port } else { 80 }
                            $binding = @{
                                protocol = "http"
                                bindingInformation = "*:$($port):"
                            }
                            
                            $newSite = New-Website -Name $site.Name -PhysicalPath $site.PhysicalPath -ApplicationPool $site.ApplicationPool -Bindings $binding
                            
                            $results.Websites += @{
                                Name = $site.Name
                                Success = $true
                                Port = $port
                                Message = "Website '$($site.Name)' created successfully on port $port"
                            }
                        }
                        catch {
                            $results.Websites += @{
                                Name = $site.Name
                                Success = $false
                                Message = "Failed to create website '$($site.Name)': $_"
                            }
                            $results.Success = $false
                        }
                    }
                    
                    return $results
                }
                
                $session = New-PSSession -VMName $VMName -Credential $AdminCredential
                $websitesResult = Invoke-Command -Session $session -ScriptBlock $configureWebsitesScript -ArgumentList $ApplicationPools, $Websites
                
                # Log application pool results
                foreach ($pool in $websitesResult.ApplicationPools) {
                    if ($pool.Success) {
                        Write-LogMessage -Level Info -Message $pool.Message
                    }
                    else {
                        Write-LogMessage -Level Warning -Message $pool.Message
                    }
                }
                
                # Log website results
                foreach ($site in $websitesResult.Websites) {
                    if ($site.Success) {
                        Write-LogMessage -Level Info -Message $site.Message
                    }
                    else {
                        Write-LogMessage -Level Warning -Message $site.Message
                    }
                }
                
                if (-not $websitesResult.Success) {
                    Write-LogMessage -Level Warning -Message "There were issues during website configuration. Check the logs for details."
                }
            }
            
            Update-OperationProgress -StepNumber $currentStep -StepDescription "Web Server configuration complete" -Completed $true
            Write-LogMessage -Level Info -Message "Web Server configuration completed successfully for VM: $VMName"
        }
        catch {
            $errorMessage = "Failed to configure Web Server: $_"
            Write-LogMessage -Level Error -Message $errorMessage
            throw $errorMessage
        }
        finally {
            # Clean up PSSession if it exists
            if ($session) {
                Remove-PSSession -Session $session -ErrorAction SilentlyContinue
            }
            if ($newSession) {
                Remove-PSSession -Session $newSession -ErrorAction SilentlyContinue
            }
        }
    }
    
    end {
        Complete-TrackingOperation -OperationName "ConfigureWebServer"
    }
}

Export-ModuleMember -Function Install-WebServer 
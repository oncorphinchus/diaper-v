using System;
using System.Threading.Tasks;
using HyperVCreator.Core.Models;
using HyperVCreator.Core.Services;
using HyperVCreator.Core.PowerShell;

namespace HyperVCreator.App
{
    /// <summary>
    /// Simple console application for testing core functionality
    /// </summary>
    public class TestConsole
    {
        private readonly Core.PowerShell.PowerShellService _powerShellService;
        private readonly TemplateService _templateService;
        private readonly VMCreationService _vmCreationService;
        
        public TestConsole()
        {
            _powerShellService = new Core.PowerShell.PowerShellService();
            _templateService = new TemplateService();
            _vmCreationService = new VMCreationService();
        }
        
        /// <summary>
        /// Entry point for the test console
        /// </summary>
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== HyperV Creator Test Console ===");
            Console.WriteLine($"Started at: {DateTime.Now}");
            
            var testConsole = new TestConsole();
            await testConsole.Run();
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        
        /// <summary>
        /// Run the test console
        /// </summary>
        private async Task Run()
        {
            try
            {
                // First check if Hyper-V is enabled
                await CheckHyperVStatus();
                
                bool exit = false;
                
                while (!exit)
                {
                    Console.Clear();
                    Console.WriteLine("=== HyperV Creator Test Console ===");
                    Console.WriteLine("1. Run PowerShell Test");
                    Console.WriteLine("2. Create Test VM");
                    Console.WriteLine("3. List Available Templates");
                    Console.WriteLine("4. Run Test Harness");
                    Console.WriteLine("5. Exit");
                    Console.Write("\nEnter choice: ");
                    
                    var choice = Console.ReadLine();
                    
                    switch (choice)
                    {
                        case "1":
                            await RunPowerShellTest();
                            break;
                        case "2":
                            await CreateTestVM();
                            break;
                        case "3":
                            ListTemplates();
                            break;
                        case "4":
                            await RunTestHarness();
                            break;
                        case "5":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("Invalid choice, please try again.");
                            break;
                    }
                    
                    if (!exit)
                    {
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Run a simple PowerShell test
        /// </summary>
        private async Task RunPowerShellTest()
        {
            Console.WriteLine("\nRunning PowerShell test...");
            
            try
            {
                _powerShellService.OutputReceived += (s, e) => Console.WriteLine($"Output: {e}");
                _powerShellService.ErrorReceived += (s, e) => Console.WriteLine($"Error: {e}");
                
                var results = await _powerShellService.ExecuteScriptAsync("Get-Process | Select-Object -First 5");
                
                Console.WriteLine("\nResults:");
                foreach (var result in results)
                {
                    Console.WriteLine($"- {result}");
                }
                
                Console.WriteLine("\nPowerShell test completed successfully.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"PowerShell test failed: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Create a test VM
        /// </summary>
        private async Task CreateTestVM()
        {
            Console.WriteLine("\nCreating test VM...");
            
            try
            {
                // Get a template
                var template = _templateService.GetTemplateByName("Default Custom VM");
                
                if (template == null)
                {
                    Console.WriteLine("Default template not found, creating a new one...");
                    template = new VMTemplate
                    {
                        TemplateName = "Test VM",
                        ServerRole = "CustomVM",
                        Description = "Test VM for console testing",
                        HardwareConfiguration = new HardwareConfig
                        {
                            ProcessorCount = 2,
                            MemoryGB = 4,
                            StorageGB = 80
                        },
                        OSConfiguration = new OSConfig
                        {
                            OSVersion = "Windows Server 2022 Standard",
                            ComputerName = "TestVM"
                        }
                    };
                }
                
                var progress = new Progress<VMCreationProgress>(p =>
                {
                    Console.WriteLine($"Progress: {p.PercentComplete}% - {p.StatusMessage}");
                });
                
                Console.WriteLine($"Creating VM using template: {template.TemplateName}");
                var result = await _vmCreationService.CreateVMAsync(template, progress);
                
                if (result.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("VM creation successful!");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"VM creation failed: {result.Message}");
                    if (result.HasErrors)
                    {
                        foreach (var error in result.ErrorMessages)
                        {
                            Console.WriteLine($"- {error}");
                        }
                    }
                    Console.ResetColor();
                }
                
                Console.WriteLine($"Operation completed in {result.Duration.TotalSeconds:F2} seconds.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"VM creation failed: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// List available templates
        /// </summary>
        private void ListTemplates()
        {
            Console.WriteLine("\nListing available templates...");
            
            try
            {
                var templates = _templateService.GetAllTemplates();
                
                if (templates.Count == 0)
                {
                    Console.WriteLine("No templates found.");
                    return;
                }
                
                Console.WriteLine($"Found {templates.Count} templates:\n");
                
                foreach (var template in templates)
                {
                    Console.WriteLine($"- {template.TemplateName} ({template.ServerRole})");
                    Console.WriteLine($"  Description: {template.Description}");
                    Console.WriteLine($"  Hardware: {template.HardwareConfiguration.ProcessorCount} CPUs, " +
                                      $"{template.HardwareConfiguration.MemoryGB} GB RAM, " +
                                      $"{template.HardwareConfiguration.StorageGB} GB Storage");
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error listing templates: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Run the test harness
        /// </summary>
        private async Task RunTestHarness()
        {
            Console.WriteLine("\nRunning test harness...");
            
            try
            {
                var testHarness = new TestHarness();
                var results = await testHarness.RunAllTests();
                testHarness.DisplayResults();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error running test harness: {ex.Message}");
                Console.ResetColor();
            }
        }
        
        /// <summary>
        /// Checks if Hyper-V is enabled on the system
        /// </summary>
        private async Task CheckHyperVStatus()
        {
            Console.WriteLine("Checking Hyper-V status...");
            
            try
            {
                // Create a HyperVService and check if Hyper-V is enabled
                var hyperVService = new Core.Services.HyperVService(_powerShellService);
                var isEnabled = await hyperVService.CheckHyperVEnabled();
                
                if (isEnabled)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Hyper-V is enabled and available.");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("WARNING: Hyper-V appears to be disabled or not properly configured.");
                    Console.WriteLine("Some functionality may not work correctly.");
                    Console.WriteLine("\nTo enable Hyper-V, run the following in an elevated PowerShell:");
                    Console.WriteLine("  Enable-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V -All");
                    Console.WriteLine("\nAfter enabling Hyper-V, restart your computer.");
                    Console.ResetColor();
                    
                    Console.WriteLine("\nPress any key to continue anyway...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error checking Hyper-V status: {ex.Message}");
                Console.WriteLine("This may indicate that Hyper-V is not properly installed.");
                Console.ResetColor();
                
                Console.WriteLine("\nPress any key to continue anyway...");
                Console.ReadKey();
            }
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HyperVCreator.Core.Models;
using HyperVCreator.Core.Services;
using CorePowerShell = HyperVCreator.Core.PowerShell;

namespace HyperVCreator.App
{
    /// <summary>
    /// Test harness for verifying application components work correctly
    /// </summary>
    public class TestHarness
    {
        // Services
        private readonly CorePowerShell.PowerShellService _powerShellService;
        private readonly Core.Services.HyperVService _hyperVService;
        private readonly Core.Services.ConfigurationService _configurationService;
        private readonly TemplateService _templateService;
        private readonly ThemeService _themeService;
        
        // Results
        private readonly List<TestResult> _testResults = new List<TestResult>();
        
        public TestHarness()
        {
            try
            {
                // Initialize services
                _powerShellService = new CorePowerShell.PowerShellService();
                _hyperVService = new Core.Services.HyperVService(_powerShellService);
                _configurationService = new Core.Services.ConfigurationService(_powerShellService);
                _templateService = new TemplateService();
                _themeService = new ThemeService();
                
                // PowerShell service doesn't have events anymore, we'll handle output in the results
                Console.WriteLine("PowerShell service initialized.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR initializing test harness: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                throw;
            }
        }
        
        /// <summary>
        /// Run all tests to check if components are functioning correctly
        /// </summary>
        public async Task<List<TestResult>> RunAllTests()
        {
            Console.WriteLine("Starting test run...");
            Console.WriteLine($"Test run started at: {DateTime.Now}");
            Console.WriteLine("==========================================");
            
            try
            {
                await TestPowerShellService();
                Console.WriteLine("PowerShell service tests completed.");
                
                await TestHyperVService();
                Console.WriteLine("Hyper-V service tests completed.");
                
                await TestConfigurationService();
                Console.WriteLine("Configuration service tests completed.");
                
                TestTemplateService();
                Console.WriteLine("Template service tests completed.");
                
                TestThemeService();
                Console.WriteLine("Theme service tests completed.");
                
                // Use the specialized template service tests
                Console.WriteLine("Running specialized template service tests...");
                var templateServiceTests = new TestTemplateService();
                _testResults.AddRange(templateServiceTests.RunTests());
                Console.WriteLine("Specialized template service tests completed.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR during test execution: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                
                _testResults.Add(new TestResult
                {
                    ComponentName = "TestHarness",
                    TestName = "Test Execution",
                    Success = false,
                    Message = $"Exception during test execution: {ex.Message}",
                    Exception = ex
                });
            }
            
            Console.WriteLine("==========================================");
            Console.WriteLine($"Test run completed at: {DateTime.Now}");
            Console.WriteLine($"Total tests: {_testResults.Count}");
            Console.WriteLine($"Passed: {_testResults.Count(r => r.Success)}");
            Console.WriteLine($"Failed: {_testResults.Count(r => !r.Success)}");
            
            return _testResults;
        }
        
        /// <summary>
        /// Test PowerShell service functionality
        /// </summary>
        private async Task TestPowerShellService()
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                var results = await _powerShellService.ExecuteScriptAsync("Get-Process | Select-Object -First 1");
                sw.Stop();
                
                bool success = results != null && results.Count > 0;
                _testResults.Add(new TestResult
                {
                    ComponentName = "PowerShellService",
                    TestName = "Basic Script Execution",
                    Success = success,
                    Message = success ? $"Executed PowerShell script in {sw.ElapsedMilliseconds}ms" 
                        : "Failed to execute PowerShell script",
                    Results = results
                });
            }
            catch (Exception ex)
            {
                _testResults.Add(new TestResult
                {
                    ComponentName = "PowerShellService",
                    TestName = "Basic Script Execution",
                    Success = false,
                    Message = $"Exception: {ex.Message}",
                    Exception = ex
                });
            }
        }
        
        /// <summary>
        /// Test HyperV service functionality
        /// </summary>
        private async Task TestHyperVService()
        {
            try
            {
                var result = await _hyperVService.CheckHyperVEnabled();
                
                _testResults.Add(new TestResult
                {
                    ComponentName = "HyperVService",
                    TestName = "Check Hyper-V Enabled",
                    Success = true,
                    Message = $"Hyper-V enabled: {result}"
                });
            }
            catch (Exception ex)
            {
                _testResults.Add(new TestResult
                {
                    ComponentName = "HyperVService",
                    TestName = "Check Hyper-V Enabled",
                    Success = false,
                    Message = $"Exception: {ex.Message}",
                    Exception = ex
                });
            }
        }
        
        /// <summary>
        /// Test configuration service functionality
        /// </summary>
        private async Task TestConfigurationService()
        {
            try
            {
                var result = await _configurationService.ConfigureDomainController("TestVM", "contoso.local", "CONTOSO", "P@ssw0rd");
                
                _testResults.Add(new TestResult
                {
                    ComponentName = "ConfigurationService",
                    TestName = "Domain Controller Configuration",
                    Success = result,
                    Message = result ? "Domain Controller configuration test passed" 
                        : "Domain Controller configuration test failed"
                });
            }
            catch (Exception ex)
            {
                _testResults.Add(new TestResult
                {
                    ComponentName = "ConfigurationService",
                    TestName = "Domain Controller Configuration",
                    Success = false,
                    Message = $"Exception: {ex.Message}",
                    Exception = ex
                });
            }
        }
        
        /// <summary>
        /// Test template service functionality
        /// </summary>
        private void TestTemplateService()
        {
            try
            {
                var templates = _templateService.GetAllTemplates();
                bool success = templates != null && templates.Count > 0;
                
                _testResults.Add(new TestResult
                {
                    ComponentName = "TemplateService",
                    TestName = "Get All Templates",
                    Success = success,
                    Message = success ? $"Retrieved {templates.Count} templates" : "No templates retrieved"
                });
                
                // Test template retrieval by name
                var dcTemplate = _templateService.GetTemplateByName("Default Domain Controller");
                bool templateFound = dcTemplate != null;
                
                _testResults.Add(new TestResult
                {
                    ComponentName = "TemplateService",
                    TestName = "Get Template By Name",
                    Success = templateFound,
                    Message = templateFound ? $"Retrieved Domain Controller template" : "Domain Controller template not found"
                });
            }
            catch (Exception ex)
            {
                _testResults.Add(new TestResult
                {
                    ComponentName = "TemplateService",
                    TestName = "Template Tests",
                    Success = false,
                    Message = $"Exception: {ex.Message}",
                    Exception = ex
                });
            }
        }
        
        /// <summary>
        /// Test theme service functionality
        /// </summary>
        private void TestThemeService()
        {
            try
            {
                string currentTheme = _themeService.CurrentTheme;
                var availableThemes = _themeService.AvailableThemes;
                
                bool success = !string.IsNullOrEmpty(currentTheme) && availableThemes != null && availableThemes.Count > 0;
                
                _testResults.Add(new TestResult
                {
                    ComponentName = "ThemeService",
                    TestName = "Theme Properties",
                    Success = success,
                    Message = success ? $"Current theme: {currentTheme}, Available themes: {string.Join(", ", availableThemes)}" 
                        : "Theme properties test failed"
                });
                
                // Test theme change
                bool themeChanged = false;
                _themeService.ThemeChanged += (s, e) => themeChanged = true;
                
                try
                {
                    string newTheme = availableThemes.FirstOrDefault(t => t != currentTheme) ?? availableThemes.First();
                    _themeService.SetTheme(newTheme);
                    
                    _testResults.Add(new TestResult
                    {
                        ComponentName = "ThemeService",
                        TestName = "Theme Change",
                        Success = themeChanged,
                        Message = themeChanged ? $"Theme changed to {newTheme}" : "Theme change event not fired"
                    });
                }
                catch (Exception ex)
                {
                    _testResults.Add(new TestResult
                    {
                        ComponentName = "ThemeService",
                        TestName = "Theme Change",
                        Success = false,
                        Message = $"Exception during theme change: {ex.Message}",
                        Exception = ex
                    });
                }
            }
            catch (Exception ex)
            {
                _testResults.Add(new TestResult
                {
                    ComponentName = "ThemeService",
                    TestName = "Theme Tests",
                    Success = false,
                    Message = $"Exception: {ex.Message}",
                    Exception = ex
                });
            }
        }
        
        /// <summary>
        /// Displays test results in the console
        /// </summary>
        public void DisplayResults()
        {
            Console.WriteLine("\n=== TEST RESULTS ===\n");
            
            foreach (var group in _testResults.GroupBy(r => r.ComponentName))
            {
                Console.WriteLine($"Component: {group.Key}");
                Console.WriteLine(new string('-', 40));
                
                foreach (var result in group)
                {
                    Console.ForegroundColor = result.Success ? ConsoleColor.Green : ConsoleColor.Red;
                    Console.Write($"[{(result.Success ? "PASS" : "FAIL")}] ");
                    Console.ResetColor();
                    Console.WriteLine($"{result.TestName}: {result.Message}");
                    
                    if (result.Exception != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"  Exception: {result.Exception.Message}");
                        Console.WriteLine($"  StackTrace: {result.Exception.StackTrace}");
                        Console.ResetColor();
                    }
                    
                    if (result.Results != null && result.Results.Count > 0)
                    {
                        Console.WriteLine("  Results:");
                        foreach (var item in result.Results.Take(5))
                        {
                            Console.WriteLine($"    - {item}");
                        }
                        
                        if (result.Results.Count > 5)
                        {
                            Console.WriteLine($"    ... and {result.Results.Count - 5} more");
                        }
                    }
                    
                    Console.WriteLine();
                }
                
                Console.WriteLine();
            }
            
            // Summary
            int passCount = _testResults.Count(r => r.Success);
            int failCount = _testResults.Count - passCount;
            
            Console.WriteLine($"SUMMARY: {passCount} passed, {failCount} failed, {_testResults.Count} total");
            Console.WriteLine(new string('=', 40));
        }
    }
    
    /// <summary>
    /// Represents the result of a test
    /// </summary>
    public class TestResult
    {
        public string ComponentName { get; set; }
        public string TestName { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public List<string> Results { get; set; }
    }
} 
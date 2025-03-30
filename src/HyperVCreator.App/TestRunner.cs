using System;
using System.Threading.Tasks;

namespace HyperVCreator.App
{
    /// <summary>
    /// TestRunner for running the TestHarness
    /// </summary>
    public class TestRunner
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== HyperV Creator Test Runner ===");
            Console.WriteLine($"Started at: {DateTime.Now}");
            Console.WriteLine("Running tests...\n");
            
            var testHarness = new TestHarness();
            var testReportGenerator = new TestReportGenerator();
            
            try
            {
                // Run all tests
                var testResults = await testHarness.RunAllTests();
                
                // Display results in console
                testHarness.DisplayResults();
                
                // Generate HTML report
                Console.WriteLine("\nGenerating HTML test report...");
                string reportPath = testReportGenerator.GenerateReport(testResults, "Comprehensive Test Report");
                Console.WriteLine($"Report generated at: {reportPath}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: Test runner crashed with exception: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
} 
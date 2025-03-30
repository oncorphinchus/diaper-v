using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HyperVCreator.App
{
    /// <summary>
    /// Generates detailed HTML test reports for the application
    /// </summary>
    public class TestReportGenerator
    {
        private readonly string _reportDirectory;
        private readonly string _applicationName;
        private readonly string _applicationVersion;
        
        public TestReportGenerator(string reportDirectory = null)
        {
            // Use provided directory or default to the application's report directory
            _reportDirectory = reportDirectory ?? 
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                    "HyperVCreator", "TestReports");
            
            // Ensure the directory exists
            if (!Directory.Exists(_reportDirectory))
            {
                Directory.CreateDirectory(_reportDirectory);
            }
            
            // Get application name and version
            var assembly = Assembly.GetEntryAssembly();
            _applicationName = assembly?.GetName().Name ?? "HyperVCreator";
            _applicationVersion = assembly?.GetName().Version?.ToString() ?? "1.0.0";
        }
        
        /// <summary>
        /// Generate an HTML report from test results
        /// </summary>
        /// <param name="testResults">The test results to include in the report</param>
        /// <param name="title">The title of the report</param>
        /// <returns>The path to the generated report file</returns>
        public string GenerateReport(List<TestResult> testResults, string title = "Test Report")
        {
            // Create report filename with timestamp
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string reportFileName = $"TestReport_{timestamp}.html";
            string reportPath = Path.Combine(_reportDirectory, reportFileName);
            
            var sb = new StringBuilder();
            
            // HTML header
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("  <meta charset=\"UTF-8\">");
            sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine($"  <title>{title} - {_applicationName}</title>");
            sb.AppendLine("  <style>");
            sb.AppendLine("    body { font-family: Arial, sans-serif; line-height: 1.6; margin: 0; padding: 20px; color: #333; }");
            sb.AppendLine("    .container { max-width: 1200px; margin: 0 auto; }");
            sb.AppendLine("    h1, h2, h3 { color: #2c3e50; }");
            sb.AppendLine("    .header { margin-bottom: 30px; border-bottom: 1px solid #eee; padding-bottom: 20px; }");
            sb.AppendLine("    .summary { background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin-bottom: 30px; }");
            sb.AppendLine("    .component { margin-bottom: 30px; }");
            sb.AppendLine("    .component-title { padding: 10px; background-color: #2c3e50; color: white; border-radius: 5px 5px 0 0; }");
            sb.AppendLine("    .component-content { border: 1px solid #ddd; border-top: none; padding: 15px; border-radius: 0 0 5px 5px; }");
            sb.AppendLine("    .test-result { margin-bottom: 15px; padding: 10px; border-radius: 5px; }");
            sb.AppendLine("    .test-pass { background-color: #d4edda; color: #155724; border: 1px solid #c3e6cb; }");
            sb.AppendLine("    .test-fail { background-color: #f8d7da; color: #721c24; border: 1px solid #f5c6cb; }");
            sb.AppendLine("    .test-name { font-weight: bold; }");
            sb.AppendLine("    .test-message { margin-top: 5px; }");
            sb.AppendLine("    .test-exception { margin-top: 10px; padding: 10px; background-color: #fff; border: 1px solid #ddd; border-radius: 5px; white-space: pre-wrap; font-family: monospace; font-size: 13px; }");
            sb.AppendLine("    .test-results { margin-top: 10px; }");
            sb.AppendLine("    .test-results ul { padding-left: 25px; }");
            sb.AppendLine("    .progress-bar { height: 20px; background-color: #e9ecef; border-radius: 5px; margin-bottom: 10px; }");
            sb.AppendLine("    .progress-value { height: 100%; background-color: #007bff; border-radius: 5px; text-align: center; color: white; line-height: 20px; font-size: 12px; }");
            sb.AppendLine("    .system-info { margin-top: 30px; padding: 15px; background-color: #f8f9fa; border-radius: 5px; }");
            sb.AppendLine("    .footer { margin-top: 50px; text-align: center; font-size: 12px; color: #6c757d; border-top: 1px solid #eee; padding-top: 20px; }");
            sb.AppendLine("  </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("  <div class=\"container\">");
            
            // Report header
            sb.AppendLine("    <div class=\"header\">");
            sb.AppendLine($"      <h1>{_applicationName} - {title}</h1>");
            sb.AppendLine($"      <p>Generated on {DateTime.Now.ToString("f")} | Version: {_applicationVersion}</p>");
            sb.AppendLine("    </div>");
            
            // Summary
            int totalTests = testResults.Count;
            int passedTests = testResults.Count(r => r.Success);
            int failedTests = totalTests - passedTests;
            double passPercentage = totalTests > 0 ? (double)passedTests / totalTests * 100 : 0;
            
            sb.AppendLine("    <div class=\"summary\">");
            sb.AppendLine("      <h2>Test Summary</h2>");
            sb.AppendLine($"      <p>Total Tests: {totalTests} | Passed: {passedTests} | Failed: {failedTests}</p>");
            sb.AppendLine("      <div class=\"progress-bar\">");
            sb.AppendLine($"        <div class=\"progress-value\" style=\"width: {passPercentage}%\">{passPercentage:F1}%</div>");
            sb.AppendLine("      </div>");
            sb.AppendLine("    </div>");
            
            // Component results
            foreach (var componentGroup in testResults.GroupBy(r => r.ComponentName))
            {
                string componentName = componentGroup.Key;
                int componentTotal = componentGroup.Count();
                int componentPassed = componentGroup.Count(r => r.Success);
                double componentPassPercentage = (double)componentPassed / componentTotal * 100;
                
                sb.AppendLine("    <div class=\"component\">");
                sb.AppendLine($"      <h3 class=\"component-title\">{componentName} ({componentPassed}/{componentTotal} passed, {componentPassPercentage:F1}%)</h3>");
                sb.AppendLine("      <div class=\"component-content\">");
                
                foreach (var result in componentGroup)
                {
                    string resultClass = result.Success ? "test-pass" : "test-fail";
                    sb.AppendLine($"        <div class=\"test-result {resultClass}\">");
                    sb.AppendLine($"          <div class=\"test-name\">{result.TestName}</div>");
                    sb.AppendLine($"          <div class=\"test-message\">{result.Message}</div>");
                    
                    if (result.Exception != null)
                    {
                        sb.AppendLine($"          <div class=\"test-exception\">{result.Exception.Message}\n{result.Exception.StackTrace}</div>");
                    }
                    
                    if (result.Results != null && result.Results.Count > 0)
                    {
                        sb.AppendLine("          <div class=\"test-results\">");
                        sb.AppendLine("            <p>Results:</p>");
                        sb.AppendLine("            <ul>");
                        foreach (var item in result.Results.Take(10))
                        {
                            sb.AppendLine($"              <li>{item}</li>");
                        }
                        if (result.Results.Count > 10)
                        {
                            sb.AppendLine($"              <li>...and {result.Results.Count - 10} more</li>");
                        }
                        sb.AppendLine("            </ul>");
                        sb.AppendLine("          </div>");
                    }
                    
                    sb.AppendLine("        </div>");
                }
                
                sb.AppendLine("      </div>");
                sb.AppendLine("    </div>");
            }
            
            // System information
            sb.AppendLine("    <div class=\"system-info\">");
            sb.AppendLine("      <h2>System Information</h2>");
            sb.AppendLine("      <ul>");
            sb.AppendLine($"        <li>OS: {Environment.OSVersion.VersionString}</li>");
            sb.AppendLine($"        <li>.NET Version: {Environment.Version}</li>");
            sb.AppendLine($"        <li>64-bit OS: {Environment.Is64BitOperatingSystem}</li>");
            sb.AppendLine($"        <li>Machine Name: {Environment.MachineName}</li>");
            sb.AppendLine($"        <li>Processor Count: {Environment.ProcessorCount}</li>");
            sb.AppendLine($"        <li>System Directory: {Environment.SystemDirectory}</li>");
            sb.AppendLine($"        <li>User Name: {Environment.UserName}</li>");
            sb.AppendLine($"        <li>Current Directory: {Environment.CurrentDirectory}</li>");
            sb.AppendLine("      </ul>");
            sb.AppendLine("    </div>");
            
            // Footer
            sb.AppendLine("    <div class=\"footer\">");
            sb.AppendLine($"      <p>{_applicationName} {_applicationVersion} | Test Report Generator</p>");
            sb.AppendLine("    </div>");
            
            // End HTML
            sb.AppendLine("  </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            
            // Write the report to file
            File.WriteAllText(reportPath, sb.ToString());
            
            // Open the report in the default browser
            try
            {
                Process.Start(new ProcessStartInfo(reportPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open report: {ex.Message}");
                Console.WriteLine($"Report saved at: {reportPath}");
            }
            
            return reportPath;
        }
    }
} 
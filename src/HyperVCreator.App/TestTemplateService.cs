using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using HyperVCreator.Core.Models;
using HyperVCreator.Core.Services;

namespace HyperVCreator.App
{
    /// <summary>
    /// Tests the TemplateService functionality
    /// </summary>
    public class TestTemplateService
    {
        private readonly TemplateService _templateService;
        private readonly string _testTemplatePath;
        
        /// <summary>
        /// Initializes a new instance of the TestTemplateService class
        /// </summary>
        public TestTemplateService()
        {
            // Create a temporary directory for test templates
            _testTemplatePath = Path.Combine(Path.GetTempPath(), "HyperVCreatorTests", "Templates", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testTemplatePath);
            
            // Create template service with test path
            _templateService = new TemplateService(_testTemplatePath);
        }
        
        /// <summary>
        /// Runs all template service tests
        /// </summary>
        /// <returns>List of test results</returns>
        public List<TestResult> RunTests()
        {
            var results = new List<TestResult>();
            
            // Test getting all templates
            results.Add(TestGetAllTemplates());
            
            // Test getting template by name
            results.Add(TestGetTemplateByName());
            
            // Test saving a template
            results.Add(TestSaveTemplate());
            
            // Test deleting a template
            results.Add(TestDeleteTemplate());
            
            // Clean up test directory
            try
            {
                if (Directory.Exists(_testTemplatePath))
                {
                    Directory.Delete(_testTemplatePath, true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
            
            return results;
        }
        
        /// <summary>
        /// Test getting all templates
        /// </summary>
        private TestResult TestGetAllTemplates()
        {
            try
            {
                var templates = _templateService.GetAllTemplates();
                bool success = templates != null && templates.Count > 0;
                
                return new TestResult
                {
                    ComponentName = "TemplateService",
                    TestName = "Get All Templates",
                    Success = success,
                    Message = success ? $"Retrieved {templates.Count} templates" : "No templates retrieved"
                };
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    ComponentName = "TemplateService",
                    TestName = "Get All Templates",
                    Success = false,
                    Message = $"Exception: {ex.Message}",
                    Exception = ex
                };
            }
        }
        
        /// <summary>
        /// Test getting template by name
        /// </summary>
        private TestResult TestGetTemplateByName()
        {
            try
            {
                var dcTemplate = _templateService.GetTemplateByName("Default Domain Controller");
                bool success = dcTemplate != null;
                
                return new TestResult
                {
                    ComponentName = "TemplateService",
                    TestName = "Get Template By Name",
                    Success = success,
                    Message = success ? "Retrieved Domain Controller template" : "Domain Controller template not found"
                };
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    ComponentName = "TemplateService",
                    TestName = "Get Template By Name",
                    Success = false,
                    Message = $"Exception: {ex.Message}",
                    Exception = ex
                };
            }
        }
        
        /// <summary>
        /// Test saving a template
        /// </summary>
        private TestResult TestSaveTemplate()
        {
            try
            {
                var template = new VMTemplate
                {
                    TemplateName = "Test Template",
                    ServerRole = "TestRole",
                    Description = "Test description"
                };
                
                bool result = _templateService.SaveTemplate(template);
                
                if (result)
                {
                    // Verify template was saved
                    var savedTemplate = _templateService.GetTemplateByName("Test Template");
                    bool success = savedTemplate != null && 
                                   savedTemplate.TemplateName == "Test Template" &&
                                   savedTemplate.ServerRole == "TestRole";
                    
                    return new TestResult
                    {
                        ComponentName = "TemplateService",
                        TestName = "Save Template",
                        Success = success,
                        Message = success ? "Template saved and retrieved successfully" : "Template saved but retrieval failed"
                    };
                }
                else
                {
                    return new TestResult
                    {
                        ComponentName = "TemplateService",
                        TestName = "Save Template",
                        Success = false,
                        Message = "SaveTemplate returned false"
                    };
                }
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    ComponentName = "TemplateService",
                    TestName = "Save Template",
                    Success = false,
                    Message = $"Exception: {ex.Message}",
                    Exception = ex
                };
            }
        }
        
        /// <summary>
        /// Test deleting a template
        /// </summary>
        private TestResult TestDeleteTemplate()
        {
            try
            {
                // Create a template to delete
                var template = new VMTemplate
                {
                    TemplateName = "Template To Delete",
                    ServerRole = "TestRole"
                };
                
                bool saveResult = _templateService.SaveTemplate(template);
                if (!saveResult)
                {
                    return new TestResult
                    {
                        ComponentName = "TemplateService",
                        TestName = "Delete Template",
                        Success = false,
                        Message = "Failed to create test template for deletion"
                    };
                }
                
                bool deleteResult = _templateService.DeleteTemplate("Template To Delete");
                
                if (deleteResult)
                {
                    // Verify template was deleted
                    var deletedTemplate = _templateService.GetTemplateByName("Template To Delete");
                    bool success = deletedTemplate == null;
                    
                    return new TestResult
                    {
                        ComponentName = "TemplateService",
                        TestName = "Delete Template",
                        Success = success,
                        Message = success ? "Template deleted successfully" : "Template still exists after deletion"
                    };
                }
                else
                {
                    return new TestResult
                    {
                        ComponentName = "TemplateService",
                        TestName = "Delete Template",
                        Success = false,
                        Message = "DeleteTemplate returned false"
                    };
                }
            }
            catch (Exception ex)
            {
                return new TestResult
                {
                    ComponentName = "TemplateService",
                    TestName = "Delete Template",
                    Success = false,
                    Message = $"Exception: {ex.Message}",
                    Exception = ex
                };
            }
        }
    }
} 
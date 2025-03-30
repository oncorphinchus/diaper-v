using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HyperVCreator.Core.Tests
{
    /// <summary>
    /// Base class for all test classes
    /// </summary>
    public abstract class TestBase
    {
        /// <summary>
        /// Gets a temporary directory for test data
        /// </summary>
        /// <returns>The temporary directory path</returns>
        protected string GetTestDataDirectory()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "HyperVCreatorTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            return tempPath;
        }
        
        /// <summary>
        /// Deletes a directory and all its contents recursively
        /// </summary>
        /// <param name="directory">The directory to delete</param>
        protected void DeleteDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                // Try to remove any read-only attributes from all files
                foreach (var file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.IsReadOnly)
                        {
                            fileInfo.IsReadOnly = false;
                        }
                    }
                    catch
                    {
                        // Ignore errors during attribute removal
                    }
                }
                
                try
                {
                    Directory.Delete(directory, true);
                }
                catch
                {
                    // In case the directory can't be deleted immediately, 
                    // schedule it for deletion on application exit
                    try
                    {
                        File.AppendAllText(
                            Path.Combine(Path.GetTempPath(), "HyperVCreator_cleanup.log"),
                            $"{DateTime.Now}: Failed to delete {directory}\n");
                    }
                    catch
                    {
                        // Ignore errors during logging
                    }
                }
            }
        }
        
        /// <summary>
        /// Creates a test file with the specified content
        /// </summary>
        /// <param name="directory">The directory to create the file in</param>
        /// <param name="fileName">The name of the file</param>
        /// <param name="content">The content to write to the file</param>
        /// <returns>The full path to the created file</returns>
        protected string CreateTestFile(string directory, string fileName, string content)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            string filePath = Path.Combine(directory, fileName);
            File.WriteAllText(filePath, content);
            return filePath;
        }
    }
} 
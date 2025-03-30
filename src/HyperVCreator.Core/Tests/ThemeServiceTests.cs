using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HyperVCreator.Core.Services;

namespace HyperVCreator.Core.Tests
{
    [TestClass]
    public class ThemeServiceTests : TestBase
    {
        private string _testSettingsPath;
        private ThemeService _themeService;
        
        [TestInitialize]
        public void Setup()
        {
            // Create a temporary directory for test settings
            _testSettingsPath = GetTestDataDirectory();
            
            // Create theme service without parameters
            _themeService = new ThemeService();
        }
        
        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test directory
            DeleteDirectory(_testSettingsPath);
        }
        
        [TestMethod]
        [TestCategory("UnitTest")]
        public void CurrentTheme_ShouldReturnDefaultTheme_WhenNoSettingsExist()
        {
            // Act
            string currentTheme = _themeService.CurrentTheme;
            
            // Assert
            Assert.AreEqual("Classic", currentTheme);
        }
        
        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetTheme_ShouldUpdateCurrentTheme()
        {
            // Act
            _themeService.SetTheme("Dark");
            
            // Assert
            Assert.AreEqual("Dark", _themeService.CurrentTheme);
        }
        
        [TestMethod]
        [TestCategory("UnitTest")]
        public void SetTheme_ShouldTriggerThemeChangedEvent()
        {
            // Arrange
            bool eventTriggered = false;
            _themeService.ThemeChanged += (s, e) => eventTriggered = true;
            
            // Act
            _themeService.SetTheme("Dark");
            
            // Assert
            Assert.IsTrue(eventTriggered);
        }
        
        [TestMethod]
        [TestCategory("UnitTest")]
        public void SaveThemePreference_ShouldPersistTheme()
        {
            // Arrange
            string themeToSave = "Dark";
            
            // Act
            _themeService.SaveThemePreference(themeToSave);
            string loadedTheme = _themeService.LoadThemePreference();
            
            // Assert
            Assert.AreEqual(themeToSave, loadedTheme);
        }
        
        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(ArgumentException))]
        public void SetTheme_ShouldThrowException_WhenThemeIsInvalid()
        {
            // Act
            _themeService.SetTheme("InvalidTheme");
            
            // Assert is handled by ExpectedException
        }
    }
} 
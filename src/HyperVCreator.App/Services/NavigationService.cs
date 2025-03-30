using System;
using System.Collections.Generic;
using System.Windows;

namespace HyperVCreator.App.Services
{
    /// <summary>
    /// Implementation of the navigation service
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly Action<string> _navigationCallback;
        private readonly Stack<string> _navigationHistory = new Stack<string>();
        private string _currentView;
        
        /// <summary>
        /// Create a new instance of the navigation service
        /// </summary>
        /// <param name="navigationCallback">Callback to execute when navigation is requested</param>
        public NavigationService(Action<string> navigationCallback)
        {
            _navigationCallback = navigationCallback ?? throw new ArgumentNullException(nameof(navigationCallback));
        }
        
        /// <summary>
        /// Gets whether the navigation service can go back
        /// </summary>
        public bool CanGoBack => _navigationHistory.Count > 0;
        
        /// <summary>
        /// Navigate to a specific view
        /// </summary>
        /// <param name="viewType">Type of the view to navigate to</param>
        public void NavigateTo(Type viewType)
        {
            if (viewType == null)
                throw new ArgumentNullException(nameof(viewType));
                
            // Use the type name as the view name, removing any "View" suffix
            string viewName = viewType.Name;
            if (viewName.EndsWith("View", StringComparison.OrdinalIgnoreCase))
                viewName = viewName.Substring(0, viewName.Length - 4);
                
            NavigateTo(viewName);
        }
        
        /// <summary>
        /// Navigate to a specific view
        /// </summary>
        /// <typeparam name="T">Type of the view to navigate to</typeparam>
        public void NavigateTo<T>() where T : class
        {
            NavigateTo(typeof(T));
        }
        
        /// <summary>
        /// Navigate to the main page
        /// </summary>
        public void NavigateToMainPage()
        {
            NavigateTo("Home");
        }
        
        /// <summary>
        /// Navigate to a specific view
        /// </summary>
        /// <param name="viewName">Name of the view to navigate to</param>
        public void NavigateTo(string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName))
                throw new ArgumentException("View name cannot be null or empty", nameof(viewName));
                
            // Save the current view in the history if it exists
            if (!string.IsNullOrEmpty(_currentView))
                _navigationHistory.Push(_currentView);
                
            // Update the current view and navigate
            _currentView = viewName;
            _navigationCallback(viewName);
        }
        
        /// <summary>
        /// Navigate to the previous page
        /// </summary>
        public void GoBack()
        {
            if (!CanGoBack)
                return;
                
            string previousView = _navigationHistory.Pop();
            _currentView = previousView;
            _navigationCallback(previousView);
        }
    }
} 
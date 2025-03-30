using System;

namespace HyperVCreator.App.Services
{
    /// <summary>
    /// Interface for navigation service
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigate to a specific view
        /// </summary>
        /// <param name="viewType">Type of the view to navigate to</param>
        void NavigateTo(Type viewType);
        
        /// <summary>
        /// Navigate to a specific view
        /// </summary>
        /// <typeparam name="T">Type of the view to navigate to</typeparam>
        void NavigateTo<T>() where T : class;
        
        /// <summary>
        /// Navigate to the main page
        /// </summary>
        void NavigateToMainPage();
        
        /// <summary>
        /// Navigate to the previous page
        /// </summary>
        void GoBack();
        
        /// <summary>
        /// Gets whether the navigation service can go back
        /// </summary>
        bool CanGoBack { get; }
    }
} 
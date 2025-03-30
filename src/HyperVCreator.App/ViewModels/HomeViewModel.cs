using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HyperVCreator.App.Commands;

namespace HyperVCreator.App.ViewModels
{
    /// <summary>
    /// ViewModel for the home view
    /// </summary>
    public class HomeViewModel : INotifyPropertyChanged
    {
        #region Properties
        
        private string _welcomeMessage = "Welcome to Hyper-V VM Creator";
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set
            {
                if (_welcomeMessage != value)
                {
                    _welcomeMessage = value;
                    OnPropertyChanged();
                }
            }
        }
        
        #endregion
        
        #region Commands
        
        // Commands will be handled by the MainWindowViewModel
        
        #endregion
        
        /// <summary>
        /// Initialize a new instance of the home view model
        /// </summary>
        public HomeViewModel()
        {
            // Any additional initialization
        }
        
        #region INotifyPropertyChanged
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        #endregion
    }
} 
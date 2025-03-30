using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HyperVCreator.App.ViewModels
{
    /// <summary>
    /// Base class for all ViewModels
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Event raised when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for the specified property
        /// </summary>
        /// <param name="propertyName">The name of the property that changed</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets a property value and raises the PropertyChanged event if the value changed
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="storage">Reference to the backing field</param>
        /// <param name="value">The new value</param>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>True if the value changed, false otherwise</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
} 
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using HyperVCreator.App.Commands;
using HyperVCreator.App.Services;
using HyperVCreator.App.Views;
using HyperVCreator.App.Views.ServerRoles.DomainController;
using HyperVCreator.App.Views.ServerRoles.FileServer;
using HyperVCreator.App.Views.ServerRoles.RDSH;
using HyperVCreator.App.Views.ServerRoles.SQLServer;
using HyperVCreator.Core.Services;

namespace HyperVCreator.App.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IThemeService _themeService;
        
        #region Properties
        
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _appTitle = "Hyper-V VM Creator";
        public string AppTitle
        {
            get => _appTitle;
            set
            {
                if (_appTitle != value)
                {
                    _appTitle = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _statusMessage = "Ready";
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }
        
        #endregion
        
        #region Commands
        
        private ICommand _navigateHomeCommand;
        public ICommand NavigateHomeCommand => _navigateHomeCommand ??= new RelayCommand(
            execute: _ => NavigateToHome(),
            canExecute: _ => true
        );
        
        private ICommand _navigateToTemplatesCommand;
        public ICommand NavigateToTemplatesCommand => _navigateToTemplatesCommand ??= new RelayCommand(
            execute: _ => NavigateToTemplates(),
            canExecute: _ => true
        );
        
        private ICommand _navigateToSettingsCommand;
        public ICommand NavigateToSettingsCommand => _navigateToSettingsCommand ??= new RelayCommand(
            execute: _ => NavigateToSettings(),
            canExecute: _ => true
        );
        
        private ICommand _navigateToDomainControllerCommand;
        public ICommand NavigateToDomainControllerCommand => _navigateToDomainControllerCommand ??= new RelayCommand(
            execute: _ => NavigateToDomainController(),
            canExecute: _ => true
        );
        
        private ICommand _navigateToRDSHCommand;
        public ICommand NavigateToRDSHCommand => _navigateToRDSHCommand ??= new RelayCommand(
            execute: _ => NavigateToRDSH(),
            canExecute: _ => true
        );
        
        private ICommand _navigateToSQLServerCommand;
        public ICommand NavigateToSQLServerCommand => _navigateToSQLServerCommand ??= new RelayCommand(
            execute: _ => NavigateToSQLServer(),
            canExecute: _ => true
        );
        
        private ICommand _navigateToFileServerCommand;
        public ICommand NavigateToFileServerCommand => _navigateToFileServerCommand ??= new RelayCommand(
            execute: _ => NavigateToFileServer(),
            canExecute: _ => true
        );
        
        private ICommand _navigateToCustomVMCommand;
        public ICommand NavigateToCustomVMCommand => _navigateToCustomVMCommand ??= new RelayCommand(
            execute: _ => NavigateToCustomVM(),
            canExecute: _ => true
        );
        
        private ICommand _exitApplicationCommand;
        public ICommand ExitApplicationCommand => _exitApplicationCommand ??= new RelayCommand(
            execute: _ => ExitApplication(),
            canExecute: _ => true
        );
        
        #endregion
        
        public MainWindowViewModel()
        {
            _themeService = App.Current.Resources["ThemeService"] as IThemeService;
            
            // Set the initial view
            NavigateToHome();
        }
        
        #region Navigation Methods
        
        private void NavigateToHome()
        {
            CurrentView = new HomeView();
            StatusMessage = "Ready";
        }
        
        private void NavigateToTemplates()
        {
            CurrentView = new TemplateListView();
            StatusMessage = "Template Management";
        }
        
        private void NavigateToSettings()
        {
            CurrentView = new ThemeSettingsView();
            StatusMessage = "Settings";
        }
        
        private void NavigateToDomainController()
        {
            CurrentView = new DomainControllerConfigView();
            StatusMessage = "Domain Controller Configuration";
        }
        
        private void NavigateToRDSH()
        {
            CurrentView = new RDSHConfigView();
            StatusMessage = "Remote Desktop Session Host Configuration";
        }
        
        private void NavigateToSQLServer()
        {
            CurrentView = new SQLServerConfigView();
            StatusMessage = "SQL Server Configuration";
        }
        
        private void NavigateToFileServer()
        {
            CurrentView = new FileServerConfigView();
            StatusMessage = "File Server Configuration";
        }
        
        private void NavigateToCustomVM()
        {
            CurrentView = new Views.ServerRoles.CustomVM.CustomVMConfigView();
            StatusMessage = "Custom VM Configuration";
        }
        
        private void ExitApplication()
        {
            Application.Current.Shutdown();
        }
        
        #endregion
        
        #region INotifyPropertyChanged
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        #endregion
    }
} 
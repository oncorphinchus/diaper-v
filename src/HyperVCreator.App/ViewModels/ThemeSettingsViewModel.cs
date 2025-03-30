using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HyperVCreator.App.Services;

namespace HyperVCreator.App.ViewModels
{
    /// <summary>
    /// ViewModel for theme selection
    /// </summary>
    public class ThemeSettingsViewModel : ViewModelBase
    {
        private readonly ThemeService _themeService;
        private ThemeItemViewModel _selectedTheme;
        
        /// <summary>
        /// Gets the available themes
        /// </summary>
        public ObservableCollection<ThemeItemViewModel> Themes { get; }
        
        /// <summary>
        /// Gets or sets the selected theme
        /// </summary>
        public ThemeItemViewModel SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (SetProperty(ref _selectedTheme, value) && value != null)
                {
                    // Apply the selected theme
                    _themeService.ChangeTheme(value.ThemeType);
                }
            }
        }
        
        /// <summary>
        /// Command to apply the selected theme
        /// </summary>
        public ICommand ApplyThemeCommand { get; }
        
        /// <summary>
        /// Initializes a new instance of the ThemeSettingsViewModel class
        /// </summary>
        /// <param name="themeService">The theme service</param>
        public ThemeSettingsViewModel(ThemeService themeService)
        {
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            
            // Initialize available themes
            Themes = new ObservableCollection<ThemeItemViewModel>(
                _themeService.GetAvailableThemes().Select(t => new ThemeItemViewModel
                {
                    ThemeType = t,
                    DisplayName = _themeService.GetThemeDisplayName(t),
                    IsSelected = t == _themeService.CurrentTheme
                }));
            
            // Set initial selection
            _selectedTheme = Themes.FirstOrDefault(t => t.ThemeType == _themeService.CurrentTheme);
            
            // Initialize commands
            ApplyThemeCommand = new RelayCommand(_ => ApplySelectedTheme());
            
            // Subscribe to theme changed events
            _themeService.ThemeChanged += OnThemeChanged;
        }
        
        /// <summary>
        /// Applies the selected theme
        /// </summary>
        private void ApplySelectedTheme()
        {
            if (SelectedTheme != null)
            {
                _themeService.ChangeTheme(SelectedTheme.ThemeType);
            }
        }
        
        /// <summary>
        /// Handles theme changed events
        /// </summary>
        private void OnThemeChanged(object sender, ThemeType themeType)
        {
            // Update the selected status of all themes
            foreach (var theme in Themes)
            {
                theme.IsSelected = theme.ThemeType == themeType;
            }
            
            // Update selected theme
            _selectedTheme = Themes.FirstOrDefault(t => t.ThemeType == themeType);
            OnPropertyChanged(nameof(SelectedTheme));
        }
    }
    
    /// <summary>
    /// Represents a theme option
    /// </summary>
    public class ThemeItemViewModel : ViewModelBase
    {
        private bool _isSelected;
        
        /// <summary>
        /// Gets or sets the theme type
        /// </summary>
        public ThemeType ThemeType { get; set; }
        
        /// <summary>
        /// Gets or sets the display name
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// Gets or sets whether this theme is selected
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
} 
using System;
using System.Windows;
using HyperVCreator.App.Services;

namespace HyperVCreator.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize theme service if needed
            if (!App.Current.Resources.Contains("ThemeService"))
            {
                var themeService = new ThemeService();
                themeService.LoadSelectedTheme();
                App.Current.Resources.Add("ThemeService", themeService);
            }
        }
    }
} 
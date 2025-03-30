using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using HyperVCreator.Core.Services;

namespace HyperVCreator.App.ViewModels
{
    public class TemplateListViewModel : ViewModelBase
    {
        private readonly TemplateService _templateService;
        private ObservableCollection<TemplateItemViewModel> _templates;
        private TemplateItemViewModel _selectedTemplate;
        private bool _isLoading;
        private string _searchTerm;
        private string _selectedRole;

        public TemplateListViewModel(TemplateService templateService)
        {
            _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            Templates = new ObservableCollection<TemplateItemViewModel>();
            
            // Commands
            RefreshCommand = new RelayCommand(async _ => await LoadTemplatesAsync(), _ => !IsLoading);
            CreateNewCommand = new RelayCommand(_ => CreateNewTemplate(), _ => !IsLoading);
            EditCommand = new RelayCommand(_ => EditSelectedTemplate(), _ => SelectedTemplate != null && !IsLoading);
            DeleteCommand = new RelayCommand(async _ => await DeleteSelectedTemplateAsync(), _ => SelectedTemplate != null && !IsLoading);
            CloneCommand = new RelayCommand(async _ => await CloneSelectedTemplateAsync(), _ => SelectedTemplate != null && !IsLoading);
            
            // Load templates initially
            Task.Run(LoadTemplatesAsync);
        }

        public ObservableCollection<TemplateItemViewModel> Templates
        {
            get => _templates;
            set
            {
                _templates = value;
                OnPropertyChanged(nameof(Templates));
            }
        }

        public TemplateItemViewModel SelectedTemplate
        {
            get => _selectedTemplate;
            set
            {
                _selectedTemplate = value;
                OnPropertyChanged(nameof(SelectedTemplate));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged(nameof(SearchTerm));
                ApplyFilter();
            }
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged(nameof(SelectedRole));
                Task.Run(LoadTemplatesAsync);
            }
        }

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand CreateNewCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CloneCommand { get; }

        public async Task LoadTemplatesAsync()
        {
            IsLoading = true;
            Templates.Clear();

            try
            {
                var templates = string.IsNullOrEmpty(SelectedRole)
                    ? await _templateService.GetAllTemplatesAsync()
                    : await _templateService.GetTemplatesByRoleAsync(SelectedRole);

                foreach (var template in templates)
                {
                    Templates.Add(new TemplateItemViewModel(template));
                }

                ApplyFilter();
            }
            catch (Exception ex)
            {
                // Log the error and show message to user
                Console.WriteLine($"Error loading templates: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CreateNewTemplate()
        {
            // Navigate to template editor with new template
            // TODO: Implement navigation service to navigate between views
            // This will be implemented with the navigation service in the Base Application phase
            // MessengerInstance.Send(new NavigateToTemplateEditorMessage(null));
        }

        private void EditSelectedTemplate()
        {
            if (SelectedTemplate != null)
            {
                // Navigate to template editor with selected template
                // MessengerInstance.Send(new NavigateToTemplateEditorMessage(SelectedTemplate.Template));
            }
        }

        private async Task DeleteSelectedTemplateAsync()
        {
            if (SelectedTemplate == null) return;

            // Confirm deletion
            // var result = MessageBox.Show($"Are you sure you want to delete the template '{SelectedTemplate.Name}'?", 
            //     "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            // if (result == MessageBoxResult.Yes)
            // {
                try
                {
                    _templateService.DeleteTemplate(SelectedTemplate.FileName);
                    Templates.Remove(SelectedTemplate);
                    SelectedTemplate = null;
                }
                catch (Exception ex)
                {
                    // Log the error and show message to user
                    Console.WriteLine($"Error deleting template: {ex.Message}");
                }
            // }
        }

        private async Task CloneSelectedTemplateAsync()
        {
            if (SelectedTemplate == null) return;

            try
            {
                var clonedTemplate = _templateService.CloneTemplate(
                    SelectedTemplate.Template, 
                    $"{SelectedTemplate.Name} (Copy)");
                
                // Save the cloned template
                await _templateService.SaveTemplateAsync(clonedTemplate);
                
                // Refresh the list
                await LoadTemplatesAsync();
            }
            catch (Exception ex)
            {
                // Log the error and show message to user
                Console.WriteLine($"Error cloning template: {ex.Message}");
            }
        }

        private void ApplyFilter()
        {
            // If search term is empty, reload all templates for selected role
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                Task.Run(LoadTemplatesAsync);
                return;
            }

            // Filter templates based on search term
            var filteredTemplates = Templates.Where(t =>
                t.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                t.Role.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                t.Template.Metadata.Tags.Any(tag => tag.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            // Update the Templates collection
            Templates.Clear();
            foreach (var template in filteredTemplates)
            {
                Templates.Add(template);
            }
        }
    }
} 
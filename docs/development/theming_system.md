# Theming System Documentation

## Overview
The HyperV Creator application includes a theming system that allows users to customize the appearance of the application. Three themes are provided out of the box:

1. **Classic Theme**: A professional light theme with blue accents
2. **Dark Theme**: A dark theme for reduced eye strain
3. **Sanrio Theme**: A themed experience with a playful, colorful design

## Implementation

### Resource Dictionary Structure
The theming system uses resource dictionaries to define styles, brushes, and other visual resources:

- **BaseTheme.xaml**: Contains common styles and resources shared across all themes
- **ClassicTheme/ClassicTheme.xaml**: Implements the Classic theme
- **DarkTheme/DarkTheme.xaml**: Implements the Dark theme
- **SanrioTheme/SanrioTheme.xaml**: Implements the Sanrio theme

### Resources Defined in Themes
Each theme defines the following resources:

- Colors (PrimaryColor, SecondaryColor, AccentColor, etc.)
- Brushes derived from those colors
- Control styles (Button, TextBox, ComboBox, etc.)
- Special theme-specific styles (e.g., Sanrio theme adds decorations to headers)

### Theme Switching
Theme switching is handled by the `ThemeService` class, which:

1. Manages the list of available themes
2. Allows changing the current theme at runtime
3. Persists theme selection across application sessions
4. Provides events for theme changes

### Converters
Several value converters support the theming system:

- **BrushLightenConverter**: Lightens or darkens brushes by a percentage
- **StringDecorationConverter**: Decorates text with special characters (used in Sanrio theme)
- **ThemePreviewConverters**: Generate previews of themes in the theme selector

## User Interface
The theme selection UI is provided through the `ThemeSettingsView`, which displays:

1. A list of available themes with visual previews
2. The currently selected theme
3. A brief description of the theming system

## Adding a New Theme
To add a new theme:

1. Create a new folder in the `Themes` directory
2. Create a new XAML file with your theme definitions
3. Add the theme to the `ThemeType` enum in `ThemeService.cs`
4. Add a path mapping in the `_themeResourcePaths` dictionary
5. Add preview colors in `ThemePreviewConverters.cs`

## Best Practices
When working with the theming system:

1. Always use resource references (`{StaticResource ResourceKey}`) rather than hard-coded values
2. Test all UI components with each theme
3. Ensure sufficient contrast between text and background colors
4. Make sure interactive elements are clearly identifiable in all themes

## Known Limitations
- Custom controls may require additional theme-specific styling
- Some third-party controls may not fully adopt the theme 
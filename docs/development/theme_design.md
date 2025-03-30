# Theme Design

This document outlines the design specifications for the application's theming system, including color schemes, visual assets, and implementation details for each of the three planned themes.

## Theming Architecture

### Resource Dictionary Structure

The application's theming is implemented using WPF resource dictionaries, with a hierarchical structure:

```
Themes/
├── BaseTheme.xaml              # Common styles and templates
├── ClassicTheme/
│   ├── ClassicTheme.xaml       # Main theme dictionary (merges others)
│   ├── Colors.xaml             # Color definitions
│   ├── Brushes.xaml            # Brush resources
│   ├── Controls.xaml           # Control style overrides
│   └── Assets/                 # Theme-specific images and icons
├── SanrioTheme/
│   ├── SanrioTheme.xaml        # Main theme dictionary
│   ├── Colors.xaml
│   ├── Brushes.xaml
│   ├── Controls.xaml
│   └── Assets/
└── DarkTheme/
    ├── DarkTheme.xaml          # Main theme dictionary
    ├── Colors.xaml
    ├── Brushes.xaml
    ├── Controls.xaml
    └── Assets/
```

### Theme Loading Mechanism

Themes are dynamically loaded at runtime:

1. Theme selection is saved in application settings.
2. At startup, the appropriate theme is loaded from the resource dictionaries.
3. Theme changes are applied immediately by:
   - Removing the current theme dictionary from application resources
   - Adding the new theme dictionary to application resources

## Classic Theme

The Classic theme provides a professional, Windows-standard appearance suitable for enterprise environments.

### Color Palette

| Element | Color | Hex Code | Sample |
|---------|-------|----------|--------|
| Primary Background | Light Gray | #F0F0F0 | ![#F0F0F0](https://via.placeholder.com/15/F0F0F0/000000?text=+) |
| Secondary Background | White | #FFFFFF | ![#FFFFFF](https://via.placeholder.com/15/FFFFFF/000000?text=+) |
| Primary Text | Dark Gray | #333333 | ![#333333](https://via.placeholder.com/15/333333/000000?text=+) |
| Secondary Text | Medium Gray | #666666 | ![#666666](https://via.placeholder.com/15/666666/000000?text=+) |
| Accent 1 | Blue | #0078D7 | ![#0078D7](https://via.placeholder.com/15/0078D7/000000?text=+) |
| Accent 2 | Light Blue | #58B2DC | ![#58B2DC](https://via.placeholder.com/15/58B2DC/000000?text=+) |
| Success | Green | #107C10 | ![#107C10](https://via.placeholder.com/15/107C10/000000?text=+) |
| Error | Red | #E81123 | ![#E81123](https://via.placeholder.com/15/E81123/000000?text=+) |
| Warning | Orange | #FF8C00 | ![#FF8C00](https://via.placeholder.com/15/FF8C00/000000?text=+) |
| Information | Blue | #0063B1 | ![#0063B1](https://via.placeholder.com/15/0063B1/000000?text=+) |
| Border | Light Gray | #CCCCCC | ![#CCCCCC](https://via.placeholder.com/15/CCCCCC/000000?text=+) |

### Typography

- **Primary Font**: Segoe UI
- **Header Sizes**:
  - H1: 24px
  - H2: 20px
  - H3: 16px
- **Body Text**: 12px
- **Small Text**: 11px

### Control Styles

- **Buttons**: 
  - Rectangular with slight rounded corners (2px radius)
  - Blue accent color for primary actions
  - Gray for secondary actions
  
- **Input Fields**:
  - White background
  - Light gray border
  - Blue border when focused
  
- **Checkboxes/Radio Buttons**:
  - Standard Windows-style controls
  - Blue accent for selected state
  
- **Progress Indicators**:
  - Blue accent color for progress bars
  - Standard circular progress ring for indeterminate operations

### Icons and Visual Elements

- Standard, professional Windows-style icons
- Line-based icons with minimal color usage
- High contrast for accessibility
- Consistent sizing and padding

## Sanrio Theme

The Sanrio theme provides a cute, colorful appearance inspired by Sanrio characters and aesthetics.

### Color Palette

| Element | Color | Hex Code | Sample |
|---------|-------|----------|--------|
| Primary Background | Soft Pink | #FEE7F0 | ![#FEE7F0](https://via.placeholder.com/15/FEE7F0/000000?text=+) |
| Secondary Background | White | #FFFFFF | ![#FFFFFF](https://via.placeholder.com/15/FFFFFF/000000?text=+) |
| Primary Text | Dark Purple | #4B0082 | ![#4B0082](https://via.placeholder.com/15/4B0082/000000?text=+) |
| Secondary Text | Medium Purple | #683A5E | ![#683A5E](https://via.placeholder.com/15/683A5E/000000?text=+) |
| Accent 1 | Bright Pink | #FF69B4 | ![#FF69B4](https://via.placeholder.com/15/FF69B4/000000?text=+) |
| Accent 2 | Light Blue | #ADD8E6 | ![#ADD8E6](https://via.placeholder.com/15/ADD8E6/000000?text=+) |
| Success | Pastel Green | #77DD77 | ![#77DD77](https://via.placeholder.com/15/77DD77/000000?text=+) |
| Error | Soft Red | #FF6B6B | ![#FF6B6B](https://via.placeholder.com/15/FF6B6B/000000?text=+) |
| Warning | Pastel Orange | #FFD1A1 | ![#FFD1A1](https://via.placeholder.com/15/FFD1A1/000000?text=+) |
| Information | Lavender | #E6E6FA | ![#E6E6FA](https://via.placeholder.com/15/E6E6FA/000000?text=+) |
| Border | Pastel Pink | #FFB2C0 | ![#FFB2C0](https://via.placeholder.com/15/FFB2C0/000000?text=+) |

### Typography

- **Primary Font**: Comic Sans MS (or a similar playful font)
- **Header Sizes**:
  - H1: 24px
  - H2: 20px
  - H3: 16px
- **Body Text**: 12px
- **Small Text**: 11px

### Control Styles

- **Buttons**: 
  - Fully rounded corners (10px radius)
  - Pink gradient background
  - White text for contrast
  
- **Input Fields**:
  - White background
  - Light pink border
  - Pink border with glow effect when focused
  
- **Checkboxes/Radio Buttons**:
  - Custom styled with character-inspired designs
  - Pink accent for selected state
  
- **Progress Indicators**:
  - Custom progress bars with character-inspired designs
  - Animated elements for indeterminate operations

### Icons and Visual Elements

- Character-inspired icons where applicable
- Rounded, cute styling
- Pastel color palette throughout
- Use of decorative elements like ribbons, bows, and stars
- Possible character cameos in UI elements

## Dark Theme

The Dark theme provides a modern, low-light appearance suitable for reduced eye strain and night-time use.

### Color Palette

| Element | Color | Hex Code | Sample |
|---------|-------|----------|--------|
| Primary Background | Dark Gray | #1E1E1E | ![#1E1E1E](https://via.placeholder.com/15/1E1E1E/000000?text=+) |
| Secondary Background | Medium Gray | #2D2D2D | ![#2D2D2D](https://via.placeholder.com/15/2D2D2D/000000?text=+) |
| Primary Text | White | #FFFFFF | ![#FFFFFF](https://via.placeholder.com/15/FFFFFF/000000?text=+) |
| Secondary Text | Light Gray | #BBBBBB | ![#BBBBBB](https://via.placeholder.com/15/BBBBBB/000000?text=+) |
| Accent 1 | Cyan | #00B7C3 | ![#00B7C3](https://via.placeholder.com/15/00B7C3/000000?text=+) |
| Accent 2 | Purple | #6B69D6 | ![#6B69D6](https://via.placeholder.com/15/6B69D6/000000?text=+) |
| Success | Green | #13A10E | ![#13A10E](https://via.placeholder.com/15/13A10E/000000?text=+) |
| Error | Red | #FF5252 | ![#FF5252](https://via.placeholder.com/15/FF5252/000000?text=+) |
| Warning | Amber | #FFC400 | ![#FFC400](https://via.placeholder.com/15/FFC400/000000?text=+) |
| Information | Blue | #0073CF | ![#0073CF](https://via.placeholder.com/15/0073CF/000000?text=+) |
| Border | Dark Gray | #444444 | ![#444444](https://via.placeholder.com/15/444444/000000?text=+) |

### Typography

- **Primary Font**: Segoe UI
- **Header Sizes**:
  - H1: 24px
  - H2: 20px
  - H3: 16px
- **Body Text**: 12px
- **Small Text**: 11px

### Control Styles

- **Buttons**: 
  - Rectangular with slight rounded corners (2px radius)
  - Cyan accent color for primary actions
  - Medium gray for secondary actions
  
- **Input Fields**:
  - Medium gray background
  - Dark gray border
  - Cyan border when focused
  
- **Checkboxes/Radio Buttons**:
  - Dark-styled controls
  - Cyan accent for selected state
  
- **Progress Indicators**:
  - Cyan accent color for progress bars
  - Subtle glow effects for active elements

### Icons and Visual Elements

- Outline-style icons with high contrast
- Minimalist design approach
- Consistent sizing and padding
- Subtle shadow effects for depth

## Implementation Details

### Resource Dictionary Examples

#### Base Theme (Colors.xaml)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!-- Base color definitions used across all themes -->
    <Color x:Key="BaseBackgroundColor">#FFFFFF</Color>
    <Color x:Key="BaseForegroundColor">#000000</Color>
    <!-- Additional base colors -->
</ResourceDictionary>
```

#### Classic Theme (Colors.xaml)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!-- Classic Theme Colors -->
    <Color x:Key="ThemePrimaryBackgroundColor">#F0F0F0</Color>
    <Color x:Key="ThemeSecondaryBackgroundColor">#FFFFFF</Color>
    <Color x:Key="ThemePrimaryTextColor">#333333</Color>
    <Color x:Key="ThemeSecondaryTextColor">#666666</Color>
    <Color x:Key="ThemeAccent1Color">#0078D7</Color>
    <Color x:Key="ThemeAccent2Color">#58B2DC</Color>
    <!-- Additional theme colors -->
</ResourceDictionary>
```

### Theme Switching Code

```csharp
public class ThemeManager
{
    private const string ClassicThemePath = "/Themes/ClassicTheme/ClassicTheme.xaml";
    private const string SanrioThemePath = "/Themes/SanrioTheme/SanrioTheme.xaml";
    private const string DarkThemePath = "/Themes/DarkTheme/DarkTheme.xaml";

    private ResourceDictionary _currentThemeDictionary;

    public void ApplyTheme(string themeName)
    {
        // Get the current app resources
        var appResources = Application.Current.Resources;
        
        // Remove the current theme dictionary if it exists
        if (_currentThemeDictionary != null)
        {
            appResources.MergedDictionaries.Remove(_currentThemeDictionary);
        }

        // Determine the new theme path
        string themePath;
        switch (themeName.ToLower())
        {
            case "sanrio":
                themePath = SanrioThemePath;
                break;
            case "dark":
                themePath = DarkThemePath;
                break;
            case "classic":
            default:
                themePath = ClassicThemePath;
                break;
        }

        // Create and load the new theme dictionary
        _currentThemeDictionary = new ResourceDictionary
        {
            Source = new Uri(themePath, UriKind.Relative)
        };

        // Add the new theme dictionary
        appResources.MergedDictionaries.Add(_currentThemeDictionary);
        
        // Save the selected theme to settings
        Properties.Settings.Default.SelectedTheme = themeName;
        Properties.Settings.Default.Save();
    }
}
```

## Design Considerations

### Accessibility

- All themes must maintain sufficient contrast ratios for text readability
- Focus states must be clearly visible in all themes
- Interactive elements should be easily distinguishable
- Theme colors should support Windows high contrast mode

### Performance

- Image assets should be optimized for size
- Use vector graphics (XAML) where possible for resolution independence
- Minimize resource dictionary complexity for faster loading

### Consistency

- Maintain consistent spacing and sizing across themes
- Ensure all UI elements have appropriate styles in all themes
- Use shared base styles where appropriate to reduce duplication

## Animation and Transitions

Each theme includes specific animation profiles:

- **Classic**: Subtle, professional animations (fast fades, minimal motion)
- **Sanrio**: Playful, bouncy animations (elastic easing, character animations)
- **Dark**: Smooth, refined animations (gentle easing, subtle glow effects)

## User Testing

Theme designs should be validated through user testing:

- Initial user feedback on mockups
- Theme switching functionality testing
- Accessibility testing with screen readers
- User preference surveys to gather feedback 
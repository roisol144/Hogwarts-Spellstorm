# Magical Button Styling Guide

This guide explains how to use the new magical button styling system for your Hogwarts Spellstorm main menu.

## Overview

The magical button styling system provides:
- **Magical Materials**: Glowing, mystical button appearances
- **Enhanced Fonts**: Themed fonts for different button types
- **Smooth Animations**: Hover, press, and glow effects
- **Automatic Styling**: Easy application to all menu buttons

## Components

### 1. MagicalButtonStyler.cs
Individual button component that handles:
- Material switching (normal/hover/pressed states)
- Font application
- Animation effects
- Glow effects

### 2. MainMenuStylingManager.cs
Manager component that:
- Automatically finds and styles all buttons
- Applies consistent theming
- Manages fonts and materials globally

### 3. Materials
- **MagicalButtonMaterial.mat**: Normal button state
- **MagicalButtonHoverMaterial.mat**: Hover state with enhanced glow
- **MagicalButtonPressedMaterial.mat**: Pressed state
- **MagicalMenuBackground.mat**: Menu background

## Setup Instructions

### Step 1: Add the Styling Manager
1. Create an empty GameObject in your main menu scene
2. Name it "MenuStylingManager"
3. Add the `MainMenuStylingManager` component

### Step 2: Assign Materials
In the MainMenuStylingManager inspector:
- **Normal Button Material**: Drag `MagicalButtonMaterial`
- **Hover Button Material**: Drag `MagicalButtonHoverMaterial`
- **Pressed Button Material**: Drag `MagicalButtonPressedMaterial`
- **Background Material**: Drag `MagicalMenuBackground` (optional)

### Step 3: Assign Fonts
- **Primary Magical Font**: Use `Socake.otf` for main buttons (Start, Play)
- **Secondary Magical Font**: Use `Cocogoose Pro Italic-trial.ttf` for secondary buttons

### Step 4: Customize Colors
Adjust the text colors in the inspector:
- **Primary Text Color**: White for main buttons
- **Secondary Text Color**: Light gray for secondary buttons
- **Accent Text Color**: Golden for exit/quit buttons

## Automatic Button Detection

The system automatically finds and styles buttons based on their names:

### Primary Buttons (Primary Font)
- Contains "start", "play", "new"
- Gets primary magical font
- White text color

### Secondary Buttons (Secondary Font)
- Contains "tutorial", "scoreboard"
- Gets secondary magical font
- Light gray text color

### Accent Buttons
- Contains "exit", "quit"
- Gets primary font with golden color

## Manual Button Styling

If you want to style a specific button manually:

1. Add `MagicalButtonStyler` component to the button
2. Assign materials in the inspector
3. Set custom font and colors
4. Adjust animation settings

## Customization Options

### Animation Settings
- **Hover Scale**: Button size on hover (default: 1.05)
- **Press Scale**: Button size when pressed (default: 0.95)
- **Animation Duration**: Speed of transitions (default: 0.2s)
- **Glow Pulse Speed**: Glow effect speed (default: 2f)

### Text Effects
- **Text Glow Intensity**: How bright the text glows (default: 1.2)
- **Text Hover Color**: Text color on hover
- **Shadow and Outline**: Automatically added for magical effect

## Troubleshooting

### Buttons Not Styling
1. Ensure `MainMenuStylingManager` is in the scene
2. Check that materials are assigned
3. Verify buttons have `Button` component
4. Check console for errors

### Materials Not Visible
1. Ensure URP/HDRP pipeline is set up correctly
2. Check material shader compatibility
3. Verify render queue settings

### Fonts Not Loading
1. Check font asset references
2. Ensure TextMeshPro is imported
3. Verify font asset settings

## Performance Notes

- The system uses DOTween for smooth animations
- Materials are shared between buttons for efficiency
- Event triggers are automatically managed
- Glow effects can be disabled for performance

## Advanced Usage

### Runtime Material Changes
```csharp
MainMenuStylingManager stylingManager = FindObjectOfType<MainMenuStylingManager>();
stylingManager.SetButtonMaterials(normalMat, hoverMat, pressedMat);
```

### Runtime Font Changes
```csharp
stylingManager.SetFonts(primaryFont, secondaryFont);
```

### Refresh Styling
```csharp
stylingManager.RefreshStyling();
```

## File Structure

```
Assets/
├── Materials/
│   ├── MagicalButtonMaterial.mat
│   ├── MagicalButtonHoverMaterial.mat
│   ├── MagicalButtonPressedMaterial.mat
│   └── MagicalMenuBackground.mat
├── Scripts/
│   ├── MagicalButtonStyler.cs
│   └── MainMenuStylingManager.cs
└── Fonts/
    ├── socake/
    │   └── Socake.otf
    └── cocogoose/
        └── Cocogoose Pro Italic-trial.ttf
```

## Support

For issues or questions about the magical button styling system, check:
1. Console logs for error messages
2. Material inspector settings
3. Font asset configurations
4. Component references in inspector

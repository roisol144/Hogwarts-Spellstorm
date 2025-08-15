# Spell Book UI System

## Overview
The Spell Book UI system allows players to view a spell book by holding the A button on the right Quest controller. The spell book appears as a floating UI in front of the player and follows the camera movement.

## Features
- ✨ **Scene-Specific**: Only works in Chamber of Secrets and Dungeons scenes
- 🎮 **VR Controller Input**: Uses A button on right Quest controller
- 📖 **Spell Book Image**: Displays the SpellBook.png from Resources folder
- 🎯 **Camera Following**: UI follows the player's camera movement
- 🔄 **Hold to Show**: Spell book appears while A button is held, disappears when released
- ❌ **Close Button**: Optional close button to manually hide the spell book

## How It Works

### Input Detection
- Listens for A button press on right Quest controller (`<XRController>{RightHand}/primaryButton`)
- Also supports Oculus Touch controller (`<OculusTouchController>{RightHand}/primaryButton`)
- Uses Unity's new Input System for reliable VR input handling

### Scene Detection
- Automatically detects if the current scene is "ChamberOfSecretsScene" or "DungeonsScene"
- Only responds to input when in allowed scenes
- Logs scene detection status for debugging

### UI Creation
- Creates a world-space canvas that follows the camera
- Loads the SpellBook sprite from `Assets/Resources/SpellBook.png`
- Positions the UI 1.5 units in front of the player
- Scales appropriately for VR viewing

## Setup Instructions

### 1. Script Setup
The `SpellBookUI` script is already added to both scenes:
- **Chamber of Secrets Scene**: Added to GameLevelManager GameObject
- **Dungeons Scene**: Added to GameLevelManager GameObject

### 2. Spell Book Image
Ensure the spell book image exists at:
```
Assets/Resources/SpellBook.png
```

### 3. Testing
1. Open either Chamber of Secrets or Dungeons scene
2. Enter Play mode
3. Hold the A button on the right Quest controller
4. The spell book should appear in front of you
5. Release the A button to hide the spell book

## Configuration Options

### Inspector Settings
- **Spell Book Canvas**: Reference to the UI canvas (auto-created if null)
- **Spell Book Image**: Reference to the image component (auto-created if null)
- **Close Button**: Reference to close button (auto-created if null)
- **Spell Book Sprite**: Reference to the spell book image (auto-loaded from Resources)
- **Target Camera**: Reference to the main camera (auto-detected if null)

### Positioning Settings
- **Offset From Camera**: Position relative to camera (default: 0, 0, 1.5)
- **Follow Speed**: How fast the UI follows camera (default: 8)
- **Look At Camera**: Whether UI faces the camera (default: true)
- **Canvas Scale**: Scale of the world-space canvas (default: 0.004)
- **Canvas Size**: Size of the canvas in pixels (default: 800x600)

### Scene Settings
- **Allowed Scene Names**: Array of scene names where this feature works
  - ChamberOfSecretsScene
  - DungeonsScene

## Debug Features

### Console Logging
The script provides detailed logging:
- Input action setup confirmation
- Scene detection status
- UI creation progress
- Show/hide events

### Context Menu
Right-click on the SpellBookUI component in the inspector to access:
- **Toggle Spell Book**: Manually show/hide the spell book for testing

### Public Methods
- `IsSpellBookVisible()`: Check if spell book is currently visible
- `IsInAllowedScene()`: Check if current scene allows spell book usage
- `ToggleSpellBook()`: Manually toggle spell book visibility

## Troubleshooting

### Spell Book Not Appearing
1. Check console for error messages
2. Verify you're in Chamber of Secrets or Dungeons scene
3. Ensure SpellBook.png exists in Resources folder
4. Check that A button input is working in other parts of the game

### Input Not Working
1. Verify Quest controller is connected and recognized
2. Check that other VR input (like trigger) works in the scene
3. Ensure the script is enabled on the GameLevelManager

### UI Positioning Issues
1. Adjust the `offsetFromCamera` values in the inspector
2. Modify `canvasScale` if the UI appears too large or small
3. Change `followSpeed` if the UI movement feels too fast or slow

## Technical Details

### Input System Integration
- Uses Unity's new Input System for reliable VR input
- Supports both XR Controller and Oculus Touch Controller mappings
- Handles button press and release events separately

### UI Architecture
- World-space canvas for proper VR positioning
- Automatic canvas creation and configuration
- Smooth camera following with configurable speed
- Proper layering with high sorting order (200)

### Performance Considerations
- UI only active when spell book is visible
- Efficient camera following using coroutines
- Automatic cleanup when component is disabled

## Future Enhancements
- Add spell book page turning functionality
- Include spell descriptions and casting instructions
- Add audio feedback when opening/closing
- Support for different spell books per scene
- Add haptic feedback when button is pressed

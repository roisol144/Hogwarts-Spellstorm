# Scene Transition Fader Setup Guide

## Overview
The scene transition fader system provides smooth fade transitions when loading scenes. It's now integrated with the MainMenuManager to provide a polished experience when starting the game.

## How It Works

### Current Implementation
- **MainMenuManager** now uses `SceneTransitionManager` for scene transitions
- **Fade effect** occurs when transitioning from menu to game scenes
- **Automatic detection** of SceneTransitionManager if not manually assigned
- **Fallback system** if SceneTransitionManager is not available

### Scene Transition Flow
```
Player completes setup (Play → Map → Difficulty → Name)
    ↓
StartGameWithSettings() called
    ↓
LoadScene() called with scene name
    ↓
SceneTransitionManager.GoToScene() called
    ↓
FadeScreen.FadeOut() starts fade to black
    ↓
Scene loads after fade duration
    ↓
FadeScreen.FadeIn() fades back to normal
```

## Setup Instructions

### Step 1: Ensure SceneTransitionManager is in MainMenu Scene
1. Open the **MainMenu** scene
2. Verify that the **Scene Transition Manager** GameObject exists
3. Ensure it has the `SceneTransitionManager` component
4. Verify the **Fade Screen** reference is assigned

### Step 2: Configure MainMenuManager (Optional)
1. Select the GameObject with `MainMenuManager` component
2. In the Inspector, find the **"Scene Transition"** section
3. Assign the `SceneTransitionManager` reference (optional - will auto-detect if not assigned)

### Step 3: Verify Scene Build Settings
1. Go to **File → Build Settings**
2. Ensure all scenes are added in the correct order:
   - **MainMenu** (index 0)
   - **TutorialScene** (index 1)
   - **DungeonsScene** (index 2)
   - **ChamberOfSecretsScene** (index 3)
   - Any other scenes...

### Step 4: Test the Transition
1. Start the game from the MainMenu scene
2. Complete the setup flow: Play → Map → Difficulty → Name
3. Click "Save & Play"
4. You should see a smooth fade to black before the game scene loads

## Configuration Options

### FadeScreen Settings
- **Fade Duration**: How long the fade takes (default: 2-3 seconds)
- **Fade Color**: Color of the fade (default: black)
- **Fade Curve**: Animation curve for the fade effect
- **Fade On Start**: Whether to fade in when scene starts

### SceneTransitionManager Settings
- **Fade Screen Reference**: Reference to the FadeScreen component
- **Singleton Pattern**: Automatically manages single instance

## Troubleshooting

### No Fade Effect
1. **Check SceneTransitionManager**: Ensure it exists in the MainMenu scene
2. **Check FadeScreen Reference**: Verify it's assigned in SceneTransitionManager
3. **Check Build Settings**: Ensure scenes are properly added to build settings
4. **Check Console**: Look for warning messages about missing components

### Fade Too Fast/Slow
1. **Adjust Fade Duration**: Modify the fadeDuration in FadeScreen component
2. **Check Fade Curve**: Adjust the animation curve for different timing
3. **Test Different Values**: Try values between 1-5 seconds

### Scene Not Loading
1. **Check Scene Names**: Ensure scene names match exactly (case-sensitive)
2. **Check Build Settings**: Verify scenes are included in build
3. **Check Console**: Look for error messages about missing scenes

### Multiple Fade Screens
1. **Check for Duplicates**: Ensure only one SceneTransitionManager exists
2. **Check Prefabs**: Verify Fader Screen prefab is not duplicated
3. **Use Singleton**: SceneTransitionManager uses singleton pattern

## Integration with Existing Systems

### With Background Music
- Background music will continue playing during fade
- Music will stop when new scene loads
- No interference between systems

### With Game Level Manager
- Difficulty settings are preserved during transition
- Player name is stored in PlayerPrefs
- Game state is maintained

### With VR Systems
- Fade effect works in VR
- No motion sickness issues
- Smooth transition experience

## Code Integration

### Automatic Integration
The MainMenuManager automatically integrates with SceneTransitionManager:

```csharp
// In LoadScene() method
if (sceneTransitionManager != null)
{
    int sceneIndex = GetSceneIndexByName(sceneName);
    if (sceneIndex >= 0)
    {
        sceneTransitionManager.GoToScene(sceneIndex); // Uses fade effect
    }
}
```

### Manual Integration
If you need to use scene transitions in other scripts:

```csharp
// Get the scene transition manager
SceneTransitionManager transitionManager = FindObjectOfType<SceneTransitionManager>();

// Transition to scene by index
transitionManager.GoToScene(sceneIndex);

// Or use async loading
transitionManager.GoToSceneAsync(sceneIndex);
```

### Custom Fade Effects
You can also use the FadeScreen directly:

```csharp
FadeScreen fadeScreen = FindObjectOfType<FadeScreen>();

// Fade out
fadeScreen.FadeOut();

// Fade in
fadeScreen.FadeIn();

// Custom fade
fadeScreen.Fade(0f, 1f); // From transparent to opaque
```

## Testing

### Manual Testing
1. **Start from MainMenu**: Complete full setup flow
2. **Test Tutorial**: Click Tutorial button
3. **Test Direct Scene**: Use SceneTransitionManager directly
4. **Test Fallback**: Disable SceneTransitionManager to test fallback

### Debug Commands
```csharp
// Test scene transition
SceneTransitionManager.Instance.GoToScene(1); // Tutorial scene

// Test fade screen directly
FadeScreen fadeScreen = FindObjectOfType<FadeScreen>();
fadeScreen.FadeOut();
```

### Performance Testing
- **Fade Duration**: Test different durations for optimal feel
- **Memory Usage**: Monitor for memory leaks during transitions
- **VR Performance**: Test in VR for smooth experience

## Best Practices

### Fade Duration
- **Menu to Game**: 2-3 seconds (gives time for scene loading)
- **Quick Transitions**: 1-2 seconds (for fast navigation)
- **VR Considerations**: Longer fades reduce motion sickness

### Scene Loading
- **Async Loading**: Use GoToSceneAsync for large scenes
- **Loading Screens**: Consider adding loading progress indicators
- **Error Handling**: Always provide fallback for missing scenes

### User Experience
- **Consistent Timing**: Use same fade duration throughout game
- **Smooth Curves**: Use smooth animation curves for natural feel
- **Audio Fade**: Consider fading audio during transitions

## Notes
- The system is designed to be non-intrusive and enhance user experience
- Fade effects work in both VR and non-VR modes
- Scene transitions preserve game state and settings
- The system automatically handles missing components gracefully
- No additional setup required if SceneTransitionManager is present in scene

# Background Music System Setup Guide

## Overview
The Background Music System provides comprehensive background music control for the Dungeon and Chamber of Secrets scenes. It features Inspector-controlled volume, fade effects, scene-specific music, and optional UI controls.

## Features

### Core Features
- **Inspector Volume Control**: Adjust volume directly in the Inspector
- **Fade Effects**: Smooth fade-in and fade-out transitions
- **Scene-Specific Music**: Different music for different scenes
- **Loop Support**: Continuous background music
- **Mute Functionality**: Quick mute/unmute capability
- **Singleton Pattern**: Single instance across scenes

### Advanced Features
- **UI Integration**: Optional volume slider and mute button
- **Event System**: Volume and mute change events
- **Context Menu Testing**: Test functions in the Inspector
- **Auto-Setup**: Automatic configuration with BackgroundMusicSetup

## Quick Setup

### Method 1: Automatic Setup (Recommended)
1. Create an empty GameObject in your scene
2. Add the `BackgroundMusicSetup` component
3. Assign your music files in the Inspector:
   - **Default Background Music**: Fallback music
   - **Dungeon Music**: Music for DungeonsScene
   - **Chamber Music**: Music for ChamberOfSecretsScene
4. The system will automatically create and configure the BackgroundMusicManager

### Method 2: Manual Setup
1. Create an empty GameObject named "BackgroundMusicManager"
2. Add the `BackgroundMusicManager` component
3. Assign your background music in the "Background Music" field
4. Configure volume and other settings in the Inspector

## Configuration

### Background Music Manager Settings

#### Basic Settings
- **Background Music**: The main music clip to play
- **Music Audio Source**: AudioSource component (auto-created if null)
- **Music Volume**: Volume level (0-1, adjustable in Inspector)
- **Play On Start**: Whether to start playing automatically
- **Loop Music**: Whether to loop the music continuously

#### Fade Effects
- **Enable Fade In**: Smooth volume increase when starting
- **Fade In Duration**: Time for fade-in effect (seconds)
- **Enable Fade Out**: Smooth volume decrease when stopping
- **Fade Out Duration**: Time for fade-out effect (seconds)

#### Audio Settings
- **Spatial Blend 2D**: True for background music (2D), false for 3D positional
- **Priority**: Audio priority (lower number = higher priority)

### Scene-Specific Configuration
The system automatically detects the current scene and can use different music:
- **DungeonsScene**: Uses dungeon-specific music
- **ChamberOfSecretsScene**: Uses chamber-specific music
- **Other scenes**: Uses default background music

## Usage

### Basic Control
```csharp
// Get the music manager instance
BackgroundMusicManager musicManager = BackgroundMusicManager.Instance;

// Play music
musicManager.PlayMusic();

// Stop music
musicManager.StopMusic();

// Pause/Resume
musicManager.PauseMusic();
musicManager.ResumeMusic();

// Set volume (0-1)
musicManager.SetVolume(0.7f);

// Mute/Unmute
musicManager.SetMuted(true);
musicManager.ToggleMute();
```

### Advanced Control
```csharp
// Change music during runtime
musicManager.ChangeMusic(newMusicClip, true);

// Fade to specific volume
musicManager.FadeToVolume(0.3f, 2f);

// Subscribe to events
musicManager.OnVolumeChanged += (volume) => Debug.Log($"Volume: {volume}");
musicManager.OnMuteChanged += (muted) => Debug.Log($"Muted: {muted}");
```

### UI Integration
If you want to add UI controls:

1. **Volume Slider**: Assign a UI Slider to control volume
2. **Mute Button**: Assign a UI Button to toggle mute
3. **Volume Text**: Assign a TextMeshProUGUI to display volume percentage
4. **Mute Button Images**: Assign sprites for muted/unmuted states

## Audio File Requirements

### Recommended Specifications
- **Format**: .wav, .mp3, or .ogg
- **Duration**: 1-5 minutes (will loop)
- **Quality**: 44.1kHz, 16-bit or higher
- **Channels**: Stereo recommended
- **Volume**: Normalized to avoid clipping

### Harry Potter Theme Suggestions
- **Dungeon Music**: Dark, mysterious, atmospheric
- **Chamber Music**: Eerie, suspenseful, magical
- **General**: Orchestral, magical, immersive

## Testing

### Inspector Testing
Right-click on the BackgroundMusicManager component in the Inspector:
- **Test Play Music**: Start playing music
- **Test Stop Music**: Stop playing music
- **Test Fade To 0**: Fade volume to 0
- **Test Fade To Full**: Fade volume to maximum

### Runtime Testing
```csharp
// Test volume changes
BackgroundMusicManager.Instance.SetVolume(0.5f);

// Test mute functionality
BackgroundMusicManager.Instance.ToggleMute();

// Test fade effects
BackgroundMusicManager.Instance.FadeToVolume(0f, 3f);
```

## Integration with Existing Systems

### With GameAnnouncementAudio
- Background music and announcements work independently
- Music continues playing during announcements
- No conflicts between systems

### With Player Health System
- Music continues during combat
- No interference with damage sounds
- Can be used to create tension during low health

### With Collectible System
- Music provides ambient atmosphere during collection challenges
- No interference with collection sounds

## Troubleshooting

### No Music Playing
1. Check that BackgroundMusicManager exists in the scene
2. Verify that background music clip is assigned
3. Check that AudioSource component is present
4. Ensure volume is not set to 0
5. Check that music is not muted

### Music Too Loud/Quiet
1. Adjust the Music Volume slider in the Inspector
2. Normalize your audio file to appropriate levels
3. Check master audio settings in Unity
4. Use fade effects for gradual volume changes

### Multiple Music Playing
- The system uses singleton pattern to prevent multiple instances
- If you hear multiple music, check for duplicate BackgroundMusicManager objects
- Ensure only one instance exists per scene

### Scene Transition Issues
- BackgroundMusicManager uses DontDestroyOnLoad to persist across scenes
- Music will continue playing when switching scenes
- Use StopMusic() before scene transitions if needed

## Performance Considerations

### Memory Usage
- Audio files are loaded into memory
- Use compressed formats (.mp3, .ogg) for large files
- Consider streaming for very long music files

### CPU Usage
- Fade effects use coroutines (minimal CPU impact)
- Volume changes are real-time
- No performance impact during normal playback

## Best Practices

### Audio Design
1. **Loop Seamlessly**: Ensure music loops without gaps
2. **Appropriate Volume**: Set volume to not overpower sound effects
3. **Theme Consistency**: Use music that fits the Harry Potter theme
4. **Atmospheric**: Choose music that enhances immersion

### Implementation
1. **Use Auto-Setup**: Let BackgroundMusicSetup handle configuration
2. **Test Fade Effects**: Ensure smooth transitions
3. **Monitor Volume**: Keep background music at appropriate levels
4. **Scene-Specific**: Use different music for different environments

## Code Examples

### Event Handling
```csharp
void Start()
{
    BackgroundMusicManager.Instance.OnVolumeChanged += OnMusicVolumeChanged;
    BackgroundMusicManager.Instance.OnMuteChanged += OnMusicMuteChanged;
}

void OnMusicVolumeChanged(float volume)
{
    Debug.Log($"Music volume changed to: {volume}");
}

void OnMusicMuteChanged(bool muted)
{
    Debug.Log($"Music muted: {muted}");
}
```

### Dynamic Music Changes
```csharp
// Change music based on game state
public void OnCombatStarted()
{
    BackgroundMusicManager.Instance.ChangeMusic(combatMusic, true);
}

public void OnCombatEnded()
{
    BackgroundMusicManager.Instance.ChangeMusic(ambientMusic, true);
}
```

### Volume Control
```csharp
// Gradual volume reduction during dialogue
public void StartDialogue()
{
    BackgroundMusicManager.Instance.FadeToVolume(0.3f, 1f);
}

public void EndDialogue()
{
    BackgroundMusicManager.Instance.FadeToVolume(0.7f, 1f);
}
```

## Notes
- The system is designed to be non-intrusive and enhance immersion
- Music automatically starts when the scene loads (if playOnStart is enabled)
- Volume changes are applied immediately in the Inspector
- The system supports both 2D and 3D audio configurations
- No additional code changes are required - just assign audio clips in the Inspector

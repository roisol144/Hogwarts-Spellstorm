# Scoring System Implementation

## Overview

The scoring system provides a VR-friendly way to track and display the player's score in real-time. The system follows the same architectural pattern as the health bar and spell debug systems, ensuring consistency throughout the game.

## Features

- **Kill Scoring**: Configurable points per enemy kill (default: 10 points)
- **VR-Friendly UI**: Score display positioned in the top right corner of the player's view
- **Camera Following**: Score UI smoothly follows the camera like other UI elements
- **Visual Feedback**: Highlight effect when points are gained
- **Audio Feedback**: Optional sound effects for score gains
- **Easy Integration**: Simple static method calls to award points

## System Components

### 1. ScoreManager (`ScoreManager.cs`)
- **Core Component**: Manages the player's score and creates the UI display
- **Singleton Pattern**: Ensures only one instance exists in the scene
- **VR UI Management**: Creates and manages a world-space canvas that follows the camera
- **Event System**: Provides events for score changes and point gains

### 2. ScoreManagerSetup (`ScoreManagerSetup.cs`)
- **Setup Helper**: Automatically creates and configures the ScoreManager
- **Testing Tools**: Provides context menu options for testing the scoring system
- **Configuration**: Allows setting points per kill and other settings

### 3. Editor Tools (`Assets/Code/Scripts/Editor/ScoreManagerSetup.cs`)
- **Editor Window**: "Scoring System > Setup Score Manager" menu option
- **Custom Inspectors**: Enhanced inspector UI for ScoreManager and ScoreManagerSetup
- **Testing Interface**: Play mode testing buttons for score operations

## Quick Setup

### Method 1: Using Editor Tools (Recommended)
1. Go to **Scoring System > Setup Score Manager** in the Unity menu
2. Click **"Create Score Manager GameObject"**
3. Done! The system is ready to use

### Method 2: Using Setup Component
1. Create an empty GameObject in your scene
2. Add the `ScoreManagerSetup` component to it
3. Configure the "Points Per Kill" value (default: 10)
4. The ScoreManager will be created automatically when the scene starts

### Method 3: Manual Setup
1. Create an empty GameObject named "ScoreManager"
2. Add the `ScoreManager` component to it
3. Configure settings in the inspector
4. The UI will be created automatically

## Usage

### Awarding Points for Kills
The scoring system is already integrated into the enemy death system. When enemies die, they automatically call:

```csharp
ScoreManager.NotifyKill(); // Awards points per kill (configurable)
```

### Custom Scoring
You can award custom points for other actions:

```csharp
ScoreManager.NotifyScore(50); // Awards 50 points
```

### Accessing Score Information
```csharp
ScoreManager scoreManager = ScoreManager.Instance;
int currentScore = scoreManager.GetCurrentScore();
int pointsPerKill = scoreManager.GetPointsPerKill();
```

### Score Events
Subscribe to score events for custom behaviors:

```csharp
ScoreManager.Instance.OnScoreChanged += (newScore) => {
    Debug.Log($"Score changed to: {newScore}");
};

ScoreManager.Instance.OnPointsGained += (pointsGained) => {
    Debug.Log($"Gained {pointsGained} points!");
};
```

## Configuration Options

### Score Settings
- **Points Per Kill**: How many points each enemy kill awards (default: 10)
- **Current Score**: The player's current score (read-only in play mode)

### VR UI Positioning
- **Offset From Camera**: Position relative to camera (default: top right)
- **Follow Speed**: How smoothly the UI follows camera movement
- **Canvas Scale**: Size scaling of the world-space UI
- **Canvas Size**: Dimensions of the score display area

### Visual Effects
- **Score Text Color**: Color of the score text (default: golden)
- **Background Colors**: Normal and highlight background colors
- **Highlight Duration**: How long the highlight effect lasts
- **Highlight Curve**: Animation curve for the highlight effect

### Audio
- **Score Gain Sound**: Audio clip played when points are gained
- **Audio Source**: AudioSource component for playing sounds

## UI Layout

The score UI is positioned in the **top right corner** of the player's view:
- **Health Bar**: Center bottom (for player health)
- **Spell Debug**: Left side (for spell casting feedback)
- **Score Display**: **Top right** (for scoring)

This layout ensures all UI elements are visible without overlapping in VR.

## Testing

### In-Editor Testing
1. Enter Play mode
2. Use the ScoreManager inspector buttons:
   - **"Test Kill Score"**: Awards points per kill
   - **"Test Custom Score"**: Awards 50 bonus points
   - **"Reset Score"**: Resets score to 0

### Context Menu Testing
Right-click on ScoreManagerSetup component:
- **"Test Add Kill Score"**
- **"Test Add Custom Score"**
- **"Reset Score"**

### Editor Window Testing
Go to **Scoring System > Setup Score Manager**:
- Use the testing buttons in the Play Mode section

## Integration with Existing Systems

### Enemy Death Integration
The system is already integrated with the enemy death system in `Enemy.cs`:

```csharp
public virtual void Die()
{
    if (isDead) return;
    isDead = true;

    Debug.Log($"Enemy {gameObject.name} died!");

    // Award score for kill
    ScoreManager.NotifyKill(); // ← This line was added

    // ... rest of death logic
}
```

### Consistency with Other UI Systems
The scoring system follows the exact same patterns as:
- **PlayerHealth**: World-space canvas, camera following, similar positioning logic
- **MagicalDebugUI**: Same canvas setup, font loading, visual effects

## Troubleshooting

### Score UI Not Visible
1. Check that ScoreManager exists in the scene
2. Verify the camera is properly assigned
3. Ensure the offset positioning is correct for your camera setup
4. Check canvas sorting order (should be 100)

### Points Not Being Awarded
1. Verify ScoreManager.Instance is not null
2. Check that enemies are calling ScoreManager.NotifyKill()
3. Look for error messages in the console
4. Test with the editor testing buttons

### Performance Issues
1. Ensure only one ScoreManager instance exists (singleton pattern)
2. Check follow speed settings (higher values = more responsive but more processing)
3. Verify canvas scale is appropriate for your scene

## Customization

### Custom Point Values
You can create different point values for different enemy types:

```csharp
// In enemy-specific scripts
public class BossEnemy : Enemy
{
    public override void Die()
    {
        base.Die(); // Calls standard ScoreManager.NotifyKill()
        ScoreManager.NotifyScore(50); // Bonus points for boss
    }
}
```

### Custom UI Styling
Modify the ScoreManager component:
- Change font by placing a TMP font asset in `Resources/Fonts/`
- Adjust colors in the Visual Effects section
- Modify canvas size and positioning

### Custom Sound Effects
1. Assign an audio clip to "Score Gain Sound"
2. The audio will play automatically when points are gained
3. AudioSource component is created automatically if needed

## System Architecture

The scoring system uses the same architectural patterns as other game systems:

1. **Manager Pattern**: ScoreManager handles all score logic
2. **Singleton Pattern**: Ensures global access via ScoreManager.Instance
3. **Event System**: Decoupled communication via events
4. **VR-First Design**: World-space UI that follows the camera
5. **Editor Integration**: Custom inspectors and editor windows for ease of use

This consistency makes the system easy to understand and maintain alongside the existing health bar and spell debug systems. 
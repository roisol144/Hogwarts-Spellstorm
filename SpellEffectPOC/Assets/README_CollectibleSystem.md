# Collectible Challenge System

## Overview

The Collectible Challenge System adds an exciting timed collection feature to the Harry Potter VR experience. Every 5 minutes (starting at 2 minutes from game start), either chocolate frogs or golden eggs spawn at random NavMesh locations. Players have 2 minutes to collect all 4 items for rewards.

## Features

### Timing System
- **First spawn**: 2 minutes after game start
- **Subsequent spawns**: Every 5 minutes
- **Collection timer**: 2 minutes per challenge
- **Items per challenge**: 4 collectibles

### Collectible Types
- **Chocolate Frogs**: Uses your existing `Assets/harry-potter-chocolate-frog-box-cards/source/SceneOverView.prefab`
- **Golden Eggs**: Uses your existing `Assets/harry-potter-golden-egg/source/EggLow/EggLow.prefab`
- **Random selection**: Each challenge spawns either frogs OR eggs

### Scoring System
- **Success**: +200 points for collecting all items in time
- **Failure**: -100 points for not collecting all items
- **Integration**: Uses existing ScoreManager system

### VR-Friendly Features
- **Proximity collection**: Items auto-collect when player gets close
- **Visual effects**: Bobbing, rotation, and glow effects
- **Simple Timer Display**: Shows countdown on left side, mirroring the score display
- **Magical color transitions**: Timer changes from silver to amber to red based on time remaining
- **Audio feedback**: Collection sounds and ambient effects

### UI Layout
- **Score Display**: Right side of screen (existing ScoreManager)
- **Timer Display**: Left side of screen (new TimerUI) - appears only during challenges
- **Consistent Design**: Both use same world-space canvas approach for VR comfort

### Timer Features
- **🕐 Simple Countdown**: Shows time remaining in MM:SS format
- **🎨 Color Transitions**: Silver (normal) → Amber (warning) → Red (critical)
- **📍 Perfect Positioning**: Positioned opposite to score for balanced UI
- **✨ Magical Styling**: Uses mystical blue background and enchanted silver text
- **🔧 Auto-Setup**: Created automatically when collectible system starts

## System Components

### 1. CollectibleSpawner (`CollectibleSpawner.cs`)
**Main system controller** - Manages spawn timing and challenge creation.

### 2. CollectibleChallenge (`CollectibleChallenge.cs`)
**Individual challenge manager** - Handles timer, collection tracking, and success/failure.

### 3. Collectible (`Collectible.cs`)
**Individual item behavior** - Handles collection detection and visual effects.

### 4. CollectibleSetup (`CollectibleSetup.cs`)
**Setup helper** - Automatically configures your existing prefabs as collectibles.

### 5. TimerUI (`TimerUI.cs`)
**Simple timer display** - Shows challenge countdown on the left side of screen (opposite to score), with magical color transitions.

### 6. CollectibleUI (`CollectibleUI.cs`)
**Advanced magical interface** - Optional complex UI with full theming and effects (can be used instead of TimerUI if desired).

## Setup Instructions

### Step 1: Prepare Your Existing Prefabs

1. **Chocolate Frog Setup:**
   - Navigate to `Assets/harry-potter-chocolate-frog-box-cards/source/SceneOverView.prefab`
   - Open the prefab for editing
   - Add the `CollectibleSetup` component
   - Configure the settings (collection radius, bobbing, etc.)
   - Save the prefab

2. **Golden Egg Setup:**
   - Navigate to `Assets/harry-potter-golden-egg/source/EggLow/EggLow.prefab`
   - Open the prefab for editing
   - Add the `CollectibleSetup` component
   - Configure the settings (collection radius, bobbing, etc.)
   - Save the prefab

### Step 2: Scene Integration

1. **Add CollectibleSpawner:**
   ```csharp
   // In Unity, create empty GameObject named "CollectibleSpawner"
   // Add Component: CollectibleSpawner
   // Assign your configured chocolate frog prefab
   // Assign your configured golden egg prefab
   ```

2. **Add CollectibleUI:**
   ```csharp
   // Create empty GameObject named "CollectibleUI"  
   // Add Component: CollectibleUI
   ```

### Step 3: Configuration

In the **CollectibleSpawner** inspector:
- **Chocolate Frog Prefab**: Drag `SceneOverView.prefab` (with CollectibleSetup added)
- **Golden Egg Prefab**: Drag `EggLow.prefab` (with CollectibleSetup added)
- Set **Spawn Center** to match your map center
- Set **Spawn Radius** to cover your playable area
- Optionally add audio clips for extra immersion

## How It Works

### Automatic Setup Process
1. When a prefab with `CollectibleSetup` is instantiated, it automatically:
   - Adds the `Collectible` component
   - Configures all collectible settings
   - Adds necessary colliders and audio sources
   - Sets up proper tagging

2. The `CollectibleSpawner` creates instances of your prefabs
3. `CollectibleSetup` runs and configures each instance as a collectible
4. Players can then collect the items by walking close to them

### No Manual Prefab Modification Required
- Your original asset files remain unchanged
- All collectible functionality is added dynamically at runtime
- Easy to adjust settings without modifying source assets

## Configuration Options

### CollectibleSetup Settings (per prefab)

**Collection:**
- `collectionRadius`: Auto-collect distance (default: 2.5 meters)
- `autoCollectOnProximity`: Enable proximity collection

**Visual Effects:**
- `enableBobbing`: Up/down animation
- `bobHeight`: Animation amplitude (default: 0.2)
- `bobSpeed`: Animation speed (default: 1.5)
- `enableRotation`: Spinning animation
- `rotationSpeed`: Rotation speed (default: 30)
- `glowIntensity`: Emission glow strength (default: 1.2)

**Audio:**
- `collectionSound`: Sound when collected
- `ambientSound`: Optional ambient sound while active

### CollectibleSpawner Settings

**Timing Configuration:**
- `initialDelay`: Time until first spawn (default: 2 minutes)
- `spawnInterval`: Time between spawns (default: 5 minutes)
- `collectionTimeLimit`: Time to collect items (default: 2 minutes)

**Spawn Configuration:**
- `itemsPerChallenge`: Number of items (default: 4)
- `minDistanceBetweenItems`: Minimum spacing (default: 5 meters)
- `spawnRadius`: Area radius (default: 50 meters)

**Scoring:**
- `successPoints`: Reward for success (default: 200)
- `failurePoints`: Penalty for failure (default: 100)

**UI Messages:**
- `announcementDuration`: How long challenge announcements stay on screen in seconds (default: 5)
  - Used for: "Challenge Started!", "Challenge Complete!", "Challenge Failed!" messages
  - Progress messages ("Collectible found!") use 2 seconds for quick feedback

## Magical UI Configuration

### Customizing the Harry Potter Theme
In the `CollectibleUI` component, you can adjust the magical appearance:

**Magical Colors:**
- `magicalGold`: Main accent color (default: Hogwarts gold)
- `mysticalBlue`: Background tint (default: Deep magical blue)
- `enchantedSilver`: Timer text color (default: Magical silver)
- `warningAmber`: Warning state color (default: Magical amber)
- `criticalCrimson`: Urgent timer color (default: Deep red)
- `successEmerald`: Progress bar color (default: Magical green)

**Magical Effects:**
- `shimmerSpeed`: Speed of border color transitions (default: 2)
- `floatAmount`: How much the UI gently floats (default: 0.1)
- `warningTimeThreshold`: When timer turns amber (default: 30 seconds)
- `criticalTimeThreshold`: When timer becomes urgent (default: 10 seconds)

**UI Positioning:**
- `offsetFromCamera`: Position relative to player camera (default: subtle left side)
- `followSpeed`: How smoothly UI follows camera (default: 3 for graceful movement)
- `canvasScale`: Overall UI size (default: 0.002 for subtle appearance)

### Example Magic Messages
The UI automatically chooses thematic messages:
- **Start**: *"Seek the 4 Chocolate Frogs..."*
- **Collection**: *"Brilliant! Your magical prowess shows..."*
- **Success**: *"Extraordinary! You've mastered the challenge!"*
- **Failure**: *"The magic fades... better luck next time."*

## Easy Testing

### Quick Testing Setup
1. **Reduce timers for testing:**
   - Set `initialDelay` to 10 seconds
   - Set `collectionTimeLimit` to 30 seconds
   - Set `spawnInterval` to 60 seconds

2. **Manual trigger:**
   - Right-click on CollectibleSpawner component
   - Select "Trigger Challenge Now"

3. **Test the timer display:**
   - **Timer appears**: When challenge starts (left side of screen)
   - **Timer counts down**: From 2:00 to 0:00 with color changes
   - **Timer disappears**: When challenge ends (success or failure)

4. **Test announcements:**
   - **"Challenge Started!"**: Should stay visible for the configured duration (default: 5 seconds)
   - **Adjust**: Change `announcementDuration` in CollectibleSpawner inspector
   - **Progress**: "Collectible found!" messages show for 2 seconds

5. **Test collection:**
   - Walk close to spawned items
   - Watch for auto-collection
   - Monitor timer countdown

## Troubleshooting

### Common Issues

**Collectibles not spawning:**
- Ensure your prefabs have the `CollectibleSetup` component
- Check that prefabs are assigned in CollectibleSpawner
- Verify NavMesh is baked in your scene

**Collection not working:**
- Check that CollectibleSetup has proper collection radius
- Verify player camera is tagged "MainCamera"
- Ensure Collectible component was added automatically

**Visual effects not working:**
- Check that your prefabs have renderers
- Verify glow intensity settings
- Make sure materials support emission

**Performance issues:**
- Reduce proximity check frequency in Collectible settings
- Limit number of active collectibles
- Optimize glow effects

## Integration Benefits

### Using Your Existing Assets
- ✅ **No asset modification**: Original prefabs remain untouched
- ✅ **Runtime configuration**: Everything set up automatically
- ✅ **Easy updates**: Change settings without touching source files
- ✅ **Modular design**: Add/remove collectible functionality easily

### Backward Compatibility
- Your existing chocolate frog and golden egg assets work as-is
- No need to recreate or manually edit complex prefabs
- Maintains all original visual fidelity and materials
- Preserves any existing animations or effects

This approach respects your existing assets while adding the collectible functionality seamlessly! 
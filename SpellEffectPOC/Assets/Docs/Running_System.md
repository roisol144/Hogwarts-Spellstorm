# Running System

## Overview
The Running System allows players to move faster by holding the **Left Grip** button on their VR controller. It's designed to feel natural and not interfere with other systems like flying.

## How to Use

### Running
- **Input**: **Hold the Left Grip button** on your VR controller
- **Effect**: Movement speed increases from **2 m/s** to **4 m/s** (2x faster)
- **Transition**: Smooth speed transition over **0.3 seconds** for comfort

### Stopping
- **Input**: **Release the Left Grip button**
- **Effect**: Returns to normal walking speed smoothly

## Features

### ✅ **Smart Integration**
- **Compatible with Flying**: Automatically detects when you're flying with the Nimbus 2000 and doesn't interfere
- **Smooth Transitions**: No jarring speed changes - everything feels natural
- **VR Optimized**: Uses the same input system as other VR interactions

### ✅ **Configurable Settings**
- **Normal Speed**: 2 m/s (walking pace)
- **Running Speed**: 4 m/s (moderate run, not too fast for VR comfort)
- **Transition Time**: 0.3 seconds (smooth speed changes)

### ✅ **Audio Support**
- **Running Sound**: Optional audio clip plays while running
- **Looped Audio**: Sound automatically starts/stops with running

## Technical Details

### Input Mapping
- **Left Hand Grip**: `<XRController>{LeftHand}/gripPressed`
- **Oculus Support**: `<OculusTouchController>{LeftHand}/gripPressed`

### System Integration
- **Movement Provider**: Uses `ActionBasedContinuousMoveProvider`
- **Flying Detection**: Automatically detects flying mode (speed > 10 m/s)
- **No Conflicts**: Designed to work alongside existing movement systems

## Usage Tips

1. **Hold for Continuous Running**: Keep the grip button held down to maintain running speed
2. **Works While Moving**: You can start/stop running while already moving
3. **VR Comfort**: The 2x speed increase is designed to be comfortable for most VR users
4. **Flying Priority**: When flying with the Nimbus, running is automatically disabled

## Customization

In the Unity Inspector, you can adjust:
- **Normal Walk Speed**: Base movement speed
- **Running Speed**: How fast you run
- **Speed Transition Time**: How quickly speeds change
- **Running Sound**: Audio clip to play while running

The system automatically adapts to your existing movement settings! 
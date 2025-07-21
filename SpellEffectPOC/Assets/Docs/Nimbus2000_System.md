# Nimbus 2000 Flying System

## Overview
The Nimbus 2000 system allows players to summon a flying broomstick to their left hand and enables fast flying movement in VR.

## How It Works

### Summoning the Nimbus
- **Input**: Press the **Left Trigger** button on your VR controller
- **Action**: The Nimbus 2000 broomstick appears in your left hand
- **Effect**: Flying mode is automatically enabled

### Flying Mode
When the Nimbus is summoned:
- **Movement Speed**: Increases from 2 m/s to 15 m/s (7.5x faster!)
- **Flying**: Gravity is disabled, allowing true 3D movement
- **Controls**: Use the normal VR movement controls (thumbsticks) to fly in any direction

### Dismissing the Nimbus
- **Input**: Press the **Left Trigger** button again
- **Action**: The Nimbus disappears
- **Effect**: Returns to normal walking mode with gravity

## Technical Details

### Components
- **NimbusController.cs**: Main script handling input and mode switching
- **Nimbus2000.prefab**: The 3D model of the broomstick
- **Nimbus2000Material.mat**: Material with realistic textures

### Settings (Configurable in Inspector)
- **Normal Move Speed**: 2 m/s (walking)
- **Flying Move Speed**: 15 m/s (flying)
- **Nimbus Offset**: Position relative to left hand
- **Gravity When Flying**: Disabled for true flight

### Input Mapping
- **Left Hand Trigger**: `<XRController>{LeftHand}/triggerPressed`
- **Oculus Support**: `<OculusTouchController>{LeftHand}/triggerPressed`

## Setup
1. The system is already configured in the YardScene
2. The NimbusController is attached to a GameObject in the scene
3. The Nimbus2000 prefab is assigned to the controller
4. No additional setup required!

## Usage Tips
- **Quick Toggle**: Tap the left trigger to quickly switch between walking and flying
- **Emergency Landing**: If you get stuck flying, tap the left trigger to return to walking mode
- **Speed Control**: The same movement inputs work for both walking and flying
- **Hand Position**: The Nimbus follows your left hand naturally

## Troubleshooting
- **Nimbus not appearing**: Check that the left hand controller is tracked
- **Flying not working**: Ensure the ActionBasedContinuousMoveProvider is present on the XR Rig
- **Input not responding**: Verify VR controllers are connected and tracked

## Future Enhancements
- Sound effects for summoning/dismissing
- Particle effects during summoning
- Visual trail while flying
- Different flight speeds based on hand position 
# Collectible Audio Setup Guide

## Overview
The collectible system now supports playing a sound when all items are collected successfully. This implementation integrates with the existing `GameAnnouncementAudio` system for consistency.

## Setup Instructions

### Step 1: Locate the GameAnnouncementAudio GameObject
In both **ChamberOfSecretsScene** and **DungeonsScene**, find the GameObject named `GameAnnoucementAudio` (note: there's a typo in the name, but it's the correct object).

### Step 2: Add the Audio Clip
1. Select the `GameAnnoucementAudio` GameObject
2. In the Inspector, find the **GameAnnouncementAudio** component
3. Locate the **All Items Collected Announcement** field in the "Announcement Audio Clips" section
4. Drag and drop your audio file into this field

### Step 3: Audio File Requirements
- **Format**: .wav, .mp3, or .ogg
- **Recommended**: Short sound effect (1-3 seconds)
- **Volume**: Normalized to avoid being too loud
- **Type**: Success/fanfare sound that fits the Harry Potter theme

### Step 4: Testing
1. Start the game in either scene
2. Wait for collectible items to spawn (2 minutes after start)
3. Collect all 4 items within the time limit
4. You should hear the success sound when all items are collected

## How It Works

### Primary Audio System (Recommended)
- Uses `GameAnnouncementAudio.PlayAllItemsCollectedAnnouncement()`
- Integrates with existing announcement system
- Consistent with other game audio
- Supports 2D/3D audio configuration

### Fallback Audio System
- Uses the `CollectibleSpawner`'s local AudioSource
- Configured via the `challengeSuccessSound` field
- Only plays if GameAnnouncementAudio is not available

## Troubleshooting

### No Sound Playing
1. Check that the audio clip is assigned in the GameAnnouncementAudio component
2. Verify the AudioSource component is enabled
3. Check the volume settings in the GameAnnouncementAudio component
4. Ensure the audio file is not corrupted

### Sound Too Loud/Quiet
1. Adjust the volume in the GameAnnouncementAudio component
2. Normalize your audio file to appropriate levels
3. Check the master audio settings in Unity

### Multiple Sounds Playing
- The system is designed to play only one sound
- If you hear multiple sounds, check that you haven't assigned the same clip to both the GameAnnouncementAudio and the CollectibleSpawner

## Code Integration

The implementation automatically calls the audio when all items are collected:

```csharp
// In CollectibleSpawner.OnChallengeCompletedHandler()
if (success)
{
    // Play success sound using GameAnnouncementAudio system (preferred)
    GameAnnouncementAudio.PlayAllItemsCollectedAnnouncement();
    
    // Fallback to local audio source if GameAnnouncementAudio doesn't have the sound
    if (challengeSuccessSound != null && audioSource != null)
    {
        audioSource.PlayOneShot(challengeSuccessSound);
    }
}
```

## Notes
- The audio system is designed to be non-intrusive and fit the magical theme
- The sound plays immediately when the last item is collected
- No additional code changes are required - just assign the audio clip in the Inspector

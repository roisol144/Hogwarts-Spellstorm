# Attack Audio System Setup Guide

## Overview
The attack audio system provides comprehensive sound feedback for both player damage and enemy attacks. It integrates with the existing `GameAnnouncementAudio` system for consistency and includes fallback options.

## Audio Components

### 1. Player Damage Audio
- **Primary System**: `GameAnnouncementAudio.PlayPlayerAttackedAnnouncement()`
- **Fallback System**: `PlayerHealth.damageSound` field
- **Trigger**: When player takes damage from any source
- **Features**: Haptic feedback, visual effects, damage indicators

### 2. Enemy Attack Audio
- **Attack Charge Sound**: `EnemyAttack.attackChargeSound` - plays during warning phase
- **Attack Execute Sound**: `EnemyAttack.attackSound` - plays when damage is dealt
- **Trigger**: When enemies attack the player
- **Features**: Different sounds for different attack phases

## Setup Instructions

### Step 1: Player Damage Audio Setup

#### Option A: GameAnnouncementAudio (Recommended)
1. In both **ChamberOfSecretsScene** and **DungeonsScene**, find the `GameAnnoucementAudio` GameObject
2. In the Inspector, locate the **"Player Attacked Announcement"** field
3. Drag and drop your player damage audio file into this field

#### Option B: PlayerHealth Fallback
1. Find the GameObject with `PlayerHealth` component (usually the main camera)
2. In the Inspector, locate the **"Damage Sound"** field
3. Drag and drop your player damage audio file into this field

### Step 2: Enemy Attack Audio Setup

#### For Each Enemy Type:
1. Select the enemy GameObject (Basilisk, Dementor, Troll, etc.)
2. Find the `EnemyAttack` component
3. In the "Attack Effects" section:
   - **Attack Charge Sound**: Sound that plays when enemy starts charging attack
   - **Attack Sound**: Sound that plays when enemy actually deals damage
4. Ensure the **Audio Source** field is assigned

### Step 3: Audio File Requirements

#### Player Damage Sound:
- **Format**: .wav, .mp3, or .ogg
- **Duration**: 0.5-2 seconds (short impact sound)
- **Type**: Pain/hurt sound, impact sound, or magical damage sound
- **Volume**: Normalized to avoid being too loud

#### Enemy Attack Sounds:
- **Charge Sound**: Warning/charging sound (0.5-1 second)
- **Attack Sound**: Impact/hit sound (0.3-1 second)
- **Type**: Should match enemy theme (roar for basilisk, magical for dementor, etc.)

## How It Works

### Player Damage Audio Flow:
```
Enemy attacks player
    ↓
PlayerHealth.TakeDamage() called
    ↓
GameAnnouncementAudio.PlayPlayerAttackedAnnouncement() (primary)
    ↓
PlayerHealth.damageSound.PlayOneShot() (fallback)
    ↓
Haptic feedback + visual effects
```

### Enemy Attack Audio Flow:
```
Enemy starts attack
    ↓
AttackWarningPhase() starts
    ↓
attackChargeSound.PlayOneShot() (warning sound)
    ↓
AttackExecutePhase() starts
    ↓
DealDamageToPlayer() called
    ↓
attackSound.PlayOneShot() (impact sound)
    ↓
PlayerHealth.TakeDamage() called
    ↓
Player damage sound plays
```

## Enemy-Specific Audio Recommendations

### Basilisk
- **Charge Sound**: Deep growl or hiss
- **Attack Sound**: Sharp impact or magical burst
- **Player Damage**: Painful impact or magical damage

### Dementor
- **Charge Sound**: Eerie wind or soul-draining sound
- **Attack Sound**: Magical drain or ethereal impact
- **Player Damage**: Cold or soul-draining effect

### Troll
- **Charge Sound**: Heavy breathing or grunt
- **Attack Sound**: Heavy impact or club swing
- **Player Damage**: Heavy impact or crushing sound

### Default Enemy
- **Charge Sound**: Generic warning sound
- **Attack Sound**: Standard impact sound
- **Player Damage**: Standard damage sound

## Testing

### Test Player Damage Audio:
1. Start the game
2. Let an enemy attack you
3. You should hear the damage sound immediately when taking damage
4. Check that haptic feedback works on VR controllers

### Test Enemy Attack Audio:
1. Approach an enemy
2. Wait for them to start attacking
3. You should hear the charge sound during warning phase
4. You should hear the attack sound when damage is dealt

### Debug Commands:
```csharp
// Test player damage
PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
playerHealth.TakeDamage(20f);

// Test enemy attack
EnemyAttack enemyAttack = FindObjectOfType<EnemyAttack>();
enemyAttack.StartAttack();
```

## Troubleshooting

### No Player Damage Sound:
1. Check that audio clip is assigned in GameAnnouncementAudio
2. Verify PlayerHealth has AudioSource component
3. Check volume settings in both systems
4. Ensure audio file is not corrupted

### No Enemy Attack Sound:
1. Check that enemy has AudioSource component
2. Verify attack sounds are assigned in EnemyAttack component
3. Check that enemy is actually attacking (not just moving)
4. Ensure audio file is not corrupted

### Multiple Sounds Playing:
- The system is designed to play appropriate sounds at appropriate times
- If you hear overlapping sounds, check that you haven't assigned the same clip to multiple fields

### Sound Too Loud/Quiet:
1. Adjust volume in AudioSource components
2. Normalize your audio files to appropriate levels
3. Check master audio settings in Unity

## Integration with Existing Systems

### With Protego Shield:
- When Protego shield is active, no damage sounds play
- Enemy attacks are blocked silently

### With Health Regeneration:
- Damage sounds play independently of health regeneration
- Low health heartbeat sound plays separately

### With Damage Indicators:
- Audio plays simultaneously with visual damage indicators
- Both systems provide feedback for the same damage event

## Code Integration

The implementation automatically handles audio when attacks occur:

```csharp
// In PlayerHealth.TakeDamage()
GameAnnouncementAudio.PlayPlayerAttackedAnnouncement(); // Primary
if (damageSound != null && audioSource != null)
{
    audioSource.PlayOneShot(damageSound); // Fallback
}

// In EnemyAttack.AttackWarningPhase()
if (attackChargeSound != null && audioSource != null)
{
    audioSource.PlayOneShot(attackChargeSound);
}

// In EnemyAttack.DealDamageToPlayer()
if (attackSound != null && audioSource != null)
{
    audioSource.PlayOneShot(attackSound);
}
```

## Notes
- The audio system is designed to be immersive and provide clear feedback
- Sounds play immediately when events occur
- Haptic feedback provides additional tactile feedback in VR
- The system supports both 2D and 3D audio for different effects
- No additional code changes are required - just assign audio clips in the Inspector

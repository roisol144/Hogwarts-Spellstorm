# Health and Damage System Implementation

## Overview

This health and damage system implements the following features:
- **Regular damage**: Fireball spells deal 1/3 of enemy's current health per hit
- **Special damage**: Impact01 spells (from special spell recognition) instantly kill enemies
- **Health bar**: Appears above enemies when taking damage, showing current health percentage
- **Visual feedback**: Health bar color changes from green → yellow → red based on health percentage

## System Components

### 1. Enemy Health System (`Enemy.cs`)
- Manages enemy health, damage calculation, and death
- Handles health bar display and positioning
- Supports both regular damage (1/3 current health) and special damage (instant kill)

### 2. Health Bar UI (`HealthBar.cs`)
- Visual health bar that appears above enemies
- Auto-hides after 3 seconds of no damage
- Color-coded: Green (high health) → Yellow (medium) → Red (low health)
- Faces the player camera for optimal visibility

### 3. Projectile Scripts
- **`FireballProjectile.cs`**: Regular damage projectile (1/3 health damage)
- **`Impact01Projectile.cs`**: Special damage projectile (instant kill)

### 4. Spell Casting Manager (`SpellCastingManager.cs`)
- **Trigger press**: Shoots fireball (regular damage)
- **Special spell recognition**: Shoots Impact01 (instant kill)
- All voice+gesture combinations are considered special spells

## How It Works

### Damage System
1. **Regular hits (Fireball)**:
   - Each hit deals 1/3 of the enemy's **maximum** health
   - Takes exactly 3 hits to kill any enemy
   - First hit: 100% → 66.7% (33.3 damage)
   - Second hit: 66.7% → 33.3% (33.3 damage)
   - Third hit: 33.3% → 0% (33.3 damage = death)

2. **Special hits (Impact01)**:
   - Instantly kills the enemy regardless of current health
   - Triggered by any special spell recognition (voice + gesture)

### Health Bar Display
- Appears when enemy takes damage
- Shows current health as a percentage
- Auto-hides after 2 seconds
- Floats above the enemy's head and follows them
- Always faces the player camera
- Color changes based on health percentage:
  - 60%+ : Green
  - 30-60%: Yellow  
  - <30%: Red

## Setup Instructions

### 1. Enemy Setup
1. Add the `Enemy` script to enemy GameObjects
2. Tag enemies with "Enemy" tag
3. In the Enemy component inspector:
   - Set `Max Health` (default: 100)
   - Assign `Health Bar Prefab` → drag `Assets/Prefabs/HealthBarPrefab.prefab`
   - Assign `Death Effect Prefab` (optional explosion effect)
   - `Player Camera` will auto-detect Camera.main

### 2. Spell Prefab Setup

#### Fireball Prefab
1. Open `Assets/Spell Effects/FireBall.prefab`
2. Remove the old `FireballProjectile` script if it exists
3. Add the new `FireballProjectile` script
4. Configure settings in inspector

#### Impact01 Prefab  
1. Open `Assets/Spell Effects/Impact01.prefab`
2. Remove any existing projectile script
3. Add the new `Impact01Projectile` script
4. Configure settings in inspector

### 3. Spell Casting Manager Setup
1. The `SpellCastingManager` is already configured
2. Ensure these prefabs are assigned:
   - `Fireball Prefab` → `Assets/Spell Effects/FireBall.prefab`
   - `Impact01 Prefab` → `Assets/Spell Effects/Impact01.prefab`

## Testing

### Test Regular Damage
1. Press trigger to shoot fireball at enemy
2. Health bar should appear showing reduced health
3. Repeat 3 times to kill enemy

### Test Special Damage  
1. Perform voice + gesture combination (e.g., say "Stupefy" + gesture)
2. Impact01 should fire and instantly kill enemy

### Test Health Bar
1. Hit enemy once and observe health bar
2. Wait 2 seconds - health bar should disappear
3. Hit again - health bar should reappear with updated color

## Troubleshooting

### Health Bar Not Appearing
- Check if `HealthBarPrefab` is assigned in Enemy component
- Verify enemy has "Enemy" tag
- Check if Camera.main exists in scene

### Wrong Damage Values
- Regular hits should always deal 1/3 of current health
- Special hits should always instant kill
- Check projectile scripts are correctly assigned to prefabs

### Spells Not Working
- Verify prefabs are assigned in SpellCastingManager
- Check if projectile scripts are attached to spell prefabs
- Ensure enemies are tagged as "Enemy"

## Files Modified/Created

### New Files
- `Assets/Code/Scripts/HealthBar.cs` - Health bar UI component
- `Assets/Code/Scripts/Impact01Projectile.cs` - Special damage projectile
- `Assets/Prefabs/HealthBarPrefab.prefab` - Health bar UI prefab

### Modified Files
- `Assets/Code/Scripts/Enemy.cs` - Added health system
- `Assets/Code/Scripts/FireballProjectile.cs` - Updated damage system
- `Assets/Code/Scripts/SpellCastingManager.cs` - Updated spell selection logic

## Features Summary

✅ Regular hits (fireball) deal 1/3 of enemy health
✅ Special hits (Impact01) instantly kill enemies  
✅ Health bar appears above enemies when damaged
✅ Health bar shows current health percentage
✅ Health bar color-codes health status
✅ Health bar auto-hides after 3 seconds
✅ Health bar always faces player camera
✅ Trigger fires fireball (regular damage)
✅ Special spell recognition fires Impact01 (instant kill)
✅ All voice+gesture combinations are special spells 
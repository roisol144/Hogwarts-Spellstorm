# Basilisk Enemy Setup - Matching Dementor Configuration

## Overview

The basilisk has been configured to use the exact same health-damage system as the dementor, with unique visual effects to distinguish between the two enemy types.

## Basilisk Configuration

### Health System (Identical to Dementor)
- **100 HP** maximum health
- **3 regular hits** (fireballs) to kill - each does 33.33 damage
- **1 special attack** (Impact01) = instant kill
- **Health bar** appears when taking damage and follows the enemy

### Basilisk-Specific Features
- **Basilisk.cs script** - inherits from Enemy class
- **Green dissolve effect** - distinguishable from dementor's purple
- **Same dissolve material** but with custom edge color
- **Bottom-to-top dissolve direction**
- **NavMeshAgent** for movement
- **CapsuleCollider** for collision detection
- **"Enemy" tag** for spell targeting

### Components Added to Basilisk Prefab
1. **Basilisk Script** (replaces Enemy script)
   - Health Bar Prefab: Same as dementor
   - Death Effect Prefab: Same as dementor
   - Dissolve Material: Same shader, green edge color
   - Death Sound: Same audio clip

2. **NavMeshAgent**
   - Radius: 6
   - Speed: 2
   - Height: 20

3. **CapsuleCollider**
   - Radius: 6
   - Height: 20
   - Center: (0, 10, 0)

4. **Enemy Tag** - enables spell targeting

## Enemy Spawner Configuration

### New Spawn Point System
The EnemySpawner has been updated to support specific enemy types per spawn point:

**SpawnPoint1** (Mixed):
- Position: (-12.02, 0, -12.48)
- Enemies: Dementor AND Basilisk (random selection)

**SpawnPoint2** (Basilisk Only):
- Position: (12.5, 0, -8.2)
- Enemies: Basilisk only

### How It Works
- The spawner will randomly select a spawn point configuration
- For each spawn point, it randomly selects from the configured enemy types
- This allows for varied enemy encounters and specific creature territories

## Visual Differences

| Feature | Dementor | Basilisk |
|---------|----------|----------|
| Dissolve Edge Color | Purple (0.21, 0.71, 0.78) | Green (0.2, 0.8, 0.3) |
| Model | Floating wraith | Serpent creature |
| Scale | 0.15x | 100x |
| Audio | Same death sound | Same death sound |
| Health System | 100 HP, 3 hits | 100 HP, 3 hits |

## Testing
- Both enemies take exactly 3 fireball hits to kill
- Both show health bars when damaged
- Both have portal spawn effects
- Both dissolve with shader effects on death
- Different visual appearances and dissolve colors distinguish them

The basilisk is now fully configured and ready for gameplay! 
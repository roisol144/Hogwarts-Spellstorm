# Player Health System with Damage Indicators

This system provides a comprehensive health management solution for the player with Call of Duty-style damage indicators and enemy attack mechanics.

## Features

### Player Health System
- **Health Management**: Configurable max health, current health tracking
- **Health Regeneration**: Automatic health regeneration after taking damage
- **Visual Health Bar**: Dynamic health bar with color changes based on health percentage
- **Low Health Effects**: Pulsing health bar and heartbeat audio when health is low
- **Damage Effects**: Screen flash and overlay effects when taking damage
- **Death/Revival System**: Complete death handling with revival capabilities

### Damage Indicators (COD-style)
- **Directional Indicators**: Red arrows pointing toward damage sources
- **Screen Flash**: Brief red screen flash when taking damage
- **Dynamic Scaling**: Indicator size scales with damage amount
- **Fade Animation**: Smooth fade-in and fade-out animations
- **Multiple Indicators**: Support for multiple simultaneous damage sources

### Enemy Attack System
- **Three Attack Types**:
  - **Melee**: Close-range lunge attacks
  - **Projectile**: Ranged projectile attacks
  - **Area**: Area-of-effect attacks
- **Attack Animations**: Visual feedback with color changes and warning phases
- **Attack Indicators**: Visual attack telegraphs for player awareness
- **Configurable Parameters**: Damage, range, cooldown, and animation settings

## Quick Setup

### Automatic Setup (Recommended)
1. Add the `PlayerHealthSetup` component to any GameObject in your scene
2. Configure the settings in the inspector
3. The system will automatically:
   - Attach `PlayerHealth` to the main camera
   - Create damage indicator UI
   - Add `EnemyAttack` components to existing enemies
   - Configure everything based on enemy types

### Manual Setup
1. Add `PlayerHealth` component to your player/camera object
2. Add `DamageIndicator` component to any GameObject (it's a singleton)
3. Add `EnemyAttack` components to enemy objects
4. Configure parameters in the inspector

## Usage

### Taking Damage
```csharp
// Basic damage
playerHealth.TakeDamage(25f);

// Damage with source position (shows directional indicator)
playerHealth.TakeDamage(25f, enemyPosition);
```

### Healing
```csharp
playerHealth.Heal(50f);
```

### Manual Damage Indicators
```csharp
// Show indicator pointing toward damage source
DamageIndicator.Instance.ShowDamageIndicator(damageSourcePosition, damageAmount);

// Show indicator in specific screen direction
DamageIndicator.Instance.ShowDamageIndicator(screenDirection, damageAmount);
```

### Enemy Attacks
```csharp
// Start manual attack
enemyAttack.StartAttack();

// Configure attack settings
enemyAttack.SetAttackDamage(30f);
enemyAttack.SetAttackRange(5f);
enemyAttack.SetAttackCooldown(3f);
```

## Configuration

### PlayerHealth Settings
- **Max Health**: Starting and maximum health value
- **Health Regen Rate**: Health points regenerated per second
- **Health Regen Delay**: Delay before regeneration starts after damage
- **Low Health Threshold**: Health percentage that triggers low health effects
- **Damage Flash Duration**: Duration of damage overlay effect

### DamageIndicator Settings
- **Indicator Distance**: Distance from screen center to place indicators
- **Indicator Fade Duration**: How long indicators take to fade out
- **Indicator Color**: Color of damage direction arrows
- **Flash Color**: Color of screen flash effect
- **Flash Duration**: Duration of screen flash

### EnemyAttack Settings
- **Attack Damage**: Damage dealt to player
- **Attack Range**: Maximum range for attacks
- **Attack Cooldown**: Time between attacks
- **Attack Type**: Melee, Projectile, or Area
- **Auto Attack**: Whether enemy attacks automatically
- **Warning Duration**: Duration of attack warning phase

## Enemy Types

### Basilisk
- **Damage**: 35
- **Range**: 4 units
- **Cooldown**: 3 seconds
- **Type**: Melee (powerful lunge attacks)

### Dementor
- **Damage**: 25
- **Range**: 5 units
- **Cooldown**: 2.5 seconds
- **Type**: Area (soul drain effect)

### Default Enemy
- **Damage**: 20
- **Range**: 3 units
- **Cooldown**: 2 seconds
- **Type**: Melee

## Events

### PlayerHealth Events
```csharp
playerHealth.OnHealthChanged += (percentage) => { /* Health changed */ };
playerHealth.OnPlayerDied += () => { /* Player died */ };
playerHealth.OnPlayerRevived += () => { /* Player revived */ };
```

### EnemyAttack Events
```csharp
enemyAttack.OnAttackStarted += (damage) => { /* Attack started */ };
enemyAttack.OnAttackCompleted += () => { /* Attack completed */ };
enemyAttack.OnAttackCancelled += () => { /* Attack cancelled */ };
```

## Testing

### Context Menu Options
Right-click on `PlayerHealthSetup` component in inspector:
- **Setup Player Health System**: Manually trigger setup
- **Test Damage**: Apply test damage from random direction
- **Test Heal**: Apply test healing
- **Clear All Damage Indicators**: Remove all active indicators

### Debug Commands
```csharp
// Test damage with specific source
PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
playerHealth.TakeDamage(20f, enemyTransform.position);

// Test damage indicators
DamageIndicator.Instance.ShowDamageIndicator(Vector3.left, 25f);

// Clear all indicators
DamageIndicator.Instance.ClearAllIndicators();
```

## Integration with Existing Systems

### With Spell System
```csharp
// In spell collision handler
if (hit.collider.CompareTag("Player"))
{
    PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
    if (playerHealth != null)
    {
        playerHealth.TakeDamage(spellDamage, spellSource.position);
    }
}
```

### With Enemy AI
```csharp
// In enemy AI update
if (CanSeePlayer() && IsInAttackRange())
{
    EnemyAttack attack = GetComponent<EnemyAttack>();
    if (attack != null && attack.CanAttack())
    {
        attack.StartAttack();
    }
}
```

## Performance Considerations

- **UI Updates**: Health bar updates only when health changes
- **Damage Indicators**: Automatically cleaned up after fade duration
- **Attack Animations**: Simple transform-based animations for performance
- **Audio**: Uses AudioSource.PlayOneShot for non-overlapping sounds

## Customization

### Custom Attack Types
Extend the `AttackType` enum and add new cases in `ExecuteAttackPhase()`:
```csharp
public enum AttackType
{
    Melee,
    Projectile,
    Area,
    Custom // Add your custom type
}
```

### Custom Damage Effects
Override or extend the damage effect methods in `PlayerHealth`:
```csharp
protected virtual void TriggerCustomDamageEffect()
{
    // Your custom damage effect
}
```

### Custom UI
Replace the automatically created UI elements with your own prefabs by assigning them in the inspector before calling setup.

## Troubleshooting

### Common Issues
1. **No damage indicators**: Ensure `DamageIndicator.Instance` is created and camera is found
2. **Health bar not showing**: Check if Canvas is properly created and camera reference is set
3. **Enemies not attacking**: Verify `EnemyAttack` components are added and player is in range
4. **Audio not playing**: Check AudioSource components and AudioClip assignments

### Debug Logs
All components include detailed debug logging. Check the Console for detailed information about system states and operations. 
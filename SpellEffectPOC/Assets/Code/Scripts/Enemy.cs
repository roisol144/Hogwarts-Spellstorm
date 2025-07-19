using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Health Bar")]
    [SerializeField] private GameObject healthBarPrefab; // Assign health bar prefab in inspector
    protected Transform healthBarInstance;
    private HealthBar healthBarComponent;

    [Header("Death Effects")]
    public GameObject deathEffectPrefab; // Assign a prefab in the Inspector
    [SerializeField] private bool useShaderDissolveEffect = true; // Whether to use shader-based dissolving effect
    private ShaderDissolveEffect shaderDissolveEffect;

    [Header("Camera Reference")]
    [SerializeField] private Transform playerCamera; // Will be set automatically if null

    protected bool isDead = false;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        
        // Find player camera if not assigned
        if (playerCamera == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                playerCamera = mainCamera.transform;
            }
        }

        // Get or add shader dissolving effect component
        if (useShaderDissolveEffect)
        {
            shaderDissolveEffect = GetComponent<ShaderDissolveEffect>();
            if (shaderDissolveEffect == null)
            {
                shaderDissolveEffect = gameObject.AddComponent<ShaderDissolveEffect>();
            }
        }
    }

    private void Update()
    {
        // Make health bar follow and face the player camera
        if (healthBarInstance != null && playerCamera != null)
        {
            // Position health bar above enemy (using overridable method)
            Vector3 healthBarPosition = GetHealthBarPosition();
            healthBarInstance.position = healthBarPosition;
            
            // Make health bar face the player camera
            Vector3 directionToCamera = playerCamera.position - healthBarInstance.position;
            healthBarInstance.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }

    /// <summary>
    /// Override this method in derived classes to customize health bar positioning
    /// </summary>
    protected virtual Vector3 GetHealthBarPosition()
    {
        // Default positioning above enemy center
        return transform.position + Vector3.up * 3f;
    }

    public void TakeDamage(float damage, bool isSpecialAttack = false)
    {
        if (isDead) 
        {
            Debug.Log($"[Enemy] {gameObject.name} is already dead, ignoring damage");
            return;
        }

        Debug.Log($"[Enemy] {gameObject.name} taking damage. Special attack: {isSpecialAttack}");
        Debug.Log($"[Enemy] Health before damage: {currentHealth}/{maxHealth} ({(currentHealth/maxHealth)*100:F1}%)");

        if (isSpecialAttack)
        {
            // Special attack (Impact01) - instant kill
            Debug.Log($"[Enemy] Special attack - instant kill!");
            
            // Set health to 0 for consistency
            currentHealth = 0f;
            
            // Trigger death animation if TrollAnimationController is present
            var trollAnimController = GetComponent<TrollAnimationController>();
            if (trollAnimController != null)
            {
                trollAnimController.TriggerDeath();
                Debug.Log($"[Enemy] Triggered death animation for special attack kill");
            }
            
            Die();
            return;
        }

        // Regular damage (fireball) - 1/3 of MAX health (not current health)
        // This ensures exactly 3 hits will kill any enemy
        float actualDamage = maxHealth / 3f;
        currentHealth -= actualDamage;
        
        // Clamp health to 0 if it's very close (fixes floating point precision issues)
        if (currentHealth < 0.01f)
        {
            currentHealth = 0f;
        }

        Debug.Log($"[Enemy] Regular damage applied: {actualDamage:F1}");
        Debug.Log($"[Enemy] Health after damage: {currentHealth:F1}/{maxHealth} ({(currentHealth/maxHealth)*100:F1}%)");

        // Show health bar when taking damage (this will also update it)
        ShowHealthBar();

        // Check if enemy should die
        if (currentHealth <= 0)
        {
            Debug.Log($"[Enemy] Health depleted - enemy should die");
            Die();
        }
        else
        {
            Debug.Log($"[Enemy] Enemy still alive with {currentHealth:F1} health remaining");
        }
    }

    private void ShowHealthBar()
    {
        if (healthBarInstance == null && healthBarPrefab != null)
        {
            // Create health bar above enemy
            Vector3 healthBarPosition = GetHealthBarPosition();
            healthBarInstance = Instantiate(healthBarPrefab, healthBarPosition, Quaternion.identity).transform;
            
            Debug.Log($"[Enemy] Created health bar at position: {healthBarPosition}");
            
            // Get the HealthBar component - it should be available immediately after instantiation
            healthBarComponent = healthBarInstance.GetComponent<HealthBar>();
            Debug.Log($"[Enemy] HealthBar component found: {healthBarComponent != null}");
            
            if (healthBarComponent == null)
            {
                Debug.LogError("[Enemy] Failed to get HealthBar component from instantiated prefab!");
                
                // Try to find it in children as backup
                healthBarComponent = healthBarInstance.GetComponentInChildren<HealthBar>();
                Debug.Log($"[Enemy] HealthBar component found in children: {healthBarComponent != null}");
                
                if (healthBarComponent == null)
                {
                    Debug.LogError("[Enemy] HealthBar component not found anywhere! Check prefab setup.");
                    return;
                }
            }
            
            // Ensure the health bar is visible and set initial state
            healthBarInstance.gameObject.SetActive(true);
            
            // Set initial health percentage immediately
            float healthPercentage = currentHealth / maxHealth;
            Debug.Log($"[Enemy] Setting initial health percentage: {healthPercentage:F2} ({healthPercentage * 100:F1}%)");
            
            // Call UpdateHealthBar which will show it and start the timer
            healthBarComponent.UpdateHealthBar(healthPercentage);
            
            Debug.Log($"[Enemy] Health bar should now be visible");
        }
        else if (healthBarInstance != null)
        {
            Debug.Log($"[Enemy] Health bar already exists, just updating");
            if (healthBarComponent != null)
            {
                float healthPercentage = currentHealth / maxHealth;
                Debug.Log($"[Enemy] Updating existing health bar to: {healthPercentage:F2} ({healthPercentage * 100:F1}%)");
                healthBarComponent.UpdateHealthBar(healthPercentage);
            }
            else
            {
                Debug.LogError("[Enemy] Health bar instance exists but component is null!");
                // Try to re-get the component
                healthBarComponent = healthBarInstance.GetComponent<HealthBar>();
                if (healthBarComponent != null)
                {
                    float healthPercentage = currentHealth / maxHealth;
                    Debug.Log($"[Enemy] Re-found component, updating health bar to: {healthPercentage:F2}");
                    healthBarComponent.UpdateHealthBar(healthPercentage);
                }
            }
        }
        else
        {
            Debug.LogError("[Enemy] Cannot show health bar - no prefab assigned!");
        }
    }

    public virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"[Enemy] {gameObject.name} died! Calling ScoreManager.NotifyKill()");

        // Award score for kill
        ScoreManager.NotifyKill();
        
        Debug.Log($"[Enemy] ScoreManager.NotifyKill() call completed");

        // Hide health bar
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance.gameObject);
        }

        // Spawn death effect
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // Start shader dissolving effect or destroy immediately
        if (useShaderDissolveEffect && shaderDissolveEffect != null)
        {
            Debug.Log($"[Enemy] Starting shader dissolving effect for {gameObject.name}");
            
            // Disable any movement or AI components during dissolving
            UnityEngine.AI.NavMeshAgent navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
            {
                navAgent.isStopped = true;
                navAgent.ResetPath();
                navAgent.enabled = false;
            }
            
            // Stop enemy animations
            EnemyAnimationController animController = GetComponent<EnemyAnimationController>();
            if (animController != null)
            {
                animController.StopAnimation();
            }

            // Disable collider to prevent further interactions
            Collider enemyCollider = GetComponent<Collider>();
            if (enemyCollider != null)
            {
                enemyCollider.enabled = false;
            }

            // Start the shader dissolving effect - it will destroy the object when complete
            shaderDissolveEffect.StartDissolving();
        }
        else
        {
            // Fallback: destroy immediately if no dissolving effect
            Destroy(gameObject);
        }
    }

    // Public method to get current health percentage (for external use)
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    /// <summary>
    /// Public method to set max health (for setup scripts)
    /// </summary>
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth; // Reset current health to max when changing max health
        Debug.Log($"[Enemy] Max health set to {maxHealth} for {gameObject.name}");
    }
    
    /// <summary>
    /// Public method to get max health
    /// </summary>
    public float GetMaxHealth()
    {
        return maxHealth;
    }
    
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public virtual void TakeDamage(float damage)
    {
        if (currentHealth <= 0)
            return;
            
        currentHealth -= damage;
        
        // Trigger hit animation if TrollAnimationController is present
        var trollAnimController = GetComponent<TrollAnimationController>();
        if (trollAnimController != null)
        {
            trollAnimController.TriggerHit();
        }
        
        Debug.Log($"💥 {gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        if (healthBarComponent != null)
        {
            float healthPercentage = currentHealth / maxHealth;
            healthBarComponent.UpdateHealthBar(healthPercentage);
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
} 
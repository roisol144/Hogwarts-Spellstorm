using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Health Bar")]
    [SerializeField] private GameObject healthBarPrefab; // Assign health bar prefab in inspector
    private Transform healthBarInstance;
    private HealthBar healthBarComponent;

    [Header("Death Effects")]
    public GameObject deathEffectPrefab; // Assign a prefab in the Inspector

    [Header("Camera Reference")]
    [SerializeField] private Transform playerCamera; // Will be set automatically if null

    private bool isDead = false;

    private void Start()
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
    }

    private void Update()
    {
        // Make health bar follow and face the player camera
        if (healthBarInstance != null && playerCamera != null)
        {
            // Position health bar above enemy (following the enemy)
            Vector3 healthBarPosition = transform.position + Vector3.up * 3f; // Higher position
            healthBarInstance.position = healthBarPosition;
            
            // Make health bar face the player camera
            Vector3 directionToCamera = playerCamera.position - healthBarInstance.position;
            healthBarInstance.rotation = Quaternion.LookRotation(directionToCamera);
        }
    }

    public void TakeDamage(float damage, bool isSpecialAttack = false)
    {
        if (isDead) return;

        Debug.Log($"[Enemy] {gameObject.name} taking damage. Special attack: {isSpecialAttack}");
        Debug.Log($"[Enemy] Health before damage: {currentHealth}/{maxHealth} ({(currentHealth/maxHealth)*100:F1}%)");

        if (isSpecialAttack)
        {
            // Special attack (Impact01) - instant kill
            Debug.Log($"[Enemy] Special attack - instant kill!");
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
            Vector3 healthBarPosition = transform.position + Vector3.up * 3f; // Higher position
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

        Debug.Log($"Enemy {gameObject.name} died!");

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

        // Destroy enemy
        Destroy(gameObject);
    }

    // Public method to get current health percentage (for external use)
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
} 
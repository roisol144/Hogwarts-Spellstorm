using UnityEngine;

public class PlayerHealthSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool attachToMainCamera = true;
    
    [Header("Health Settings")]
    [SerializeField] private float playerMaxHealth = 100f;
    [SerializeField] private float healthRegenRate = 5f;
    [SerializeField] private float healthRegenDelay = 3f;
    
    [Header("Damage Indicator Settings")]
    [SerializeField] private bool enableDamageIndicators = true;
    [SerializeField] private Color indicatorColor = new Color(1f, 0f, 0f, 0.8f);
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.3f);
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupPlayerHealth();
        }
    }
    
    [ContextMenu("Setup Player Health System")]
    public void SetupPlayerHealth()
    {
        GameObject playerObject = gameObject;
        
        // If attach to main camera is enabled, find the main camera
        if (attachToMainCamera)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                playerObject = mainCamera.gameObject;
            }
            else
            {
                Debug.LogWarning("[PlayerHealthSetup] No main camera found, using current GameObject");
            }
        }
        
        // Setup PlayerHealth component
        PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = playerObject.AddComponent<PlayerHealth>();
            Debug.Log($"[PlayerHealthSetup] Added PlayerHealth component to {playerObject.name}");
        }
        
        // Configure health settings
        playerHealth.SetMaxHealth(playerMaxHealth);
        
        // Setup DamageIndicator system
        if (enableDamageIndicators)
        {
            DamageIndicator damageIndicator = DamageIndicator.Instance;
            if (damageIndicator != null)
            {
                damageIndicator.SetIndicatorColor(indicatorColor);
                damageIndicator.SetFlashColor(flashColor);
                Debug.Log("[PlayerHealthSetup] Configured damage indicator system");
            }
        }
        
        // Setup enemy attacks
        SetupEnemyAttacks();
        
        Debug.Log($"[PlayerHealthSetup] Player health system setup complete on {playerObject.name}");
    }
    
    private void SetupEnemyAttacks()
    {
        // Find all enemies and add attack components if they don't have them
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        int enemiesSetup = 0;
        
        foreach (Enemy enemy in enemies)
        {
            EnemyAttack enemyAttack = enemy.GetComponent<EnemyAttack>();
            if (enemyAttack == null)
            {
                enemyAttack = enemy.gameObject.AddComponent<EnemyAttack>();
                
                // Configure based on enemy type
                ConfigureEnemyAttack(enemy, enemyAttack);
                
                enemiesSetup++;
            }
        }
        
        if (enemiesSetup > 0)
        {
            Debug.Log($"[PlayerHealthSetup] Added attack components to {enemiesSetup} enemies");
        }
    }
    
    private void ConfigureEnemyAttack(Enemy enemy, EnemyAttack enemyAttack)
    {
        // Configure attack based on enemy type
        string enemyName = enemy.gameObject.name.ToLower();
        
        // Get enemy movement component to sync with stop radius
        EnemyMovement enemyMovement = enemy.GetComponent<EnemyMovement>();
        float baseStopRadius = enemyMovement != null ? enemyMovement.stopRadius : 2f;
        
        if (enemyName.Contains("basilisk"))
        {
            // Basilisk - powerful melee attacks
            enemyAttack.SetAttackDamage(35f);
            enemyAttack.SetAttackRange(baseStopRadius + 0.5f); // Sync with stop radius
            enemyAttack.SetAttackCooldown(3f);
        }
        else if (enemyName.Contains("dementor"))
        {
            // Dementor - area attacks with soul drain effect
            enemyAttack.SetAttackDamage(25f);
            enemyAttack.SetAttackRange(baseStopRadius + 1f); // Slightly larger for area attacks
            enemyAttack.SetAttackCooldown(2.5f);
        }
        else
        {
            // Default enemy settings
            enemyAttack.SetAttackDamage(20f);
            enemyAttack.SetAttackRange(baseStopRadius + 0.5f); // Sync with stop radius
            enemyAttack.SetAttackCooldown(2f);
        }
        
        Debug.Log($"[PlayerHealthSetup] Configured attack for {enemy.gameObject.name} with range {enemyAttack.GetAttackRange()} (stop radius: {baseStopRadius})");
    }
    
    [ContextMenu("Test Damage")]
    public void TestDamage()
    {
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            // Test damage from a random direction
            Vector3 testDirection = Random.insideUnitSphere;
            testDirection.y = 0; // Keep on horizontal plane
            testDirection = testDirection.normalized * 5f; // 5 units away
            
            Vector3 damageSource = playerHealth.transform.position + testDirection;
            playerHealth.TakeDamage(15f, damageSource);
            
            Debug.Log("[PlayerHealthSetup] Test damage applied");
        }
        else
        {
            Debug.LogWarning("[PlayerHealthSetup] No PlayerHealth component found to test");
        }
    }
    
    [ContextMenu("Test Heal")]
    public void TestHeal()
    {
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Heal(25f);
            Debug.Log("[PlayerHealthSetup] Test heal applied");
        }
        else
        {
            Debug.LogWarning("[PlayerHealthSetup] No PlayerHealth component found to test");
        }
    }
    
    [ContextMenu("Clear All Damage Indicators")]
    public void ClearDamageIndicators()
    {
        DamageIndicator.Instance.ClearAllIndicators();
        Debug.Log("[PlayerHealthSetup] Cleared all damage indicators");
    }
} 
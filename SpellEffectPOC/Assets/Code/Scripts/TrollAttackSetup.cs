using UnityEngine;

/// <summary>
/// Sets up the new troll model with proper attack components like basilisk and dementor
/// </summary>
public class TrollAttackSetup : MonoBehaviour
{
    [Header("Attack Setup")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private bool forceResetup = false;
    
    [Header("Troll Attack Configuration")]
    [SerializeField] private float trollDamage = 40f;
    [SerializeField] private float trollAttackRange = 4f;
    [SerializeField] private float trollAttackCooldown = 3.5f;
    [SerializeField] private float trollHealth = 120f;
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupTrollForAttack();
        }
    }
    
    [ContextMenu("Setup Troll for Attack")]
    public void SetupTrollForAttack()
    {
        Debug.Log("=== SETTING UP TROLL FOR ATTACK ===");
        
        // 1. Ensure Enemy tag
        if (!gameObject.CompareTag("Enemy"))
        {
            gameObject.tag = "Enemy";
            Debug.Log("✅ Set Enemy tag");
        }
        
        // 2. Add/Configure NavMeshAgent
        SetupNavMeshAgent();
        
        // 3. Add/Configure Collider
        SetupCollider();
        
        // 4. Add/Configure EnemyMovement
        SetupEnemyMovement();
        
        // 5. Add/Configure EnemyAttack 
        SetupEnemyAttack();
        
        // 6. Add/Configure Troll script (Enemy derivative)
        SetupTrollScript();
        
        // 7. Add material validation
        SetupMaterialValidation();
        
        Debug.Log("=== TROLL ATTACK SETUP COMPLETE ===");
        Debug.Log("Your troll should now attack like the basilisk and dementor!");
        
        // Disable this setup script after completion (it's only needed once)
        if (!forceResetup)
        {
            this.enabled = false;
            Debug.Log("🔧 TrollAttackSetup script disabled (job complete)");
        }
    }
    
    void SetupNavMeshAgent()
    {
        var navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent == null || forceResetup)
        {
            if (navAgent == null)
            {
                navAgent = gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
            }
            
            // Configure for troll (larger than other enemies)
            navAgent.radius = 1.5f;
            navAgent.speed = 2f;
            navAgent.acceleration = 8f;
            navAgent.angularSpeed = 120f;
            navAgent.stoppingDistance = 0.1f;
            navAgent.height = 4f;
            navAgent.avoidancePriority = 50;
            navAgent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            
            Debug.Log("✅ NavMeshAgent configured");
        }
    }
    
    void SetupCollider()
    {
        var collider = GetComponent<Collider>();
        if (collider == null || forceResetup)
        {
            // Remove existing collider if force reset
            if (forceResetup && collider != null)
            {
                DestroyImmediate(collider);
            }
            
            var capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            capsuleCollider.radius = 1.5f;
            capsuleCollider.height = 4.5f;
            capsuleCollider.center = new Vector3(0, 2, 0);
            capsuleCollider.isTrigger = false;
            
            Debug.Log("✅ CapsuleCollider configured");
        }
    }
    
    void SetupEnemyMovement()
    {
        var enemyMovement = GetComponent<EnemyMovement>();
        if (enemyMovement == null || forceResetup)
        {
            if (enemyMovement == null)
            {
                enemyMovement = gameObject.AddComponent<EnemyMovement>();
            }
            
            // Configure movement - trolls should face player directly (no wobbling like boss enemy)
            enemyMovement.stopRadius = 3f;
            enemyMovement.wobbleAmount = 0f; // No wobbling - face player directly like basilisk/dementor
            enemyMovement.wobbleSpeed = 0f;  // No wobble speed
            
            Debug.Log("✅ EnemyMovement configured");
        }
    }
    
    void SetupEnemyAttack()
    {
        var enemyAttack = GetComponent<EnemyAttack>();
        if (enemyAttack == null || forceResetup)
        {
            if (enemyAttack == null)
            {
                enemyAttack = gameObject.AddComponent<EnemyAttack>();
            }
            
                    // Configure troll-specific attack (strongest enemy)
        enemyAttack.SetAttackDamage(trollDamage);
        enemyAttack.SetAttackRange(trollAttackRange);
        enemyAttack.SetAttackCooldown(trollAttackCooldown);
        enemyAttack.SetAutoAttack(true);
        
        // Set troll to use AnimationOnly attack (no tackle/lunge movement)
        enemyAttack.SetAttackType(EnemyAttack.AttackType.AnimationOnly);
        Debug.Log("✅ Troll set to use AnimationOnly attack (no movement)");
            
            Debug.Log($"✅ EnemyAttack configured - Damage: {trollDamage}, Range: {trollAttackRange}, Cooldown: {trollAttackCooldown}");
        }
        
        // Force update the attack settings even if component exists
        if (forceResetup && enemyAttack != null)
        {
            enemyAttack.SetAttackDamage(trollDamage);
            enemyAttack.SetAttackRange(trollAttackRange);
            enemyAttack.SetAttackCooldown(trollAttackCooldown);
            enemyAttack.SetAutoAttack(true);
            
            // Set troll to use AnimationOnly attack (no tackle/lunge movement)
            enemyAttack.SetAttackType(EnemyAttack.AttackType.AnimationOnly);
            Debug.Log("🔄 Troll attack type updated to AnimationOnly (no movement)");
            
            Debug.Log($"🔄 EnemyAttack reconfigured - Damage: {trollDamage}, Range: {trollAttackRange}, Cooldown: {trollAttackCooldown}");
        }
    }
    
    void SetupTrollScript()
    {
        var trollScript = GetComponent<Troll>();
        if (trollScript == null)
        {
            trollScript = gameObject.AddComponent<Troll>();
            Debug.Log("✅ Troll script added");
        }
        
        // Configure troll health (stronger than other enemies)
        trollScript.SetMaxHealth(trollHealth);
        
        Debug.Log($"✅ Troll health set to {trollHealth}");
    }
    
    void SetupMaterialValidation()
    {
        var materialGuide = GetComponent<TrollMaterialSetupGuide>();
        if (materialGuide == null)
        {
            materialGuide = gameObject.AddComponent<TrollMaterialSetupGuide>();
            Debug.Log("✅ Material validation added");
        }
    }
    
    [ContextMenu("Test Attack Setup")]
    public void TestAttackSetup()
    {
        Debug.Log("=== TESTING ATTACK SETUP ===");
        
        var requiredComponents = new System.Type[]
        {
            typeof(UnityEngine.AI.NavMeshAgent),
            typeof(Collider),
            typeof(EnemyMovement),
            typeof(EnemyAttack),
            typeof(Troll)
        };
        
        bool allGood = true;
        foreach (var componentType in requiredComponents)
        {
            var component = GetComponent(componentType);
            if (component != null)
            {
                Debug.Log($"✅ {componentType.Name}: Found");
            }
            else
            {
                Debug.LogError($"❌ {componentType.Name}: MISSING!");
                allGood = false;
            }
        }
        
        if (!gameObject.CompareTag("Enemy"))
        {
            Debug.LogError("❌ Enemy tag: MISSING!");
            allGood = false;
        }
        else
        {
            Debug.Log("✅ Enemy tag: Set");
        }
        
        if (allGood)
        {
            Debug.Log("🎉 ATTACK SETUP TEST PASSED! Troll should attack properly!");
        }
        else
        {
            Debug.LogError("💥 ATTACK SETUP TEST FAILED! Run 'Setup Troll for Attack' first!");
        }
    }
} 
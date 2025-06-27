using UnityEngine;
using System.Collections;

public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackAnimationDuration = 0.8f;
    
    [Header("Attack Animation")]
    [SerializeField] private Transform attackIndicator; // Visual indicator for attack
    [SerializeField] private Color attackWarningColor = Color.red;
    [SerializeField] private Color attackExecuteColor = new Color(1f, 0.5f, 0f, 1f); // Orange color
    [SerializeField] private float warningDuration = 0.5f;
    
    [Header("Attack Effects")]
    [SerializeField] private GameObject attackEffectPrefab;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip attackChargeSound;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Attack Pattern")]
    [SerializeField] private AttackType attackType = AttackType.Melee;
    [SerializeField] private bool autoAttack = true;
    [SerializeField] private float autoAttackInterval = 3f;
    
    public enum AttackType
    {
        Melee,       // Close range attack
        Projectile,  // Ranged attack with projectile
        Area         // Area of effect attack
    }
    
    // Private variables
    private PlayerHealth playerHealth;
    private Transform player;
    private bool canAttack = true;
    private bool isAttacking = false;
    private Coroutine attackCoroutine;
    private Coroutine autoAttackCoroutine;
    private Enemy enemyComponent;
    private EnemyMovement enemyMovement;
    private Renderer enemyRenderer;
    private Color originalColor;
    private bool hasReachedStopRadius = false;
    private bool protegoShieldActive = false; // New field for Protego shield
    
    // Events
    public System.Action<float> OnAttackStarted; // float = damage that will be dealt
    public System.Action OnAttackCompleted;
    public System.Action OnAttackCancelled;
    
    private void Awake()
    {
        // Get enemy component
        enemyComponent = GetComponent<Enemy>();
        
        // Get enemy movement component
        enemyMovement = GetComponent<EnemyMovement>();
        
        // Get renderer for color changes
        enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }
        
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Create attack indicator if none assigned (only for non-melee attacks)
        if (attackIndicator == null && attackType != AttackType.Melee)
        {
            CreateAttackIndicator();
        }
        
        // Sync attack range with movement stop radius if available
        if (enemyMovement != null)
        {
            attackRange = enemyMovement.stopRadius + 0.5f; // Slightly larger than stop radius
            Debug.Log($"[EnemyAttack] Synced attack range to {attackRange} based on stop radius {enemyMovement.stopRadius}");
        }
    }
    
    private void Start()
    {
        // Find player
        FindPlayer();
        
        // Start auto attack if enabled
        if (autoAttack)
        {
            StartAutoAttack();
        }
    }
    
    private void Update()
    {
        // Check if enemy is still alive
        if (enemyComponent != null && enemyComponent.GetHealthPercentage() <= 0)
        {
            StopAllAttacks();
            return;
        }
        
        // Check if enemy has reached stop radius
        CheckStopRadiusReached();
        
        // Update attack indicator position and rotation
        if (attackIndicator != null && player != null)
        {
            UpdateAttackIndicator();
        }
    }
    
    private void OnDestroy()
    {
        StopAllAttacks();
        CleanupVisualElements();
    }
    
    private void CleanupVisualElements()
    {
        // Clean up attack indicator
        if (attackIndicator != null)
        {
            if (attackIndicator.gameObject != null)
            {
                Destroy(attackIndicator.gameObject);
            }
        }
        
        // Clean up any remaining area attack indicators (in case they weren't properly destroyed)
        GameObject[] areaIndicators = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (GameObject obj in areaIndicators)
        {
            if (obj.name == "AreaAttackIndicator")
            {
                Destroy(obj);
            }
        }
    }
    
    private void FindPlayer()
    {
        // Try to find player health component first
        playerHealth = FindObjectOfType<PlayerHealth>();
        
        if (playerHealth != null)
        {
            player = playerHealth.transform;
        }
        else
        {
            // Fallback to main camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                player = mainCamera.transform;
            }
        }
        
        if (player == null)
        {
            Debug.LogWarning($"[EnemyAttack] Could not find player for {gameObject.name}");
        }
    }
    
    private void CreateAttackIndicator()
    {
        GameObject indicatorObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicatorObject.name = "AttackIndicator";
        indicatorObject.transform.SetParent(transform);
        indicatorObject.transform.localPosition = Vector3.forward * 2f;
        indicatorObject.transform.localScale = Vector3.one * 0.5f;
        
        // Remove collider
        Collider indicatorCollider = indicatorObject.GetComponent<Collider>();
        if (indicatorCollider != null)
        {
            DestroyImmediate(indicatorCollider);
        }
        
        // Make it invisible by default - immediately disable the renderer to prevent pink flash
        Renderer indicatorRenderer = indicatorObject.GetComponent<Renderer>();
        if (indicatorRenderer != null)
        {
            // Disable renderer immediately to prevent any visual artifacts
            indicatorRenderer.enabled = false;
            
            // Still setup the material for when we need to show it
            Material indicatorMaterial = new Material(Shader.Find("Standard"));
            indicatorMaterial.SetFloat("_Mode", 3); // Set to transparent mode
            indicatorMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            indicatorMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            indicatorMaterial.SetInt("_ZWrite", 0);
            indicatorMaterial.DisableKeyword("_ALPHATEST_ON");
            indicatorMaterial.EnableKeyword("_ALPHABLEND_ON");
            indicatorMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            indicatorMaterial.renderQueue = 3000;
            indicatorMaterial.color = new Color(1, 0, 0, 0); // Transparent red
            indicatorRenderer.material = indicatorMaterial;
        }
        
        attackIndicator = indicatorObject.transform;
    }
    
    private void CheckStopRadiusReached()
    {
        if (player == null || enemyMovement == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool withinStopRadius = distanceToPlayer <= enemyMovement.stopRadius;
        
        // If we just reached the stop radius, trigger first attack
        if (withinStopRadius && !hasReachedStopRadius)
        {
            hasReachedStopRadius = true;
            Debug.Log($"[EnemyAttack] {gameObject.name} reached stop radius, starting attacks!");
            
            // Start attacking immediately when reaching stop radius
            if (canAttack && !isAttacking)
            {
                StartAttack();
            }
        }
        else if (!withinStopRadius && hasReachedStopRadius)
        {
            // Player moved away, reset the flag
            hasReachedStopRadius = false;
        }
    }
    
    private void UpdateAttackIndicator()
    {
        if (isAttacking) return;
        
        // Position indicator between enemy and player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        attackIndicator.position = transform.position + directionToPlayer * 2f;
        
        // Make indicator look at player
        attackIndicator.LookAt(player.position);
    }
    
    private void StartAutoAttack()
    {
        if (autoAttackCoroutine != null)
        {
            StopCoroutine(autoAttackCoroutine);
        }
        autoAttackCoroutine = StartCoroutine(AutoAttackCoroutine());
    }
    
    private IEnumerator AutoAttackCoroutine()
    {
        while (gameObject != null && (enemyComponent == null || enemyComponent.GetHealthPercentage() > 0))
        {
            // Check more frequently when within stop radius
            float checkInterval = hasReachedStopRadius ? autoAttackInterval * 0.5f : autoAttackInterval;
            yield return new WaitForSeconds(checkInterval);
            
            if (CanAttackPlayer())
            {
                StartAttack();
            }
        }
        
        autoAttackCoroutine = null;
    }
    
    public void StartAttack()
    {
        if (!canAttack || isAttacking) return;
        if (enemyComponent != null && enemyComponent.GetHealthPercentage() <= 0) return;
        if (!CanAttackPlayer()) return;
        
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        
        attackCoroutine = StartCoroutine(AttackCoroutine());
    }
    
    public void StopAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        
        isAttacking = false;
        ResetVisualEffects();
        OnAttackCancelled?.Invoke();
    }
    
    private void StopAllAttacks()
    {
        StopAttack();
        
        if (autoAttackCoroutine != null)
        {
            StopCoroutine(autoAttackCoroutine);
            autoAttackCoroutine = null;
        }
    }
    
    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        canAttack = false;
        
        OnAttackStarted?.Invoke(attackDamage);
        
        // Warning phase
        yield return StartCoroutine(AttackWarningPhase());
        
        // Execute phase
        yield return StartCoroutine(AttackExecutePhase());
        
        // Cleanup
        isAttacking = false;
        ResetVisualEffects();
        OnAttackCompleted?.Invoke();
        
        // Start cooldown
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        
        attackCoroutine = null;
    }
    
    private IEnumerator AttackWarningPhase()
    {
        Debug.Log($"[EnemyAttack] {gameObject.name} starting attack warning phase");
        
        // Play charge sound
        if (attackChargeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackChargeSound);
        }
        
        // Change enemy color to warning color
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = attackWarningColor;
        }
        
        // Show attack indicator
        if (attackIndicator != null)
        {
            Renderer indicatorRenderer = attackIndicator.GetComponent<Renderer>();
            if (indicatorRenderer != null)
            {
                // Enable renderer when we want to show the indicator
                indicatorRenderer.enabled = true;
                Color indicatorColor = attackWarningColor;
                indicatorColor.a = 0.7f;
                indicatorRenderer.material.color = indicatorColor;
            }
        }
        
        float elapsedTime = 0f;
        while (elapsedTime < warningDuration)
        {
            // Pulse effect
            float pulse = Mathf.Sin(elapsedTime * 10f) * 0.5f + 0.5f;
            
            if (enemyRenderer != null)
            {
                Color pulsedColor = Color.Lerp(originalColor, attackWarningColor, pulse);
                enemyRenderer.material.color = pulsedColor;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    
    private IEnumerator AttackExecutePhase()
    {
        Debug.Log($"[EnemyAttack] {gameObject.name} executing attack");
        
        // Change to execute color
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = attackExecuteColor;
        }
        
        // Update indicator color
        if (attackIndicator != null)
        {
            Renderer indicatorRenderer = attackIndicator.GetComponent<Renderer>();
            if (indicatorRenderer != null)
            {
                Color indicatorColor = attackExecuteColor;
                indicatorColor.a = 1f;
                indicatorRenderer.material.color = indicatorColor;
            }
        }
        
        // Execute attack based on type
        switch (attackType)
        {
            case AttackType.Melee:
                yield return StartCoroutine(ExecuteMeleeAttack());
                break;
            case AttackType.Projectile:
                yield return StartCoroutine(ExecuteProjectileAttack());
                break;
            case AttackType.Area:
                yield return StartCoroutine(ExecuteAreaAttack());
                break;
        }
    }
    
    private IEnumerator ExecuteMeleeAttack()
    {
        // Simple lunge animation
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = Vector3.MoveTowards(startPosition, player.position, 1.5f);
        
        float elapsedTime = 0f;
        float attackDuration = attackAnimationDuration * 0.5f;
        
        // Lunge forward
        while (elapsedTime < attackDuration)
        {
            float t = elapsedTime / attackDuration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Deal damage at the peak of the attack
        DealDamageToPlayer();
        
        // Return to original position
        elapsedTime = 0f;
        while (elapsedTime < attackDuration)
        {
            float t = elapsedTime / attackDuration;
            transform.position = Vector3.Lerp(targetPosition, startPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.position = startPosition;
    }
    
    private IEnumerator ExecuteProjectileAttack()
    {
        // Create a simple projectile
        Vector3 projectileSpawnPos = transform.position + Vector3.up * 1.5f;
        Vector3 directionToPlayer = (player.position - projectileSpawnPos).normalized;
        
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = "EnemyProjectile";
        projectile.transform.position = projectileSpawnPos;
        projectile.transform.localScale = Vector3.one * 0.3f;
        
        // Make it red
        Renderer projRenderer = projectile.GetComponent<Renderer>();
        if (projRenderer != null)
        {
            projRenderer.material.color = Color.red;
        }
        
        // Add simple movement
        StartCoroutine(MoveProjectile(projectile, directionToPlayer));
        
        yield return new WaitForSeconds(attackAnimationDuration);
    }
    
    private IEnumerator MoveProjectile(GameObject projectile, Vector3 direction)
    {
        float speed = 10f;
        float lifetime = 3f;
        float elapsedTime = 0f;
        bool hasHit = false;
        
        while (elapsedTime < lifetime && projectile != null && !hasHit)
        {
            projectile.transform.position += direction * speed * Time.deltaTime;
            
            // Check distance to player
            if (Vector3.Distance(projectile.transform.position, player.position) < 1f)
            {
                DealDamageToPlayer();
                hasHit = true;
                
                // Spawn hit effect
                SpawnAttackEffect(projectile.transform.position);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        if (projectile != null)
        {
            Destroy(projectile);
        }
    }
    
    private IEnumerator ExecuteAreaAttack()
    {
        GameObject areaIndicator = null;
        
        try
        {
            // Create area effect indicator
            areaIndicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            areaIndicator.name = "AreaAttackIndicator";
            areaIndicator.transform.position = player.position;
            areaIndicator.transform.localScale = new Vector3(attackRange * 2f, 0.1f, attackRange * 2f);
            
            // Remove collider
            Collider areaCollider = areaIndicator.GetComponent<Collider>();
            if (areaCollider != null)
            {
                DestroyImmediate(areaCollider);
            }
            
            // Make it red and transparent
            Renderer areaRenderer = areaIndicator.GetComponent<Renderer>();
            if (areaRenderer != null)
            {
                Material areaMaterial = new Material(Shader.Find("Standard"));
                areaMaterial.SetFloat("_Mode", 3); // Transparent mode
                areaMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                areaMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                areaMaterial.SetInt("_ZWrite", 0);
                areaMaterial.EnableKeyword("_ALPHABLEND_ON");
                areaMaterial.renderQueue = 3000;
                areaMaterial.color = new Color(1f, 0f, 0f, 0.5f);
                areaRenderer.material = areaMaterial;
            }
            
            yield return new WaitForSeconds(attackAnimationDuration * 0.8f);
            
            // Check if player is still in area and deal damage
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                DealDamageToPlayer();
            }
            
            yield return new WaitForSeconds(attackAnimationDuration * 0.2f);
        }
        finally
        {
            // Ensure cleanup always happens
            if (areaIndicator != null)
            {
                Destroy(areaIndicator);
            }
        }
    }
    
    private void DealDamageToPlayer()
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage, transform.position);
            Debug.Log($"[EnemyAttack] {gameObject.name} dealt {attackDamage} damage to player");
        }
        
        // Play attack sound
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
        
        // Spawn attack effect
        SpawnAttackEffect(player.position);
    }
    
    private void SpawnAttackEffect(Vector3 position)
    {
        if (attackEffectPrefab != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f); // Auto-cleanup
        }
    }
    
    private void ResetVisualEffects()
    {
        // Reset enemy color
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = originalColor;
        }
        
        // Hide attack indicator
        if (attackIndicator != null)
        {
            Renderer indicatorRenderer = attackIndicator.GetComponent<Renderer>();
            if (indicatorRenderer != null)
            {
                // Disable renderer instead of just making it transparent
                indicatorRenderer.enabled = false;
            }
        }
    }
    
    private bool CanAttackPlayer()
    {
        if (player == null) return false;
        if (enemyComponent != null && enemyComponent.GetHealthPercentage() <= 0) return false;
        if (protegoShieldActive) return false; // Cannot attack if Protego shield is active
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Use movement stop radius if available, otherwise use attack range
        float effectiveRange = enemyMovement != null ? enemyMovement.stopRadius + 0.5f : attackRange;
        return distanceToPlayer <= effectiveRange;
    }
    
    // Public methods
    public void SetAttackDamage(float damage) => attackDamage = damage;
    public void SetAttackRange(float range) => attackRange = range;
    public void SetAttackCooldown(float cooldown) => attackCooldown = cooldown;
    public void SetAutoAttack(bool enabled) 
    { 
        autoAttack = enabled;
        if (enabled)
        {
            StartAutoAttack();
        }
        else
        {
            StopAllAttacks();
        }
    }
    
    // New method to handle Protego shield
    public void SetProtegoShieldActive(bool active)
    {
        protegoShieldActive = active;
        
        if (active)
        {
            Debug.Log($"[EnemyAttack] {gameObject.name} attacks stopped by Protego shield");
            // Stop any ongoing attacks
            StopAttack();
        }
        else
        {
            Debug.Log($"[EnemyAttack] {gameObject.name} attacks resumed after Protego shield");
        }
    }
    
    // Getters
    public bool IsAttacking() => isAttacking;
    public bool CanAttack() => canAttack;
    public float GetAttackRange() => attackRange;
    public bool IsProtegoShieldActive() => protegoShieldActive;
} 
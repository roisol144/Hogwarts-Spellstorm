using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class TrollAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float movementSpeedThreshold = 0.1f;
    [SerializeField] private float attackAnimationDuration = 1.2f;
    [SerializeField] private float hitAnimationDuration = 0.8f;
    [SerializeField] private bool useRandomAttackAnimation = true;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // Component References
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private EnemyAttack enemyAttack;
    private Enemy enemyHealth;
    
    // Animation State Tracking
    private bool isAttacking = false;
    private bool isTakingHit = false;
    private bool isDead = false;
    private float lastAttackTime = 0f;
    private float lastHitTime = 0f;
    private float previousHealth = 0f;
    
    // Animator Parameters (must match the Animator Controller)
    private readonly string SPEED_PARAM = "Speed";
    private readonly string ATTACK_TRIGGER = "Attack";
    private readonly string TAKE_HIT_TRIGGER = "TakeHit";
    private readonly string DIE_TRIGGER = "Die";
    private readonly string IS_DEAD_BOOL = "IsDead";
    
    void Start()
    {
        InitializeComponents();
        SetupEventListeners();
        
        if (enableDebugLogs)
            Debug.Log("🎬 TrollAnimationController initialized for " + gameObject.name);
    }
    
    void InitializeComponents()
    {
        // Get required components
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyAttack = GetComponent<EnemyAttack>();
        enemyHealth = GetComponent<Enemy>();
        
        // Validate components
        if (animator == null)
        {
            Debug.LogError("❌ TrollAnimationController: No Animator component found!");
            enabled = false;
            return;
        }
        
        if (navMeshAgent == null)
        {
            Debug.LogError("❌ TrollAnimationController: No NavMeshAgent component found!");
            enabled = false;
            return;
        }
        
        // The animator controller should already be assigned in the prefab
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("⚠️ No Animator Controller assigned! Please assign TrollAnimatorController in the Animator component.");
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("🎛️ Using assigned Animator Controller: " + animator.runtimeAnimatorController.name);
        }
        
        // Initialize animator state
        if (animator.runtimeAnimatorController != null)
        {
            animator.SetFloat(SPEED_PARAM, 0f);
            animator.SetBool(IS_DEAD_BOOL, false);
        }
        
        // Initialize health tracking
        if (enemyHealth != null)
        {
            previousHealth = enemyHealth.GetCurrentHealth();
        }
    }
    
    void SetupEventListeners()
    {
        // Listen for attack events from EnemyAttack component
        if (enemyAttack != null)
        {
            // We'll use Update to monitor attack state since EnemyAttack doesn't have events
            if (enableDebugLogs)
                Debug.Log("🔗 Connected to EnemyAttack component");
        }
        
        // Listen for health events from Enemy component
        if (enemyHealth != null)
        {
            // We'll monitor health changes in Update
            if (enableDebugLogs)
                Debug.Log("🔗 Connected to Enemy health component");
        }
    }
    
    void Update()
    {
        if (isDead || animator == null || navMeshAgent == null)
            return;
            
        UpdateMovementAnimation();
        MonitorAttackState();
        MonitorHealthState();
    }
    
    void UpdateMovementAnimation()
    {
        // Get current movement speed from NavMeshAgent
        float currentSpeed = navMeshAgent.velocity.magnitude;
        
        // Update Speed parameter for Idle <-> Walking transitions
        animator.SetFloat(SPEED_PARAM, currentSpeed);
        
        if (enableDebugLogs && Time.frameCount % 120 == 0) // Log every 2 seconds
        {
            string state = currentSpeed > movementSpeedThreshold ? "Walking" : "Idle";
            Debug.Log($"🚶 Troll Speed: {currentSpeed:F2} | State: {state}");
        }
    }
    
    void MonitorAttackState()
    {
        if (enemyAttack == null)
            return;
            
        // For AnimationOnly attacks, let EnemyAttack handle triggering via TriggerAttack() calls
        // Only auto-trigger for other attack types that don't have explicit animation triggering
        bool isAnimationOnlyAttack = IsAnimationOnlyAttackType();
        
        if (!isAnimationOnlyAttack)
        {
            // Check if we should trigger attack animation (for non-AnimationOnly attacks)
            bool shouldAttack = ShouldTriggerAttackAnimation();
            
            if (shouldAttack && !isAttacking && !isTakingHit)
            {
                TriggerAttackAnimation();
            }
        }
        
        // Reset attack state after animation duration
        if (isAttacking && Time.time - lastAttackTime > attackAnimationDuration)
        {
            isAttacking = false;
            if (enableDebugLogs)
                Debug.Log("⚔️ Attack animation completed");
        }
    }
    
    bool IsAnimationOnlyAttackType()
    {
        // Check if this enemy is using AnimationOnly attacks
        if (enemyAttack != null)
        {
            return enemyAttack.GetAttackType() == EnemyAttack.AttackType.AnimationOnly;
        }
        return false;
    }
    
    bool ShouldTriggerAttackAnimation()
    {
        // Check if enough time has passed since last attack and we're in range of player
        if (Time.time - lastAttackTime < 3.5f) // Use troll's attack cooldown
            return false;
            
        // Check if player is in attack range
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= enemyAttack.GetAttackRange() && enemyAttack.CanAttack())
            {
                return true;
            }
        }
        
        return false;
    }
    
    void TriggerAttackAnimation()
    {
        animator.SetTrigger(ATTACK_TRIGGER);
        isAttacking = true;
        lastAttackTime = Time.time;
        
        if (enableDebugLogs)
        {
            string attackType = useRandomAttackAnimation && Random.value > 0.5f ? "Attack2" : "Attack1";
            Debug.Log($"⚔️ Triggering {attackType} animation");
        }
    }
    
    void MonitorHealthState()
    {
        if (enemyHealth == null)
            return;
            
        float currentHealth = enemyHealth.GetCurrentHealth();
        
        // Check for death
        if (currentHealth <= 0 && !isDead)
        {
            TriggerDeathAnimation();
            return;
        }
        
        // Check for damage taken by comparing with previous health
        if (currentHealth < previousHealth && !isTakingHit && !isDead)
        {
            TriggerHitAnimation();
        }
        
        // Update previous health
        previousHealth = currentHealth;
        
        // Reset hit state after animation duration
        if (isTakingHit && Time.time - lastHitTime > hitAnimationDuration)
        {
            isTakingHit = false;
            if (enableDebugLogs)
                Debug.Log("💥 Hit animation completed");
        }
    }
    
    void TriggerHitAnimation()
    {
        if (!isTakingHit && !isDead)
        {
            animator.SetTrigger(TAKE_HIT_TRIGGER);
            isTakingHit = true;
            lastHitTime = Time.time;
            
            if (enableDebugLogs)
                Debug.Log("💥 Triggering hit animation");
        }
    }
    
    void TriggerDeathAnimation()
    {
        animator.SetTrigger(DIE_TRIGGER);
        animator.SetBool(IS_DEAD_BOOL, true);
        isDead = true;
        
        // Stop movement
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
        }
        
        // Disable attack component
        if (enemyAttack != null)
        {
            enemyAttack.enabled = false;
        }
        
        if (enableDebugLogs)
            Debug.Log("💀 Triggering death animation");
    }
    
    // Public methods for external systems to trigger animations
    public void TriggerHit()
    {
        TriggerHitAnimation();
    }
    
    public void TriggerDeath()
    {
        if (!isDead)
        {
            TriggerDeathAnimation();
            if (enableDebugLogs)
                Debug.Log($"💀 {gameObject.name} death animation triggered externally");
        }
    }
    
    public void TriggerAttack()
    {
        if (!isAttacking && !isTakingHit && !isDead)
        {
            TriggerAttackAnimation();
            if (enableDebugLogs)
                Debug.Log($"🎯 {gameObject.name} attack animation triggered externally");
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log($"⚠️ {gameObject.name} cannot trigger attack - isAttacking:{isAttacking}, isTakingHit:{isTakingHit}, isDead:{isDead}");
        }
    }
    
    public void ForceIdle()
    {
        if (animator != null)
        {
            animator.SetFloat(SPEED_PARAM, 0f);
        }
    }
    
    public void ForceWalk(float speed = 1f)
    {
        if (animator != null)
        {
            animator.SetFloat(SPEED_PARAM, speed);
        }
    }
    
    // Getter methods
    public bool IsAttacking() => isAttacking;
    public bool IsTakingHit() => isTakingHit;
    public bool IsDead() => isDead;
    
    void OnValidate()
    {
        // Ensure reasonable values in inspector
        movementSpeedThreshold = Mathf.Max(0.01f, movementSpeedThreshold);
        attackAnimationDuration = Mathf.Max(0.1f, attackAnimationDuration);
        hitAnimationDuration = Mathf.Max(0.1f, hitAnimationDuration);
    }
    
    void OnDrawGizmosSelected()
    {
        // Visual debugging in Scene view
        if (enemyAttack != null)
        {
            // Draw attack range
            Gizmos.color = isAttacking ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyAttack.GetAttackRange());
        }
        
        if (navMeshAgent != null)
        {
            // Draw movement direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, navMeshAgent.velocity);
        }
    }
} 
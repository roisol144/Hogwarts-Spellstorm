using UnityEngine;
using UnityEngine.AI;

public class TrollAnimationDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebug = true;
    [SerializeField] private float debugInterval = 1f; // Log every 1 second
    
    private Animator animator;
    private NavMeshAgent navAgent;
    private TrollAnimationController animController;
    private EnemyAttack enemyAttack;
    private Enemy enemy;
    
    private float lastDebugTime = 0f;
    
    void Start()
    {
        // Get all components
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        animController = GetComponent<TrollAnimationController>();
        enemyAttack = GetComponent<EnemyAttack>();
        enemy = GetComponent<Enemy>();
        
        if (enableDebug)
        {
            Debug.Log("🔧 === TROLL ANIMATION DEBUGGER STARTED ===");
            Debug.Log($"Animator: {(animator != null ? "✅" : "❌")}");
            Debug.Log($"NavMeshAgent: {(navAgent != null ? "✅" : "❌")}");
            Debug.Log($"TrollAnimationController: {(animController != null ? "✅" : "❌")}");
            Debug.Log($"EnemyAttack: {(enemyAttack != null ? "✅" : "❌")}");
            Debug.Log($"Enemy: {(enemy != null ? "✅" : "❌")}");
            
            if (animator != null)
            {
                Debug.Log($"Animator Controller: {(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "❌ NONE")}");
                Debug.Log($"Animator Enabled: {animator.enabled}");
            }
        }
    }
    
    void Update()
    {
        if (!enableDebug || Time.time - lastDebugTime < debugInterval)
            return;
            
        lastDebugTime = Time.time;
        LogAnimationState();
    }
    
    void LogAnimationState()
    {
        if (animator == null) return;
        
        // Current animation info
        var currentClip = animator.GetCurrentAnimatorClipInfo(0);
        string currentAnimation = currentClip.Length > 0 ? currentClip[0].clip.name : "NONE";
        
        // Parameters
        float speed = animator.GetFloat("Speed");
        bool isDead = animator.GetBool("IsDead");
        
        // NavMesh info
        float velocity = navAgent != null ? navAgent.velocity.magnitude : 0f;
        
        // Component states
        string attackStatus = enemyAttack != null ? (enemyAttack.CanAttack() ? "Ready" : "Cooldown") : "N/A";
        float health = enemy != null ? enemy.GetCurrentHealth() : 0f;
        
        Debug.Log($"🎬 ANIMATION DEBUG:");
        Debug.Log($"   Current Animation: {currentAnimation}");
        Debug.Log($"   Speed Parameter: {speed:F2}");
        Debug.Log($"   NavMesh Velocity: {velocity:F2}");
        Debug.Log($"   Is Dead: {isDead}");
        Debug.Log($"   Attack Status: {attackStatus}");
        Debug.Log($"   Health: {health:F1}");
        
        if (animController != null)
        {
            Debug.Log($"   Animation Controller States:");
            Debug.Log($"     - Is Attacking: {animController.IsAttacking()}");
            Debug.Log($"     - Is Taking Hit: {animController.IsTakingHit()}");
            Debug.Log($"     - Is Dead: {animController.IsDead()}");
        }
    }
    
    // Manual trigger methods for testing
    [ContextMenu("Force Attack Animation")]
    public void ForceAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
            Debug.Log("🔧 Manually triggered Attack animation");
        }
    }
    
    [ContextMenu("Force Hit Animation")]
    public void ForceHitAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("TakeHit");
            Debug.Log("🔧 Manually triggered Hit animation");
        }
    }
    
    [ContextMenu("Force Walk Animation")]
    public void ForceWalkAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", 2f);
            Debug.Log("🔧 Set Speed to 2.0 for walking");
        }
    }
    
    [ContextMenu("Force Idle Animation")]
    public void ForceIdleAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            Debug.Log("🔧 Set Speed to 0.0 for idle");
        }
    }
} 
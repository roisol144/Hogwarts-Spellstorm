using UnityEngine;

public class AnimationDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private KeyCode debugKey = KeyCode.F1;
    
    private Animator animator;
    private EnemyAnimationController animController;
    private UnityEngine.AI.NavMeshAgent navAgent;
    
    void Start()
    {
        // Get components
        animator = GetComponent<Animator>();
        animController = GetComponent<EnemyAnimationController>();
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        
        if (showDebugInfo)
        {
            Debug.Log($"[AnimationDebugger] {gameObject.name} - Components found:");
            Debug.Log($"  - Animator: {(animator != null ? "✅" : "❌")}");
            Debug.Log($"  - EnemyAnimationController: {(animController != null ? "✅" : "❌")}");
            Debug.Log($"  - NavMeshAgent: {(navAgent != null ? "✅" : "❌")}");
            
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                Debug.Log($"  - Animator Controller: ✅ {animator.runtimeAnimatorController.name}");
            }
            else
            {
                Debug.Log($"  - Animator Controller: ❌ Missing!");
            }
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            LogCurrentState();
        }
        
        if (showDebugInfo && animator != null)
        {
            // Log animation state changes
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            string currentStateName = GetStateName(stateInfo.shortNameHash);
            
            if (Time.frameCount % 120 == 0) // Every 2 seconds at 60fps
            {
                float speed = animController != null ? animController.GetCurrentSpeed() : 0f;
                bool isWalking = animController != null ? animController.IsWalking() : false;
                float navSpeed = navAgent != null ? navAgent.velocity.magnitude : 0f;
                
                Debug.Log($"[AnimationDebugger] {gameObject.name}:");
                Debug.Log($"  Current State: {currentStateName}");
                Debug.Log($"  Animation Speed: {speed:F2}");
                Debug.Log($"  Is Walking: {isWalking}");
                Debug.Log($"  NavAgent Speed: {navSpeed:F2}");
                Debug.Log($"  Animator Parameters - Speed: {animator.GetFloat("Speed"):F2}, IsWalking: {animator.GetBool("IsWalking")}");
            }
        }
    }
    
    void LogCurrentState()
    {
        if (animator == null) return;
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        string currentStateName = GetStateName(stateInfo.shortNameHash);
        
        Debug.Log($"[AnimationDebugger] === MANUAL DEBUG - {gameObject.name} ===");
        Debug.Log($"Current Animation State: {currentStateName}");
        Debug.Log($"State Time: {stateInfo.normalizedTime:F2}");
        Debug.Log($"Animation Speed Parameter: {animator.GetFloat("Speed"):F2}");
        Debug.Log($"Is Walking Parameter: {animator.GetBool("IsWalking")}");
        
        if (navAgent != null)
        {
            Debug.Log($"NavMeshAgent - Velocity: {navAgent.velocity.magnitude:F2}, isStopped: {navAgent.isStopped}");
        }
        
        if (animController != null)
        {
            Debug.Log($"EnemyAnimationController - Speed: {animController.GetCurrentSpeed():F2}, Walking: {animController.IsWalking()}");
        }
    }
    
    private string GetStateName(int shortNameHash)
    {
        if (shortNameHash == Animator.StringToHash("Idle"))
            return "Idle";
        else if (shortNameHash == Animator.StringToHash("Walking"))
            return "Walking";
        else
            return $"Unknown ({shortNameHash})";
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugInfo || navAgent == null) return;
        
        // Draw velocity vector
        Vector3 velocity = navAgent.velocity;
        if (velocity.magnitude > 0.1f)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + Vector3.up, velocity);
            Gizmos.DrawWireSphere(transform.position + Vector3.up + velocity, 0.2f);
        }
    }
} 
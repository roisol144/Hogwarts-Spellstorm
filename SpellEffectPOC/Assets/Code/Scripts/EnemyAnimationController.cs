using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls enemy animations based on NavMeshAgent movement
/// Connects movement velocity to walking animations for realistic humanoid movement
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    [Header("Animation Parameters")]
    [SerializeField] private string speedParameterName = "Speed";
    [SerializeField] private string isWalkingParameterName = "IsWalking";
    [SerializeField] private float walkSpeedThreshold = 0.1f;
    [SerializeField] private float animationSpeedMultiplier = 1f;
    [SerializeField] private float smoothingTime = 0.2f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    // Components
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    
    // Animation state
    private float currentSpeed;
    private float targetSpeed;
    private bool isWalking;
    
    // Animation parameter IDs (for performance)
    private int speedParameterID;
    private int isWalkingParameterID;
    
    void Awake()
    {
        // Get required components
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // Cache parameter IDs for better performance
        speedParameterID = Animator.StringToHash(speedParameterName);
        isWalkingParameterID = Animator.StringToHash(isWalkingParameterName);
        
        if (navMeshAgent == null)
        {
            Debug.LogError($"[EnemyAnimationController] No NavMeshAgent found on {gameObject.name}!");
        }
        
        if (animator == null)
        {
            Debug.LogError($"[EnemyAnimationController] No Animator found on {gameObject.name}!");
        }
    }
    
    void Update()
    {
        if (navMeshAgent == null || animator == null) return;
        
        UpdateAnimation();
    }
    
    void UpdateAnimation()
    {
        // Calculate movement speed based on NavMeshAgent velocity
        Vector3 velocity = navMeshAgent.velocity;
        targetSpeed = velocity.magnitude;
        
        // Smooth the speed transition
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / smoothingTime);
        
        // Determine if enemy is walking
        bool wasWalking = isWalking;
        isWalking = currentSpeed > walkSpeedThreshold && !navMeshAgent.isStopped;
        
        // Set animation parameters
        UpdateAnimatorParameters();
        
        // Debug logging
        if (showDebugInfo && wasWalking != isWalking)
        {
            Debug.Log($"[EnemyAnimationController] {gameObject.name} walking state changed: {isWalking} (speed: {currentSpeed:F2})");
        }
    }
    
    void UpdateAnimatorParameters()
    {
        // Set speed parameter (normalized 0-1 based on max agent speed)
        float normalizedSpeed = navMeshAgent.speed > 0 ? (currentSpeed / navMeshAgent.speed) * animationSpeedMultiplier : 0f;
        animator.SetFloat(speedParameterID, normalizedSpeed);
        
        // Set walking boolean
        animator.SetBool(isWalkingParameterID, isWalking);
        
        if (showDebugInfo)
        {
            Debug.Log($"[EnemyAnimationController] Speed: {normalizedSpeed:F2}, Walking: {isWalking}");
        }
    }
    
    /// <summary>
    /// Call this when the enemy stops moving (e.g., when dead or stunned)
    /// </summary>
    public void StopAnimation()
    {
        if (animator == null) return;
        
        currentSpeed = 0f;
        targetSpeed = 0f;
        isWalking = false;
        
        animator.SetFloat(speedParameterID, 0f);
        animator.SetBool(isWalkingParameterID, false);
        
        if (showDebugInfo)
        {
            Debug.Log($"[EnemyAnimationController] {gameObject.name} animations stopped");
        }
    }
    
    /// <summary>
    /// Public method to manually set walking state (for external control)
    /// </summary>
    public void SetWalking(bool walking)
    {
        isWalking = walking;
        animator.SetBool(isWalkingParameterID, walking);
    }
    
    /// <summary>
    /// Public method to manually set speed (for external control)
    /// </summary>
    public void SetSpeed(float speed)
    {
        targetSpeed = speed;
        float normalizedSpeed = navMeshAgent.speed > 0 ? (speed / navMeshAgent.speed) * animationSpeedMultiplier : 0f;
        animator.SetFloat(speedParameterID, normalizedSpeed);
    }
    
    /// <summary>
    /// Get current movement speed
    /// </summary>
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    
    /// <summary>
    /// Check if enemy is currently walking
    /// </summary>
    public bool IsWalking()
    {
        return isWalking;
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugInfo || navMeshAgent == null) return;
        
        // Draw velocity vector
        Vector3 velocity = navMeshAgent.velocity;
        if (velocity.magnitude > 0.1f)
        {
            Gizmos.color = isWalking ? Color.green : Color.yellow;
            Gizmos.DrawRay(transform.position + Vector3.up, velocity);
        }
        
        // Draw speed indicator
        Vector3 speedIndicatorPos = transform.position + Vector3.up * 3f;
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(speedIndicatorPos, Vector3.one * (currentSpeed * 0.5f));
    }
} 
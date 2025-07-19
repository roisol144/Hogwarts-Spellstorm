using UnityEngine;

public class TrollAnimationTest : MonoBehaviour
{
    [Header("Testing")]
    [SerializeField] private bool forceWalkingAnimation = false;
    [SerializeField] private bool forceIdleAnimation = false;
    [SerializeField] private float testSpeed = 1.0f;
    
    private Animator animator;
    private EnemyAnimationController enemyAnimController;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        enemyAnimController = GetComponent<EnemyAnimationController>();
        
        if (animator == null)
        {
            Debug.LogError("[TrollAnimationTest] No Animator component found!");
            return;
        }
        
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("[TrollAnimationTest] No Animator Controller assigned!");
            return;
        }
        
        Debug.Log($"[TrollAnimationTest] Animator found: {animator.runtimeAnimatorController.name}");
        Debug.Log($"[TrollAnimationTest] Parameters count: {animator.parameterCount}");
        
        // List all animator parameters
        for (int i = 0; i < animator.parameterCount; i++)
        {
            var param = animator.GetParameter(i);
            Debug.Log($"[TrollAnimationTest] Parameter {i}: {param.name} (Type: {param.type})");
        }
    }
    
    void Update()
    {
        if (animator == null) return;
        
        // Force animations for testing
        if (forceWalkingAnimation)
        {
            animator.SetFloat("Speed", testSpeed);
            animator.SetBool("IsWalking", true);
            Debug.Log($"[TrollAnimationTest] Forcing Walking animation - Speed: {testSpeed}");
        }
        else if (forceIdleAnimation)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsWalking", false);
            Debug.Log($"[TrollAnimationTest] Forcing Idle animation");
        }
        
        // Debug current state every 2 seconds
        if (Time.frameCount % 120 == 0)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            string currentState = "";
            
            if (stateInfo.IsName("Idle"))
                currentState = "Idle";
            else if (stateInfo.IsName("Walking"))
                currentState = "Walking";
            else
                currentState = $"Unknown ({stateInfo.shortNameHash})";
            
            Debug.Log($"[TrollAnimationTest] Current Animation State: {currentState}");
            Debug.Log($"[TrollAnimationTest] Speed Parameter: {animator.GetFloat("Speed"):F2}");
            Debug.Log($"[TrollAnimationTest] IsWalking Parameter: {animator.GetBool("IsWalking")}");
            Debug.Log($"[TrollAnimationTest] State Time: {stateInfo.normalizedTime:F2}");
        }
    }
    
    [ContextMenu("Test Idle Animation")]
    public void TestIdleAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsWalking", false);
            Debug.Log("[TrollAnimationTest] Testing Idle Animation");
        }
    }
    
    [ContextMenu("Test Walking Animation")]
    public void TestWalkingAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", 1f);
            animator.SetBool("IsWalking", true);
            Debug.Log("[TrollAnimationTest] Testing Walking Animation");
        }
    }
    
    [ContextMenu("Log Animation Info")]
    public void LogAnimationInfo()
    {
        if (animator == null)
        {
            Debug.LogError("[TrollAnimationTest] No Animator found!");
            return;
        }
        
        Debug.Log($"[TrollAnimationTest] === ANIMATION DEBUG INFO ===");
        Debug.Log($"Controller: {(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "NULL")}");
        Debug.Log($"Enabled: {animator.enabled}");
        Debug.Log($"GameObject Active: {gameObject.activeInHierarchy}");
        Debug.Log($"Animator Active: {animator.gameObject.activeInHierarchy}");
        
        if (animator.runtimeAnimatorController != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"Current State Hash: {stateInfo.shortNameHash}");
            Debug.Log($"Current State Time: {stateInfo.normalizedTime:F2}");
            Debug.Log($"Speed: {animator.GetFloat("Speed"):F2}");
            Debug.Log($"IsWalking: {animator.GetBool("IsWalking")}");
        }
    }
} 
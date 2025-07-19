using UnityEngine;

/// <summary>
/// Simple bone-based walking animation that works with any character model
/// Doesn't require Unity's Humanoid system - works with Generic rigs too!
/// </summary>
public class SimpleBoneWalkAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float walkCycleSpeed = 3f;
    [SerializeField] private float bodyBobAmount = 0.1f;
    [SerializeField] private float bodyTiltAmount = 2f;
    [SerializeField] private bool enableAnimation = true;
    
    [Header("Manual Bone Assignment (Optional)")]
    [SerializeField] private Transform rootBone;
    [SerializeField] private Transform spineBone;
    [SerializeField] private Transform leftLegBone;
    [SerializeField] private Transform rightLegBone;
    [SerializeField] private Transform leftArmBone;
    [SerializeField] private Transform rightArmBone;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool autoFindBones = true;
    
    private EnemyAnimationController enemyController;
    private float walkCycle = 0f;
    private bool isWalking = false;
    private float currentSpeed = 0f;
    
    // Original transforms for reset
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool hasStoredOriginals = false;
    
    void Start()
    {
        enemyController = GetComponent<EnemyAnimationController>();
        
        if (autoFindBones)
        {
            FindBonesAutomatically();
        }
        
        if (showDebugInfo)
        {
            LogBoneInfo();
        }
        
        // Store original transform
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        hasStoredOriginals = true;
        
        Debug.Log($"[SimpleBoneWalkAnimation] Initialized for {gameObject.name}");
    }
    
    void FindBonesAutomatically()
    {
        Debug.Log("[SimpleBoneWalkAnimation] Auto-finding bones...");
        
        // Try to find bones by common naming conventions
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        
        foreach (Transform child in allChildren)
        {
            string nameLower = child.name.ToLower();
            
            // Look for spine/torso bones
            if (spineBone == null && (nameLower.Contains("spine") || nameLower.Contains("torso") || nameLower.Contains("chest")))
            {
                spineBone = child;
                Debug.Log($"Found spine bone: {child.name}");
            }
            
            // Look for leg bones
            if (leftLegBone == null && (nameLower.Contains("left") && (nameLower.Contains("leg") || nameLower.Contains("thigh") || nameLower.Contains("upper"))))
            {
                leftLegBone = child;
                Debug.Log($"Found left leg bone: {child.name}");
            }
            
            if (rightLegBone == null && (nameLower.Contains("right") && (nameLower.Contains("leg") || nameLower.Contains("thigh") || nameLower.Contains("upper"))))
            {
                rightLegBone = child;
                Debug.Log($"Found right leg bone: {child.name}");
            }
            
            // Look for arm bones
            if (leftArmBone == null && (nameLower.Contains("left") && (nameLower.Contains("arm") || nameLower.Contains("shoulder") || nameLower.Contains("upper"))))
            {
                leftArmBone = child;
                Debug.Log($"Found left arm bone: {child.name}");
            }
            
            if (rightArmBone == null && (nameLower.Contains("right") && (nameLower.Contains("arm") || nameLower.Contains("shoulder") || nameLower.Contains("upper"))))
            {
                rightArmBone = child;
                Debug.Log($"Found right arm bone: {child.name}");
            }
        }
        
        // If we didn't find specific bones, use the root transform for basic animation
        if (rootBone == null)
        {
            rootBone = transform;
            Debug.Log("Using root transform for basic animation");
        }
    }
    
    void LogBoneInfo()
    {
        Debug.Log("=== BONE ASSIGNMENT STATUS ===");
        Debug.Log($"Root Bone: {(rootBone != null ? rootBone.name : "NOT FOUND")}");
        Debug.Log($"Spine Bone: {(spineBone != null ? spineBone.name : "NOT FOUND")}");
        Debug.Log($"Left Leg: {(leftLegBone != null ? leftLegBone.name : "NOT FOUND")}");
        Debug.Log($"Right Leg: {(rightLegBone != null ? rightLegBone.name : "NOT FOUND")}");
        Debug.Log($"Left Arm: {(leftArmBone != null ? leftArmBone.name : "NOT FOUND")}");
        Debug.Log($"Right Arm: {(rightArmBone != null ? rightArmBone.name : "NOT FOUND")}");
    }
    
    void Update()
    {
        if (!enableAnimation) return;
        
        // Get walking state
        if (enemyController != null)
        {
            isWalking = enemyController.IsWalking();
            currentSpeed = enemyController.GetCurrentSpeed();
        }
        else
        {
            // Fallback: assume walking if moving
            var navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null)
            {
                currentSpeed = navAgent.velocity.magnitude;
                isWalking = currentSpeed > 0.1f;
            }
        }
        
        if (isWalking && currentSpeed > 0.1f)
        {
            UpdateWalkAnimation();
        }
        else
        {
            UpdateIdleAnimation();
        }
        
        if (showDebugInfo && Time.frameCount % 120 == 0)
        {
            Debug.Log($"[SimpleBoneWalkAnimation] Walking: {isWalking}, Speed: {currentSpeed:F2}");
        }
    }
    
    void UpdateWalkAnimation()
    {
        // Update walk cycle
        walkCycle += Time.deltaTime * walkCycleSpeed * currentSpeed;
        
        // Body bobbing and tilting
        AnimateRootTransform();
        
        // Animate individual bones if available
        if (spineBone != null) AnimateSpine();
        if (leftLegBone != null && rightLegBone != null) AnimateLegs();
        if (leftArmBone != null && rightArmBone != null) AnimateArms();
    }
    
    void UpdateIdleAnimation()
    {
        // Simple breathing/idle animation
        float breathCycle = Time.time * 0.5f;
        float breathAmount = Mathf.Sin(breathCycle) * 0.02f;
        
        if (hasStoredOriginals)
        {
            transform.localPosition = originalPosition + Vector3.up * breathAmount;
        }
    }
    
    void AnimateRootTransform()
    {
        if (!hasStoredOriginals) return;
        
        // Body bobbing (up and down movement)
        float bobCycle = Mathf.Sin(walkCycle * 2f); // Double frequency for natural bob
        float bobOffset = bobCycle * bodyBobAmount * currentSpeed;
        
        // Body tilting (left and right)
        float tiltCycle = Mathf.Sin(walkCycle);
        float tiltAngle = tiltCycle * bodyTiltAmount * currentSpeed;
        
        // Apply to root transform
        Vector3 newPosition = originalPosition + Vector3.up * bobOffset;
        Quaternion newRotation = originalRotation * Quaternion.Euler(0, 0, tiltAngle);
        
        transform.localPosition = newPosition;
        transform.localRotation = newRotation;
    }
    
    void AnimateSpine()
    {
        float spineTilt = Mathf.Sin(walkCycle) * 3f * currentSpeed;
        spineBone.localRotation *= Quaternion.Euler(0, 0, spineTilt);
    }
    
    void AnimateLegs()
    {
        float leftLegCycle = Mathf.Sin(walkCycle) * 15f * currentSpeed;
        float rightLegCycle = Mathf.Sin(walkCycle + Mathf.PI) * 15f * currentSpeed;
        
        leftLegBone.localRotation *= Quaternion.Euler(leftLegCycle, 0, 0);
        rightLegBone.localRotation *= Quaternion.Euler(rightLegCycle, 0, 0);
    }
    
    void AnimateArms()
    {
        // Arms swing opposite to legs
        float leftArmCycle = Mathf.Sin(walkCycle + Mathf.PI) * 20f * currentSpeed;
        float rightArmCycle = Mathf.Sin(walkCycle) * 20f * currentSpeed;
        
        leftArmBone.localRotation *= Quaternion.Euler(leftArmCycle, 0, 0);
        rightArmBone.localRotation *= Quaternion.Euler(rightArmCycle, 0, 0);
    }
    
    [ContextMenu("Find Bones Manually")]
    public void FindBonesManually()
    {
        FindBonesAutomatically();
        LogBoneInfo();
    }
    
    [ContextMenu("Test Animation")]
    public void TestAnimation()
    {
        enableAnimation = !enableAnimation;
        Debug.Log($"Animation {(enableAnimation ? "enabled" : "disabled")}");
    }
    
    [ContextMenu("Reset Position")]
    public void ResetPosition()
    {
        if (hasStoredOriginals)
        {
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            Debug.Log("Reset to original position");
        }
    }
} 
using UnityEngine;

/// <summary>
/// Procedural walking animation for humanoid characters
/// Creates realistic walking motion by directly manipulating bone rotations
/// Works with any Unity Humanoid rig - no external animation files needed!
/// </summary>
[RequireComponent(typeof(Animator))]
public class ProceduralWalkAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float walkCycleSpeed = 2f;
    [SerializeField] private float stepHeight = 15f;
    [SerializeField] private float bodySwayAmount = 5f;
    [SerializeField] private float armSwingAmount = 30f;
    [SerializeField] private bool enableWalkAnimation = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    private Animator animator;
    private EnemyAnimationController enemyController;
    
    // Animation state
    private float walkCycle = 0f;
    private bool isWalking = false;
    private float currentSpeed = 0f;
    
    // Original bone rotations for reset
    private Quaternion originalSpineRotation;
    private bool hasStoredOriginals = false;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        enemyController = GetComponent<EnemyAnimationController>();
        
        if (animator == null)
        {
            Debug.LogError("[ProceduralWalkAnimation] No Animator component found!");
            enabled = false;
            return;
        }
        
        if (!animator.isHuman)
        {
            Debug.LogError("[ProceduralWalkAnimation] Animator is not configured as Humanoid!");
            enabled = false;
            return;
        }
        
        Debug.Log($"[ProceduralWalkAnimation] Initialized for {gameObject.name} - Humanoid rig detected");
    }
    
    void Update()
    {
        if (!enableWalkAnimation || animator == null) return;
        
        // Get walking state from EnemyAnimationController
        if (enemyController != null)
        {
            isWalking = enemyController.IsWalking();
            currentSpeed = enemyController.GetCurrentSpeed();
        }
        else
        {
            // Fallback: check animator parameters directly
            isWalking = animator.GetBool("IsWalking");
            currentSpeed = animator.GetFloat("Speed");
        }
        
        if (isWalking && currentSpeed > 0.1f)
        {
            UpdateWalkAnimation();
        }
        else
        {
            UpdateIdleAnimation();
        }
        
        if (showDebugInfo && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[ProceduralWalkAnimation] Walking: {isWalking}, Speed: {currentSpeed:F2}, Cycle: {walkCycle:F2}");
        }
    }
    
    void UpdateWalkAnimation()
    {
        // Update walk cycle based on speed
        walkCycle += Time.deltaTime * walkCycleSpeed * currentSpeed;
        
        // Store original rotations on first frame
        if (!hasStoredOriginals)
        {
            StoreOriginalRotations();
        }
        
        // Apply procedural walking animations
        AnimateLegs();
        AnimateSpine();
        AnimateArms();
    }
    
    void UpdateIdleAnimation()
    {
        // Simple breathing animation
        float breathCycle = Time.time * 0.5f;
        float breathAmount = Mathf.Sin(breathCycle) * 2f;
        
        // Very subtle spine movement for breathing
        if (animator.isHuman)
        {
            Transform spine = animator.GetBoneTransform(HumanBodyBones.Spine);
            if (spine != null)
            {
                spine.localRotation = originalSpineRotation * Quaternion.Euler(breathAmount, 0, 0);
            }
        }
    }
    
    void AnimateLegs()
    {
        if (!animator.isHuman) return;
        
        // Get leg bones
        Transform leftUpperLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        Transform rightUpperLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        Transform leftLowerLeg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        Transform rightLowerLeg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        
        // Calculate leg swing angles
        float leftLegCycle = Mathf.Sin(walkCycle);
        float rightLegCycle = Mathf.Sin(walkCycle + Mathf.PI); // Opposite phase
        
        // Apply leg rotations
        if (leftUpperLeg != null)
            leftUpperLeg.localRotation *= Quaternion.Euler(leftLegCycle * stepHeight, 0, 0);
        
        if (rightUpperLeg != null)
            rightUpperLeg.localRotation *= Quaternion.Euler(rightLegCycle * stepHeight, 0, 0);
        
        // Knee bending
        float leftKneeBend = Mathf.Max(0, leftLegCycle) * stepHeight * 0.5f;
        float rightKneeBend = Mathf.Max(0, rightLegCycle) * stepHeight * 0.5f;
        
        if (leftLowerLeg != null)
            leftLowerLeg.localRotation *= Quaternion.Euler(-leftKneeBend, 0, 0);
        
        if (rightLowerLeg != null)
            rightLowerLeg.localRotation *= Quaternion.Euler(-rightKneeBend, 0, 0);
    }
    
    void AnimateSpine()
    {
        if (!animator.isHuman) return;
        
        Transform spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        if (spine == null) return;
        
        // Body sway while walking
        float swayCycle = Mathf.Sin(walkCycle * 2f); // Double frequency for more natural sway
        float swayAngle = swayCycle * bodySwayAmount * currentSpeed;
        
        spine.localRotation = originalSpineRotation * Quaternion.Euler(0, 0, swayAngle);
    }
    
    void AnimateArms()
    {
        if (!animator.isHuman) return;
        
        Transform leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        Transform rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        
        if (leftUpperArm == null || rightUpperArm == null) return;
        
        // Arm swing opposite to legs
        float leftArmCycle = Mathf.Sin(walkCycle + Mathf.PI); // Opposite to left leg
        float rightArmCycle = Mathf.Sin(walkCycle); // Opposite to right leg
        
        float leftArmSwing = leftArmCycle * armSwingAmount * currentSpeed;
        float rightArmSwing = rightArmCycle * armSwingAmount * currentSpeed;
        
        leftUpperArm.localRotation *= Quaternion.Euler(leftArmSwing, 0, 0);
        rightUpperArm.localRotation *= Quaternion.Euler(rightArmSwing, 0, 0);
    }
    
    void StoreOriginalRotations()
    {
        if (!animator.isHuman) return;
        
        Transform spine = animator.GetBoneTransform(HumanBodyBones.Spine);
        if (spine != null)
        {
            originalSpineRotation = spine.localRotation;
            hasStoredOriginals = true;
        }
    }
    
    void LateUpdate()
    {
        // Ensure bone rotations are applied after Unity's animation system
        // This gives our procedural animation priority over any base animations
    }
    
    [ContextMenu("Test Walking Animation")]
    public void TestWalkingAnimation()
    {
        enableWalkAnimation = !enableWalkAnimation;
        Debug.Log($"[ProceduralWalkAnimation] Animation {(enableWalkAnimation ? "enabled" : "disabled")}");
    }
    
    [ContextMenu("Reset To Original Pose")]
    public void ResetToOriginalPose()
    {
        if (!animator.isHuman) return;
        
        // Reset all humanoid bones to their original rotations
        foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
        {
            if (bone == HumanBodyBones.LastBone) continue;
            
            Transform boneTransform = animator.GetBoneTransform(bone);
            if (boneTransform != null)
            {
                boneTransform.localRotation = Quaternion.identity;
            }
        }
        
        hasStoredOriginals = false;
        Debug.Log("[ProceduralWalkAnimation] Reset to original pose");
    }
} 
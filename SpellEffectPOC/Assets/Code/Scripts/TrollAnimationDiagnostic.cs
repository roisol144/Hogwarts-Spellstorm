using UnityEngine;

public class TrollAnimationDiagnostic : MonoBehaviour
{
    [Header("Diagnostic Controls")]
    [SerializeField] private bool runDiagnosticOnStart = true;
    [SerializeField] private bool continuousMonitoring = true;
    
    private Animator animator;
    private ProceduralWalkAnimation proceduralWalk;
    private EnemyAnimationController enemyController;
    
    void Start()
    {
        if (runDiagnosticOnStart)
        {
            RunFullDiagnostic();
        }
    }
    
    void Update()
    {
        if (continuousMonitoring && Time.frameCount % 120 == 0) // Every 2 seconds
        {
            QuickStatusCheck();
        }
    }
    
    [ContextMenu("Run Full Diagnostic")]
    public void RunFullDiagnostic()
    {
        Debug.Log("=== TROLL ANIMATION DIAGNOSTIC START ===");
        
        // Get all components
        animator = GetComponent<Animator>();
        proceduralWalk = GetComponent<ProceduralWalkAnimation>();
        enemyController = GetComponent<EnemyAnimationController>();
        
        CheckAnimatorSetup();
        CheckHumanoidBones();
        CheckProceduralAnimation();
        CheckMovementController();
        CheckAnimatorController();
        
        Debug.Log("=== TROLL ANIMATION DIAGNOSTIC END ===");
    }
    
    void CheckAnimatorSetup()
    {
        Debug.Log("--- ANIMATOR SETUP CHECK ---");
        
        if (animator == null)
        {
            Debug.LogError("❌ NO ANIMATOR COMPONENT FOUND!");
            return;
        }
        
        Debug.Log($"✅ Animator Component: Found");
        Debug.Log($"Enabled: {animator.enabled}");
        Debug.Log($"GameObject Active: {gameObject.activeInHierarchy}");
        
        if (animator.avatar == null)
        {
            Debug.LogError("❌ NO AVATAR ASSIGNED TO ANIMATOR!");
            return;
        }
        
        Debug.Log($"✅ Avatar: {animator.avatar.name}");
        Debug.Log($"Is Human: {animator.isHuman}");
        Debug.Log($"Is Valid: {animator.avatar.isValid}");
        Debug.Log($"Avatar Human: {animator.avatar.isHuman}");
        
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("❌ NO ANIMATOR CONTROLLER ASSIGNED!");
        }
        else
        {
            Debug.Log($"✅ Controller: {animator.runtimeAnimatorController.name}");
        }
    }
    
    void CheckHumanoidBones()
    {
        Debug.Log("--- HUMANOID BONES CHECK ---");
        
        if (animator == null || !animator.isHuman)
        {
            Debug.LogError("❌ ANIMATOR IS NOT HUMANOID - Cannot check bones");
            return;
        }
        
        // Check critical bones for walking animation
        CheckBone(HumanBodyBones.Hips, "Hips (Root)");
        CheckBone(HumanBodyBones.Spine, "Spine");
        CheckBone(HumanBodyBones.LeftUpperLeg, "Left Upper Leg");
        CheckBone(HumanBodyBones.RightUpperLeg, "Right Upper Leg");
        CheckBone(HumanBodyBones.LeftLowerLeg, "Left Lower Leg");
        CheckBone(HumanBodyBones.RightLowerLeg, "Right Lower Leg");
        CheckBone(HumanBodyBones.LeftFoot, "Left Foot");
        CheckBone(HumanBodyBones.RightFoot, "Right Foot");
        CheckBone(HumanBodyBones.LeftUpperArm, "Left Upper Arm");
        CheckBone(HumanBodyBones.RightUpperArm, "Right Upper Arm");
    }
    
    void CheckBone(HumanBodyBones bone, string boneName)
    {
        Transform boneTransform = animator.GetBoneTransform(bone);
        if (boneTransform != null)
        {
            Debug.Log($"✅ {boneName}: {boneTransform.name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ {boneName}: NOT FOUND");
        }
    }
    
    void CheckProceduralAnimation()
    {
        Debug.Log("--- PROCEDURAL ANIMATION CHECK ---");
        
        if (proceduralWalk == null)
        {
            Debug.LogError("❌ ProceduralWalkAnimation COMPONENT NOT FOUND!");
            return;
        }
        
        Debug.Log($"✅ ProceduralWalkAnimation: Found");
        Debug.Log($"Enabled: {proceduralWalk.enabled}");
        
        // Try to access some fields via reflection to check state
        var fields = typeof(ProceduralWalkAnimation).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (field.Name == "enableWalkAnimation")
            {
                bool enabled = (bool)field.GetValue(proceduralWalk);
                Debug.Log($"Walk Animation Enabled: {enabled}");
            }
        }
    }
    
    void CheckMovementController()
    {
        Debug.Log("--- MOVEMENT CONTROLLER CHECK ---");
        
        if (enemyController == null)
        {
            Debug.LogWarning("⚠️ EnemyAnimationController NOT FOUND!");
            return;
        }
        
        Debug.Log($"✅ EnemyAnimationController: Found");
        Debug.Log($"Enabled: {enemyController.enabled}");
        Debug.Log($"Current Speed: {enemyController.GetCurrentSpeed():F2}");
        Debug.Log($"Is Walking: {enemyController.IsWalking()}");
        
        // Check NavMeshAgent
        var navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            Debug.Log($"✅ NavMeshAgent: Found");
            Debug.Log($"Agent Speed: {navAgent.speed}");
            Debug.Log($"Velocity: {navAgent.velocity.magnitude:F2}");
            Debug.Log($"Is Stopped: {navAgent.isStopped}");
        }
        else
        {
            Debug.LogWarning("⚠️ NavMeshAgent NOT FOUND!");
        }
    }
    
    void CheckAnimatorController()
    {
        Debug.Log("--- ANIMATOR CONTROLLER CHECK ---");
        
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogError("❌ Cannot check animator controller - missing components");
            return;
        }
        
        Debug.Log($"Parameters count: {animator.parameterCount}");
        
        for (int i = 0; i < animator.parameterCount; i++)
        {
            var param = animator.GetParameter(i);
            Debug.Log($"Parameter {i}: {param.name} (Type: {param.type})");
            
            if (param.name == "Speed")
            {
                Debug.Log($"  Current Speed Value: {animator.GetFloat("Speed"):F2}");
            }
            else if (param.name == "IsWalking")
            {
                Debug.Log($"  Current IsWalking Value: {animator.GetBool("IsWalking")}");
            }
        }
        
        // Check current animator state
        if (animator.layerCount > 0)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"Current Animation State Hash: {stateInfo.shortNameHash}");
            Debug.Log($"State Normal Time: {stateInfo.normalizedTime:F2}");
        }
    }
    
    void QuickStatusCheck()
    {
        if (animator == null) return;
        
        string status = "[TrollDiagnostic] ";
        status += $"Human: {animator.isHuman} | ";
        
        if (enemyController != null)
        {
            status += $"Walking: {enemyController.IsWalking()} | ";
            status += $"Speed: {enemyController.GetCurrentSpeed():F2} | ";
        }
        
        if (proceduralWalk != null)
        {
            status += $"ProcAnim: {proceduralWalk.enabled}";
        }
        
        Debug.Log(status);
    }
    
    [ContextMenu("Force Test Walking")]
    public void ForceTestWalking()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", 1.0f);
            animator.SetBool("IsWalking", true);
            Debug.Log("Forced walking parameters set");
        }
        
        if (proceduralWalk != null)
        {
            // Try to enable procedural walking
            var field = typeof(ProceduralWalkAnimation).GetField("enableWalkAnimation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(proceduralWalk, true);
                Debug.Log("Forced procedural animation enabled");
            }
        }
    }
} 
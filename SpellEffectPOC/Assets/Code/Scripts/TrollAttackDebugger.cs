using UnityEngine;

public class TrollAttackDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showGUIDebugInfo = true;
    [SerializeField] private float logInterval = 1f;
    
    private EnemyAttack enemyAttack;
    private TrollAnimationController animController;
    private Animator animator;
    private float lastLogTime;
    
    void Start()
    {
        enemyAttack = GetComponent<EnemyAttack>();
        animController = GetComponent<TrollAnimationController>();
        animator = GetComponent<Animator>();
        
        if (enableDebugLogs)
        {
            Debug.Log($"🐛 TrollAttackDebugger initialized for {gameObject.name}");
            LogComponentStatus();
        }
    }
    
    void Update()
    {
        if (enableDebugLogs && Time.time - lastLogTime > logInterval)
        {
            LogAttackStatus();
            lastLogTime = Time.time;
        }
    }
    
    void LogComponentStatus()
    {
        Debug.Log($"📊 Component Status for {gameObject.name}:");
        Debug.Log($"   EnemyAttack: {(enemyAttack != null ? "✅" : "❌")}");
        Debug.Log($"   TrollAnimationController: {(animController != null ? "✅" : "❌")}");
        Debug.Log($"   Animator: {(animator != null ? "✅" : "❌")}");
        
        if (enemyAttack != null)
        {
            Debug.Log($"   Attack Type: {enemyAttack.GetAttackType()}");
            Debug.Log($"   Attack Range: {enemyAttack.GetAttackRange()}");
        }
        
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            Debug.Log($"   Animator Controller: {animator.runtimeAnimatorController.name}");
        }
    }
    
    void LogAttackStatus()
    {
        if (enemyAttack == null || animController == null) return;
        
        var player = GameObject.FindGameObjectWithTag("Player");
        float distanceToPlayer = player != null ? Vector3.Distance(transform.position, player.transform.position) : -1f;
        
        string status = $"🔍 {gameObject.name} Status:\n";
        status += $"   Distance to Player: {distanceToPlayer:F2}\n";
        status += $"   Can Attack: {enemyAttack.CanAttack()}\n";
        status += $"   Is Attacking: {enemyAttack.IsAttacking()}\n";
        status += $"   Anim Is Attacking: {animController.IsAttacking()}\n";
        status += $"   Anim Is Taking Hit: {animController.IsTakingHit()}\n";
        status += $"   Anim Is Dead: {animController.IsDead()}\n";
        
        if (animator != null)
        {
            status += $"   Anim Speed: {animator.GetFloat("Speed"):F2}\n";
            status += $"   Current State: {GetCurrentAnimationState()}\n";
        }
        
        Debug.Log(status);
    }
    
    string GetCurrentAnimationState()
    {
        if (animator == null) return "No Animator";
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName("Idle") ? "Idle" :
               stateInfo.IsName("Walk") ? "Walk" :
               stateInfo.IsName("Attack1") ? "Attack1" :
               stateInfo.IsName("Attack2") ? "Attack2" :
               stateInfo.IsName("TakeDamage") ? "TakeDamage" :
               stateInfo.IsName("Death") ? "Death" :
               "Unknown";
    }
    
    void OnGUI()
    {
        if (!showGUIDebugInfo) return;
        
        GUI.color = Color.white;
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"Troll: {gameObject.name}", GUI.skin.box);
        
        if (enemyAttack != null)
        {
            GUILayout.Label($"Attack Type: {enemyAttack.GetAttackType()}");
            GUILayout.Label($"Can Attack: {enemyAttack.CanAttack()}");
            GUILayout.Label($"Is Attacking: {enemyAttack.IsAttacking()}");
        }
        
        if (animController != null)
        {
            GUILayout.Label($"Anim Attacking: {animController.IsAttacking()}");
        }
        
        if (animator != null)
        {
            GUILayout.Label($"Current State: {GetCurrentAnimationState()}");
        }
        
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            GUILayout.Label($"Distance: {dist:F2}");
        }
        
        GUILayout.EndArea();
    }
    
    [ContextMenu("Force Attack Test")]
    public void ForceAttackTest()
    {
        if (animController != null)
        {
            animController.TriggerAttack();
            Debug.Log($"🧪 Forced attack animation trigger for {gameObject.name}");
        }
        else
        {
            Debug.LogError($"❌ No TrollAnimationController found on {gameObject.name}");
        }
    }
    
    [ContextMenu("Log Full Status")]
    public void LogFullStatus()
    {
        LogComponentStatus();
        LogAttackStatus();
    }
} 
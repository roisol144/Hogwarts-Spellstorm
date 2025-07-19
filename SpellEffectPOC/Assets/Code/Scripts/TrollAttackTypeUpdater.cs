using UnityEngine;

public class TrollAttackTypeUpdater : MonoBehaviour
{
    [Header("Troll Attack Type Updater")]
    [SerializeField] private bool updateOnStart = true;
    
    void Start()
    {
        if (updateOnStart)
        {
            UpdateTrollAttackType();
        }
    }
    
    [ContextMenu("Update Troll Attack Type")]
    public void UpdateTrollAttackType()
    {
        var enemyAttack = GetComponent<EnemyAttack>();
        if (enemyAttack != null)
        {
            enemyAttack.SetAttackType(EnemyAttack.AttackType.AnimationOnly);
            Debug.Log($"✅ {gameObject.name} updated to use AnimationOnly attack (no tackling)");
        }
        else
        {
            Debug.LogWarning($"❌ {gameObject.name} has no EnemyAttack component!");
        }
        
        // Also fix movement wobbling
        var enemyMovement = GetComponent<EnemyMovement>();
        if (enemyMovement != null)
        {
            enemyMovement.wobbleAmount = 0f;
            enemyMovement.wobbleSpeed = 0f;
            Debug.Log($"✅ {gameObject.name} movement updated to face player directly (no wobbling)");
        }
        
        // Disable this script after use
        this.enabled = false;
    }
    
    [ContextMenu("Update All Trolls in Scene")]
    public void UpdateAllTrollsInScene()
    {
        // Find all trolls in scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int trollsUpdated = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("troll"))
            {
                var enemyAttack = obj.GetComponent<EnemyAttack>();
                if (enemyAttack != null)
                {
                    enemyAttack.SetAttackType(EnemyAttack.AttackType.AnimationOnly);
                    Debug.Log($"✅ {obj.name} updated to AnimationOnly attack");
                    
                    // Also fix movement wobbling
                    var enemyMovement = obj.GetComponent<EnemyMovement>();
                    if (enemyMovement != null)
                    {
                        enemyMovement.wobbleAmount = 0f;
                        enemyMovement.wobbleSpeed = 0f;
                        Debug.Log($"✅ {obj.name} movement fixed (no wobbling)");
                    }
                    
                    trollsUpdated++;
                }
            }
        }
        
        Debug.Log($"🎉 Updated {trollsUpdated} trolls in scene to use AnimationOnly attacks!");
    }
} 
using UnityEngine;

public class Dementor : Enemy
{
    [Header("Dementor Specific Effects")]
    [SerializeField] private bool useDementorShaderDissolve = true;
    [SerializeField] private AudioClip dementorDeathSound;
    [SerializeField] private Material dementorDissolveMaterial; // Assign dissolve material from ShaderGraph_Dissolve
    [SerializeField] private Vector3 dementorDissolveDirection = Vector3.up; // Dissolve from bottom to top
    [SerializeField] private Color dementorEdgeColor = new Color(0.8f, 0.2f, 1f, 1f); // Purple edge color for dementor
    
    private SafeShaderDissolveEffect dementorShaderDissolveEffect;

    protected override void Start()
    {
        base.Start();
        
        // Setup dementor-specific shader dissolving effect
        if (useDementorShaderDissolve)
        {
            dementorShaderDissolveEffect = GetComponent<SafeShaderDissolveEffect>();
            if (dementorShaderDissolveEffect == null)
            {
                dementorShaderDissolveEffect = gameObject.AddComponent<SafeShaderDissolveEffect>();
            }

            // Configure dementor-specific dissolve settings
            ConfigureDementorDissolve();
        }
    }

    private void ConfigureDementorDissolve()
    {
        if (dementorShaderDissolveEffect != null)
        {
            // Set dementor-specific dissolve material if provided
            if (dementorDissolveMaterial != null)
            {
                dementorShaderDissolveEffect.DissolveMaterial = dementorDissolveMaterial;
            }

            // Set dementor-specific edge color
            dementorShaderDissolveEffect.EdgeColor = dementorEdgeColor;

            // Force refresh material setup since we set the material after Awake()
            dementorShaderDissolveEffect.RefreshMaterialSetup();

            Debug.Log($"[Dementor] Configured dissolve effect with material: {dementorDissolveMaterial?.name} and edge color: {dementorEdgeColor}");
        }
    }

    public override void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"Dementor {gameObject.name} died with dissolving effect!");

        // Hide health bar
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance.gameObject);
        }

        // Spawn death effect
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // Use dementor-specific shader dissolving effect if available
        if (useDementorShaderDissolve && dementorShaderDissolveEffect != null)
        {
            Debug.Log($"[Dementor] Starting shader dissolving effect for {gameObject.name}");
            
            // Start the shader dissolving effect (this will handle NavMeshAgent and collider disabling)
            dementorShaderDissolveEffect.StartDissolving();
        }
        else
        {
            // Fallback to base class behavior
            base.Die();
        }
    }
} 
using UnityEngine;

public class Troll : Enemy
{
    [Header("Troll Specific Effects")]
    [SerializeField] private bool useTrollShaderDissolve = true;
    [SerializeField] private AudioClip trollDeathSound;
    [SerializeField] private Material trollDissolveMaterial; // Assign dissolve material from ShaderGraph_Dissolve
    [SerializeField] private Vector3 trollDissolveDirection = Vector3.up; // Dissolve from bottom to top
    [SerializeField] private Color trollEdgeColor = new Color(1f, 0.4f, 0.1f, 1f); // Orange/fiery edge color for troll
    
    [Header("Health Bar Positioning")]
    [SerializeField] [Range(0f, 8f)] private float headOffsetUp = 2.5f; // How high above the head (trolls are tall)
    [SerializeField] [Range(0f, 5f)] private float headOffsetForward = 0.2f; // How far forward toward the head
    
    private SafeShaderDissolveEffect trollShaderDissolveEffect;
    private AudioSource audioSource;

    protected override void Start()
    {
        base.Start();
        
        // Setup audio source for troll death sound
        SetupAudioSource();
        
        // Setup troll-specific shader dissolving effect
        if (useTrollShaderDissolve)
        {
            trollShaderDissolveEffect = GetComponent<SafeShaderDissolveEffect>();
            if (trollShaderDissolveEffect == null)
            {
                trollShaderDissolveEffect = gameObject.AddComponent<SafeShaderDissolveEffect>();
            }

            // Configure troll-specific dissolve settings
            ConfigureTrollDissolve();
        }
    }

    protected override Vector3 GetHealthBarPosition()
    {
        // For troll, position health bar high above the head (trolls are very tall)
        // Use editor-configurable offsets for precise positioning
        Vector3 headOffset = transform.forward * headOffsetForward; // Move slightly toward the front
        return transform.position + headOffset + Vector3.up * headOffsetUp; // High position for troll height
    }

    private void SetupAudioSource()
    {
        // Setup audio source if not assigned
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("[Troll] Added AudioSource component");
        }

        // Load troll death sound if not assigned
        if (trollDeathSound == null)
        {
            trollDeathSound = Resources.Load<AudioClip>("Sounds/enemy_dissolve");
            if (trollDeathSound != null)
            {
                Debug.Log("[Troll] Loaded troll death sound from Resources");
            }
            else
            {
                Debug.LogWarning("[Troll] Could not load troll death sound from Resources/Sounds/enemy_dissolve");
            }
        }
    }

    private void ConfigureTrollDissolve()
    {
        if (trollShaderDissolveEffect != null)
        {
            // Set troll-specific dissolve material if provided
            if (trollDissolveMaterial != null)
            {
                trollShaderDissolveEffect.DissolveMaterial = trollDissolveMaterial;
            }

            // Set troll-specific edge color
            trollShaderDissolveEffect.EdgeColor = trollEdgeColor;

            // Force refresh material setup since we set the material after Awake()
            trollShaderDissolveEffect.RefreshMaterialSetup();

            Debug.Log($"[Troll] Configured dissolve effect with material: {trollDissolveMaterial?.name} and edge color: {trollEdgeColor}");
        }
    }

    private void PlayTrollDeathSound()
    {
        if (audioSource != null && trollDeathSound != null)
        {
            audioSource.PlayOneShot(trollDeathSound);
            Debug.Log("[Troll] Playing troll death sound effect");
        }
        else
        {
            Debug.LogWarning("[Troll] Cannot play troll death sound - missing AudioSource or AudioClip");
        }
    }

    public override void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"Troll {gameObject.name} died with dissolving effect!");

        // Award score for kill (same as base Enemy class)
        ScoreManager.NotifyKill();

        // Play troll death sound
        PlayTrollDeathSound();

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

        // Use troll-specific shader dissolving effect if available
        if (useTrollShaderDissolve && trollShaderDissolveEffect != null)
        {
            Debug.Log($"[Troll] Starting shader dissolving effect for {gameObject.name}");
            
            // Force refresh the material setup to ensure it's properly configured
            trollShaderDissolveEffect.RefreshMaterialSetup();
            
            // Start the shader dissolving effect (this will handle NavMeshAgent and collider disabling)
            trollShaderDissolveEffect.StartDissolving();
        }
        else
        {
            Debug.LogWarning($"[Troll] Shader dissolve effect not available - useTrollShaderDissolve: {useTrollShaderDissolve}, component: {trollShaderDissolveEffect != null}");
            // Fallback to base class behavior
            base.Die();
        }
    }
} 
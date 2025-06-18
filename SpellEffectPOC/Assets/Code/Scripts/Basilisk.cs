using UnityEngine;

public class Basilisk : Enemy
{
    [Header("Basilisk Specific Effects")]
    [SerializeField] private bool useBasiliskShaderDissolve = true;
    [SerializeField] private AudioClip basiliskDeathSound;
    [SerializeField] private Material basiliskDissolveMaterial; // Assign dissolve material from ShaderGraph_Dissolve
    [SerializeField] private Vector3 basiliskDissolveDirection = Vector3.up; // Dissolve from bottom to top
    [SerializeField] private Color basiliskEdgeColor = new Color(0.2f, 0.8f, 0.3f, 1f); // Green edge color for basilisk
    
    [Header("Health Bar Positioning")]
    [SerializeField] [Range(0f, 8f)] private float headOffsetForward = 0.15f; // How far forward toward the head
    [SerializeField] [Range(0f, 5f)] private float headOffsetUp = 0.08f; // How high above the head
    
    private SafeShaderDissolveEffect basiliskShaderDissolveEffect;
    private AudioSource audioSource;

    protected override void Start()
    {
        base.Start();
        
        // Setup audio source for basilisk death sound
        SetupAudioSource();
        
        // Setup basilisk-specific shader dissolving effect
        if (useBasiliskShaderDissolve)
        {
            basiliskShaderDissolveEffect = GetComponent<SafeShaderDissolveEffect>();
            if (basiliskShaderDissolveEffect == null)
            {
                basiliskShaderDissolveEffect = gameObject.AddComponent<SafeShaderDissolveEffect>();
            }

            // Configure basilisk-specific dissolve settings
            ConfigureBasiliskDissolve();
        }
    }

    protected override Vector3 GetHealthBarPosition()
    {
        // For basilisk, position health bar above the head (front of the model)
        // Use editor-configurable offsets for precise positioning
        Vector3 headOffset = transform.forward * headOffsetForward; // Move toward the head
        return transform.position + headOffset + Vector3.up * headOffsetUp; // Higher position for basilisk scale
    }

    private void SetupAudioSource()
    {
        // Setup audio source if not assigned
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("[Basilisk] Added AudioSource component");
        }

        // Load basilisk death sound if not assigned
        if (basiliskDeathSound == null)
        {
            basiliskDeathSound = Resources.Load<AudioClip>("Sounds/enemy_dissolve");
            if (basiliskDeathSound != null)
            {
                Debug.Log("[Basilisk] Loaded basilisk death sound from Resources");
            }
            else
            {
                Debug.LogWarning("[Basilisk] Could not load basilisk death sound from Resources/Sounds/enemy_dissolve");
            }
        }
    }

    private void ConfigureBasiliskDissolve()
    {
        if (basiliskShaderDissolveEffect != null)
        {
            // Set basilisk-specific dissolve material if provided
            if (basiliskDissolveMaterial != null)
            {
                basiliskShaderDissolveEffect.DissolveMaterial = basiliskDissolveMaterial;
            }

            // Set basilisk-specific edge color
            basiliskShaderDissolveEffect.EdgeColor = basiliskEdgeColor;

            // Force refresh material setup since we set the material after Awake()
            basiliskShaderDissolveEffect.RefreshMaterialSetup();

            Debug.Log($"[Basilisk] Configured dissolve effect with material: {basiliskDissolveMaterial?.name} and edge color: {basiliskEdgeColor}");
        }
    }

    private void PlayBasiliskDeathSound()
    {
        if (audioSource != null && basiliskDeathSound != null)
        {
            audioSource.PlayOneShot(basiliskDeathSound);
            Debug.Log("[Basilisk] Playing basilisk death sound effect");
        }
        else
        {
            Debug.LogWarning("[Basilisk] Cannot play basilisk death sound - missing AudioSource or AudioClip");
        }
    }

    public override void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"Basilisk {gameObject.name} died with dissolving effect!");

        // Award score for kill (same as base Enemy class)
        ScoreManager.NotifyKill();

        // Play basilisk death sound
        PlayBasiliskDeathSound();

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

        // Use basilisk-specific shader dissolving effect if available
        if (useBasiliskShaderDissolve && basiliskShaderDissolveEffect != null)
        {
            Debug.Log($"[Basilisk] Starting shader dissolving effect for {gameObject.name}");
            
            // Force refresh the material setup to ensure it's properly configured
            basiliskShaderDissolveEffect.RefreshMaterialSetup();
            
            // Start the shader dissolving effect (this will handle NavMeshAgent and collider disabling)
            basiliskShaderDissolveEffect.StartDissolving();
        }
        else
        {
            Debug.LogWarning($"[Basilisk] Shader dissolve effect not available - useBasiliskShaderDissolve: {useBasiliskShaderDissolve}, component: {basiliskShaderDissolveEffect != null}");
            // Fallback to base class behavior
            base.Die();
        }
    }
} 
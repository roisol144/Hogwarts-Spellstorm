using UnityEngine;

/// <summary>
/// Creates an immersive water splash effect with multiple particle systems and realistic behavior
/// </summary>
public class WaterSplashEffect : MonoBehaviour
{
    [Header("Main Splash Settings")]
    [SerializeField] private float splashDuration = 4f;
    [SerializeField] private int mainParticleCount = 150;
    [SerializeField] private float splashRadius = 3f;
    [SerializeField] private float splashHeight = 5f;
    [SerializeField] private Color waterColor = new Color(0.1f, 0.6f, 1f, 0.9f);
    
    [Header("Secondary Effects")]
    [SerializeField] private int mistParticleCount = 80;
    [SerializeField] private int dropletCount = 60;
    [SerializeField] private Color mistColor = new Color(0.8f, 0.9f, 1f, 0.3f);
    
    [Header("Audio")]
    [SerializeField] private AudioClip[] splashSounds; // Multiple sounds for variety
    [SerializeField] private float volume = 0.7f;
    
    private ParticleSystem mainSplash;
    private ParticleSystem mistEffect;
    private ParticleSystem dropletEffect;
    private AudioSource audioSource;

    private void Awake()
    {
        // Create multiple particle systems for layered effect
        CreateParticleSystems();
        
        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        SetupAllParticleSystems();
    }

    /// <summary>
    /// Creates multiple particle systems for different splash effects
    /// </summary>
    private void CreateParticleSystems()
    {
        // Main splash system
        mainSplash = GetComponent<ParticleSystem>();
        if (mainSplash == null)
        {
            mainSplash = gameObject.AddComponent<ParticleSystem>();
        }
        
        // Mist effect system
        GameObject mistObject = new GameObject("WaterMist");
        mistObject.transform.SetParent(transform);
        mistObject.transform.localPosition = Vector3.zero;
        mistEffect = mistObject.AddComponent<ParticleSystem>();
        
        // Droplet effect system
        GameObject dropletObject = new GameObject("WaterDroplets");
        dropletObject.transform.SetParent(transform);
        dropletObject.transform.localPosition = Vector3.zero;
        dropletEffect = dropletObject.AddComponent<ParticleSystem>();
    }

    /// <summary>
    /// Sets up all particle systems for immersive splash effect
    /// </summary>
    private void SetupAllParticleSystems()
    {
        SetupMainSplash();
        SetupMistEffect();
        SetupDropletEffect();
    }

    /// <summary>
    /// Sets up the main dramatic splash effect
    /// </summary>
    private void SetupMainSplash()
    {
        var main = mainSplash.main;
        main.startLifetime = splashDuration;
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 15f); // More dramatic initial speed
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.25f); // Bigger droplets for main splash
        main.startColor = waterColor;
        main.maxParticles = mainParticleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 2f; // Stronger gravity for dramatic fall
        main.loop = false; // CRITICAL: Don't loop the splash!
        main.playOnAwake = false; // Don't auto-play

        // Enhanced shape for directional splash
        var shape = mainSplash.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Hemisphere; // More realistic water splash shape
        shape.radius = splashRadius;
        shape.arc = 180f; // Forward-facing splash

        // Dramatic burst emissions
        var emission = mainSplash.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, mainParticleCount), // Instant dramatic splash
        });

        // Size animation for impact
        var size = mainSplash.sizeOverLifetime;
        size.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.8f);   // Start smaller
        sizeCurve.AddKey(0.1f, 1.2f); // Expand on impact
        sizeCurve.AddKey(0.7f, 0.8f); // Shrink as they fall
        sizeCurve.AddKey(1f, 0.2f);   // Almost disappear
        size.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        SetupWaterMaterial(mainSplash, waterColor);
    }

    /// <summary>
    /// Sets up the mist/spray effect for atmosphere
    /// </summary>
    private void SetupMistEffect()
    {
        var main = mistEffect.main;
        main.startLifetime = splashDuration * 1.5f; // Mist lasts longer
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 3f); // Slow floating mist
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.4f); // Larger, softer particles
        main.startColor = mistColor;
        main.maxParticles = mistParticleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.1f; // Slight upward float
        main.loop = false; // No looping for mist
        main.playOnAwake = false; // Manual control

        // Wider, softer shape for mist
        var shape = mistEffect.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = splashRadius * 1.2f; // Wider spread
        shape.arc = 120f; // Softer, more spread out mist

        // Delayed mist emission
        var emission = mistEffect.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.2f, mistParticleCount), // Delayed mist appearance
        });

        SetupWaterMaterial(mistEffect, mistColor);
    }

    /// <summary>
    /// Sets up secondary droplet effects
    /// </summary>
    private void SetupDropletEffect()
    {
        var main = dropletEffect.main;
        main.startLifetime = splashDuration * 0.8f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 20f); // High-speed droplets
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.08f); // Small, fast droplets
        main.startColor = waterColor;
        main.maxParticles = dropletCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 2.5f; // Heavy gravity for realistic droplets
        main.loop = false; // No looping for droplets
        main.playOnAwake = false; // Manual trigger only

        // Cone shape for scattered droplets
        var shape = dropletEffect.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 45f; // Wide cone spread
        shape.radius = splashRadius * 0.3f;

        // Secondary droplet bursts
        var emission = dropletEffect.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.05f, dropletCount / 2), // Early droplets
            new ParticleSystem.Burst(0.15f, dropletCount / 2), // Later droplets
        });

        SetupWaterMaterial(dropletEffect, waterColor);
    }

    /// <summary>
    /// Sets up realistic water material for particle systems
    /// </summary>
    private void SetupWaterMaterial(ParticleSystem particles, Color color)
    {
        // Realistic water color fade
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(color, 0.0f),
                new GradientColorKey(new Color(color.r * 0.7f, color.g * 0.8f, color.b * 1.1f, color.a), 1.0f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(color.a, 0.0f),
                new GradientAlphaKey(0.4f, 0.6f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = gradient;

        // Enhanced material
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            Material waterMaterial = new Material(Shader.Find("Sprites/Default"));
            waterMaterial.color = color;
            renderer.material = waterMaterial;
            renderer.alignment = ParticleSystemRenderSpace.World;
            renderer.sortMode = ParticleSystemSortMode.Distance;
        }
    }

    /// <summary>
    /// Triggers the immersive splash effect at the specified position with directional impact
    /// </summary>
    /// <param name="position">World position where the splash should occur</param>
    /// <param name="impactDirection">Direction of the impact (spell's forward direction)</param>
    /// <param name="surfaceNormal">Normal of the surface being hit</param>
    public void TriggerSplash(Vector3 position, Vector3 impactDirection = default, Vector3 surfaceNormal = default)
    {
        transform.position = position;
        
        // Orient splash based on impact direction
        if (impactDirection != Vector3.zero)
        {
            // Point the splash in the direction of impact
            transform.rotation = Quaternion.LookRotation(impactDirection);
        }
        else if (surfaceNormal != Vector3.zero)
        {
            // If no impact direction, use surface normal
            transform.rotation = Quaternion.LookRotation(-surfaceNormal);
        }
        
        // Play random splash sound for variety
        if (splashSounds != null && splashSounds.Length > 0 && audioSource != null)
        {
            AudioClip randomSplash = splashSounds[Random.Range(0, splashSounds.Length)];
            if (randomSplash != null)
            {
                audioSource.clip = randomSplash;
                audioSource.volume = volume;
                audioSource.pitch = Random.Range(0.9f, 1.1f); // Slight pitch variation
                audioSource.Play();
            }
        }
        
        // Trigger all particle effects with slight delays for realism
        mainSplash.Play();
        
        // Delayed secondary effects
        StartCoroutine(DelayedEffects());
        
        // Auto-destroy after all effects are done
        Destroy(gameObject, splashDuration * 1.5f + 2f);
    }

    /// <summary>
    /// Triggers secondary effects with realistic timing
    /// </summary>
    private System.Collections.IEnumerator DelayedEffects()
    {
        // Mist appears slightly after main splash
        yield return new WaitForSeconds(0.1f);
        mistEffect.Play();
        
        // Droplets appear as the splash peaks
        yield return new WaitForSeconds(0.05f);
        dropletEffect.Play();
    }

    /// <summary>
    /// Creates an immersive splash effect at the specified position with direction
    /// </summary>
    /// <param name="position">Position to create splash</param>
    /// <param name="impactDirection">Direction of the impact</param>
    /// <param name="surfaceNormal">Normal of the surface</param>
    /// <param name="splashSounds">Optional array of splash sounds</param>
    /// <returns>The created splash effect GameObject</returns>
    public static GameObject CreateSplash(Vector3 position, Vector3 impactDirection = default, Vector3 surfaceNormal = default, AudioClip[] splashSounds = null)
    {
        GameObject splashObject = new GameObject("ImmersiveWaterSplash");
        WaterSplashEffect splash = splashObject.AddComponent<WaterSplashEffect>();
        
        if (splashSounds != null && splashSounds.Length > 0)
        {
            splash.splashSounds = splashSounds;
        }
        
        splash.TriggerSplash(position, impactDirection, surfaceNormal);
        return splashObject;
    }
} 
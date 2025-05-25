using UnityEngine;
using System.Collections;

public class DementorDissolvingEffect : MonoBehaviour
{
    [Header("Dissolving Settings")]
    [SerializeField] private float dissolveDuration = 3f;
    [SerializeField] private AnimationCurve dissolveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool includeChildRenderers = true;

    [Header("Particle Effects")]
    [SerializeField] private GameObject dissolveParticlesPrefab; // Optional particle effect (e.g., from Hovl Studio)
    [SerializeField] private bool createSmokeEffect = true;
    [SerializeField] private int smokeParticleCount = 50;
    [SerializeField] private float smokeLifetime = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip dissolveSound;
    [SerializeField] private float audioVolume = 0.5f;

    [Header("Advanced Settings")]
    [SerializeField] private bool useShrinkEffect = true; // Also shrink the object while dissolving
    [SerializeField] private bool fadeToBlack = true; // Fade to darker colors instead of just transparent

    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Material[] dissolveMaterials;
    private bool isDissolving = false;
    private AudioSource audioSource;
    private ParticleSystem smokeParticles;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        SetupComponents();
        SetupMaterials();
    }

    private void SetupComponents()
    {
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && dissolveSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.volume = audioVolume;
        }

        // Create smoke particle system if needed and no external prefab is provided
        if (createSmokeEffect && dissolveParticlesPrefab == null)
        {
            CreateSmokeParticleSystem();
        }
    }

    private void CreateSmokeParticleSystem()
    {
        GameObject smokeObject = new GameObject("DissolveSmoke");
        smokeObject.transform.SetParent(transform);
        smokeObject.transform.localPosition = Vector3.zero;

        smokeParticles = smokeObject.AddComponent<ParticleSystem>();
        
        var main = smokeParticles.main;
        main.startLifetime = smokeLifetime;
        main.startSpeed = 2f;
        main.startSize = 1f;
        main.startColor = new Color(0.2f, 0.2f, 0.3f, 0.5f); // Dark greyish color for dementor
        main.maxParticles = smokeParticleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = smokeParticles.emission;
        emission.enabled = false; // We'll control emission manually

        var shape = smokeParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(4f, 8f, 4f); // Adjust based on dementor size

        var velocityOverLifetime = smokeParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.y = 3f; // Upward movement

        var sizeOverLifetime = smokeParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.5f);
        sizeCurve.AddKey(0.5f, 1f);
        sizeCurve.AddKey(1f, 1.5f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var colorOverLifetime = smokeParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient colorGradient = new Gradient();
        colorGradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.2f, 0.2f, 0.3f), 0f),
                new GradientColorKey(new Color(0.1f, 0.1f, 0.2f), 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.5f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = colorGradient;
    }

    private void SetupMaterials()
    {
        // Get all renderers
        if (includeChildRenderers)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }
        else
        {
            renderers = GetComponents<Renderer>();
        }

        // Store original materials
        int totalMaterials = 0;
        foreach (Renderer renderer in renderers)
        {
            totalMaterials += renderer.materials.Length;
        }

        originalMaterials = new Material[totalMaterials];
        dissolveMaterials = new Material[totalMaterials];

        int materialIndex = 0;
        foreach (Renderer renderer in renderers)
        {
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                originalMaterials[materialIndex] = renderer.materials[i];
                
                // Create a copy of the material for dissolving
                dissolveMaterials[materialIndex] = new Material(renderer.materials[i]);
                
                materialIndex++;
            }
        }
    }

    public void StartDissolving()
    {
        if (isDissolving) return;
        
        isDissolving = true;
        StartCoroutine(DissolveCoroutine());
    }

    private IEnumerator DissolveCoroutine()
    {
        Debug.Log($"[DementorDissolvingEffect] Starting dissolving effect for {gameObject.name}");

        // Play sound effect
        if (audioSource != null && dissolveSound != null)
        {
            audioSource.PlayOneShot(dissolveSound);
        }

        // Start particle effects
        if (dissolveParticlesPrefab != null)
        {
            GameObject particleEffect = Instantiate(dissolveParticlesPrefab, transform.position, Quaternion.identity);
            // Parent it to this object so it follows during dissolving
            particleEffect.transform.SetParent(transform);
            Debug.Log($"[DementorDissolvingEffect] Spawned particle effect: {dissolveParticlesPrefab.name}");
        }

        if (smokeParticles != null)
        {
            var emission = smokeParticles.emission;
            emission.enabled = true;
            emission.rateOverTime = smokeParticleCount / dissolveDuration;
        }

        // Replace materials with dissolve materials
        int materialIndex = 0;
        foreach (Renderer renderer in renderers)
        {
            Material[] newMaterials = new Material[renderer.materials.Length];
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                newMaterials[i] = dissolveMaterials[materialIndex];
                materialIndex++;
            }
            renderer.materials = newMaterials;
        }

        float elapsedTime = 0f;
        
        while (elapsedTime < dissolveDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / dissolveDuration;
            float curveValue = dissolveCurve.Evaluate(progress);
            
            // Apply dissolving effect by reducing alpha
            foreach (Material material in dissolveMaterials)
            {
                if (material != null)
                {
                    // Change rendering mode to transparent if it's not already
                    if (material.HasProperty("_Mode"))
                    {
                        material.SetFloat("_Mode", 3); // Transparent mode
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.EnableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = 3000;
                    }

                    // Apply alpha fade
                    Color color = material.color;
                    
                    if (fadeToBlack)
                    {
                        // Fade to darker color and transparent
                        float darkening = 1f - (curveValue * 0.8f); // Don't go completely black
                        color.r *= darkening;
                        color.g *= darkening;
                        color.b *= darkening;
                    }
                    
                    color.a = 1f - curveValue;
                    material.color = color;

                    // Also fade emission if it has one
                    if (material.HasProperty("_EmissionColor"))
                    {
                        Color emissionColor = material.GetColor("_EmissionColor");
                        emissionColor.a = 1f - curveValue;
                        material.SetColor("_EmissionColor", emissionColor);
                    }
                }
            }

            // Apply shrinking effect if enabled
            if (useShrinkEffect)
            {
                float scale = 1f - (curveValue * 0.3f); // Shrink by up to 30%
                transform.localScale = originalScale * scale;
            }

            yield return null;
        }

        Debug.Log($"[DementorDissolvingEffect] Dissolving effect completed for {gameObject.name}");
        
        // Stop particle emission
        if (smokeParticles != null)
        {
            var emission = smokeParticles.emission;
            emission.enabled = false;
        }
        
        // Completely hide the object
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        // Wait a bit for particles to fade out, then destroy
        yield return new WaitForSeconds(smokeLifetime);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Clean up created materials
        if (dissolveMaterials != null)
        {
            foreach (Material material in dissolveMaterials)
            {
                if (material != null)
                {
                    DestroyImmediate(material);
                }
            }
        }
    }
} 
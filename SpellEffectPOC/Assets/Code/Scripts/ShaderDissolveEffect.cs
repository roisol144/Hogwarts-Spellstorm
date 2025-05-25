using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShaderDissolveEffect : MonoBehaviour
{
    [Header("Dissolve Settings")]
    [SerializeField] private float dissolveDuration = 2.5f;
    [SerializeField] private AnimationCurve dissolveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool includeChildRenderers = true;

    [Header("Shader Dissolve Materials")]
    [SerializeField] private Material dissolveMaterial; // Assign a dissolve material from ShaderGraph_Dissolve
    [SerializeField] private bool useDirectionalDissolve = true;
    [SerializeField] private Vector3 dissolveDirection = Vector3.up; // Direction for directional dissolve

    [Header("Dissolve Visual Settings")]
    [SerializeField] private Color edgeColor = new Color(1f, 0.5f, 0f, 1f); // Orange edge color
    [SerializeField] private float edgeColorIntensity = 2f;
    [SerializeField] private float edgeWidth = 0.1f;
    [SerializeField] private float noiseScale = 50f;

    // Public properties for external configuration
    public Material DissolveMaterial 
    { 
        get => dissolveMaterial; 
        set => dissolveMaterial = value; 
    }
    
    public Vector3 DissolveDirection 
    { 
        get => dissolveDirection; 
        set => dissolveDirection = value; 
    }
    
    public Color EdgeColor 
    { 
        get => edgeColor; 
        set => edgeColor = value; 
    }
    
    public float EdgeColorIntensity 
    { 
        get => edgeColorIntensity; 
        set => edgeColorIntensity = value; 
    }

    [Header("Particle Effects")]
    [SerializeField] private GameObject dissolveParticlesPrefab; // Optional particle effect
    [SerializeField] private bool spawnParticlesAtEdge = true;

    [Header("Audio")]
    [SerializeField] private AudioClip dissolveSound;
    [SerializeField] private float audioVolume = 0.5f;

    [Header("Advanced Settings")]
    [SerializeField] private bool animateNoiseUV = true;
    [SerializeField] private Vector2 noiseUVSpeed = new Vector2(0.5f, 0.5f);

    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Material[] dissolveMaterials;
    private bool isDissolving = false;
    private AudioSource audioSource;

    // Shader property IDs for performance
    private static readonly int DissolveProperty = Shader.PropertyToID("_Dissolve");
    private static readonly int EdgeColorProperty = Shader.PropertyToID("_EdgeColor");
    private static readonly int EdgeColorIntensityProperty = Shader.PropertyToID("_EdgeColorIntensity");
    private static readonly int EdgeWidthProperty = Shader.PropertyToID("_EdgeWidth");
    private static readonly int NoiseScaleProperty = Shader.PropertyToID("_NoiseScale");
    private static readonly int NoiseUVSpeedProperty = Shader.PropertyToID("_NoiseUVSpeed");
    private static readonly int DissolveDirectionProperty = Shader.PropertyToID("_DissolveDirection");

    private void Awake()
    {
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

        // Validate dissolve material
        if (dissolveMaterial == null)
        {
            Debug.LogError($"[ShaderDissolveEffect] No dissolve material assigned to {gameObject.name}! Please assign a material from ShaderGraph_Dissolve/URP/Materials/");
            return;
        }

        // Validate renderers
        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogError($"[ShaderDissolveEffect] No renderers found on {gameObject.name}!");
            return;
        }

        // Store original materials and create dissolve instances
        List<Material> originalList = new List<Material>();
        List<Material> dissolveList = new List<Material>();

        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.materials)
            {
                originalList.Add(material);
                
                // Create an instance of the dissolve material
                Material dissolveInstance = new Material(dissolveMaterial);
                
                // Try to copy base texture from original material if it has one
                if (material.mainTexture != null && dissolveInstance.HasProperty("_BaseMap"))
                {
                    dissolveInstance.SetTexture("_BaseMap", material.mainTexture);
                }
                
                // Copy color if available
                if (material.HasProperty("_Color") && dissolveInstance.HasProperty("_BaseColor"))
                {
                    dissolveInstance.SetColor("_BaseColor", material.color);
                }
                else if (material.HasProperty("_BaseColor") && dissolveInstance.HasProperty("_BaseColor"))
                {
                    dissolveInstance.SetColor("_BaseColor", material.GetColor("_BaseColor"));
                }

                dissolveList.Add(dissolveInstance);
            }
        }

        originalMaterials = originalList.ToArray();
        dissolveMaterials = dissolveList.ToArray();

        Debug.Log($"[ShaderDissolveEffect] Setup {dissolveMaterials.Length} dissolve materials for {gameObject.name}");
    }

    public void StartDissolving()
    {
        if (isDissolving) return;
        
        if (dissolveMaterials == null || dissolveMaterials.Length == 0)
        {
            Debug.LogError($"[ShaderDissolveEffect] No dissolve materials available for {gameObject.name}!");
            return;
        }

        isDissolving = true;
        StartCoroutine(DissolveCoroutine());
    }

    private IEnumerator DissolveCoroutine()
    {
        Debug.Log($"[ShaderDissolveEffect] Starting shader-based dissolving effect for {gameObject.name}");

        // Play sound effect
        if (audioSource != null && dissolveSound != null)
        {
            audioSource.PlayOneShot(dissolveSound);
        }

        // Spawn particle effects
        if (dissolveParticlesPrefab != null)
        {
            GameObject particleEffect = Instantiate(dissolveParticlesPrefab, transform.position, Quaternion.identity);
            particleEffect.transform.SetParent(transform);
            Debug.Log($"[ShaderDissolveEffect] Spawned particle effect: {dissolveParticlesPrefab.name}");
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

        // Configure initial dissolve material properties
        foreach (Material dissolveMat in dissolveMaterials)
        {
            if (dissolveMat != null)
            {
                // Set initial dissolve value (0 = not dissolved)
                dissolveMat.SetFloat(DissolveProperty, 0f);
                
                // Set edge appearance
                dissolveMat.SetColor(EdgeColorProperty, edgeColor);
                dissolveMat.SetFloat(EdgeColorIntensityProperty, edgeColorIntensity);
                dissolveMat.SetFloat(EdgeWidthProperty, edgeWidth);
                dissolveMat.SetFloat(NoiseScaleProperty, noiseScale);

                // Set directional dissolve if using directional shader
                if (useDirectionalDissolve && dissolveMat.HasProperty(DissolveDirectionProperty))
                {
                    dissolveMat.SetVector(DissolveDirectionProperty, dissolveDirection);
                }

                // Set noise UV animation
                if (animateNoiseUV && dissolveMat.HasProperty(NoiseUVSpeedProperty))
                {
                    dissolveMat.SetVector(NoiseUVSpeedProperty, new Vector4(noiseUVSpeed.x, noiseUVSpeed.y, 0, 0));
                }
            }
        }

        float elapsedTime = 0f;
        
        while (elapsedTime < dissolveDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / dissolveDuration;
            float curveValue = dissolveCurve.Evaluate(progress);
            
            // Update dissolve value on all materials
            foreach (Material dissolveMat in dissolveMaterials)
            {
                if (dissolveMat != null)
                {
                    dissolveMat.SetFloat(DissolveProperty, curveValue);
                }
            }

            yield return null;
        }

        Debug.Log($"[ShaderDissolveEffect] Shader dissolving effect completed for {gameObject.name}");
        
        // Completely hide the object
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        // Wait a bit before destroying
        yield return new WaitForSeconds(0.5f);
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

    // Public method to trigger dissolve with custom parameters
    public void StartDissolving(float duration, Color customEdgeColor, float customEdgeIntensity)
    {
        dissolveDuration = duration;
        edgeColor = customEdgeColor;
        edgeColorIntensity = customEdgeIntensity;
        StartDissolving();
    }
} 
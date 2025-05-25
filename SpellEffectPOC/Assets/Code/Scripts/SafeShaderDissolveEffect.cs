using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SafeShaderDissolveEffect : MonoBehaviour
{
    [Header("Dissolve Settings")]
    [SerializeField] private float dissolveDuration = 2.5f;
    [SerializeField] private AnimationCurve dissolveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Required Setup")]
    [SerializeField] private Material dissolveMaterial; // Assign a dissolve material from ShaderGraph_Dissolve
    [SerializeField] private Color edgeColor = new Color(0.8f, 0.2f, 1f, 1f); // Purple for dementor
    
    // Public properties for external configuration
    public Material DissolveMaterial 
    { 
        get => dissolveMaterial; 
        set => dissolveMaterial = value; 
    }
    
    public Color EdgeColor 
    { 
        get => edgeColor; 
        set => edgeColor = value; 
    }
    
    /// <summary>
    /// Force refresh of material setup - useful when material is assigned after Awake()
    /// </summary>
    public void RefreshMaterialSetup()
    {
        dissolveMaterials = null; // Force re-setup
        SetupMaterials();
    }

    /// <summary>
    /// Copy visual properties from original material to dissolve material to preserve appearance
    /// </summary>
    private void CopyMaterialProperties(Material originalMaterial, Material dissolveTarget)
    {
        try
        {
            // Common material properties to copy
            string[] propertiesToCopy = {
                "_BaseColor", "_Color", "_MainTex", "_BaseMap", 
                "_Metallic", "_Smoothness", "_Glossiness",
                "_BumpMap", "_NormalMap", "_NormalScale",
                "_OcclusionMap", "_OcclusionStrength",
                "_EmissionColor", "_EmissionMap"
            };

            foreach (string property in propertiesToCopy)
            {
                // Copy color properties
                if (originalMaterial.HasProperty(property) && dissolveTarget.HasProperty(property))
                {
                    if (property.Contains("Color"))
                    {
                        dissolveTarget.SetColor(property, originalMaterial.GetColor(property));
                    }
                    else if (property.Contains("Tex") || property.Contains("Map"))
                    {
                        // Copy texture properties
                        dissolveTarget.SetTexture(property, originalMaterial.GetTexture(property));
                        if (originalMaterial.HasProperty(property + "_ST"))
                        {
                            dissolveTarget.SetVector(property + "_ST", originalMaterial.GetVector(property + "_ST"));
                        }
                    }
                    else
                    {
                        // Copy float properties
                        dissolveTarget.SetFloat(property, originalMaterial.GetFloat(property));
                    }
                }
            }

            Debug.Log($"[SafeShaderDissolveEffect] Copied properties from '{originalMaterial.name}' to dissolve material");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SafeShaderDissolveEffect] Error copying material properties: {e.Message}");
        }
    }
    
    private Renderer[] renderers;
    private Material[] dissolveMaterials;
    private bool isDissolving = false;
    
    // Shader property IDs
    private static readonly int DissolveProperty = Shader.PropertyToID("_Dissolve");
    private static readonly int EdgeColorProperty = Shader.PropertyToID("_EdgeColor");
    private static readonly int EdgeColorIntensityProperty = Shader.PropertyToID("_EdgeColorIntensity");

    private void Awake()
    {
        SetupMaterials();
    }

    private void SetupMaterials()
    {
        // Skip if already set up and material hasn't changed
        if (dissolveMaterials != null && dissolveMaterials.Length > 0)
        {
            return;
        }

        try
        {
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>();
            }
            
            if (renderers == null || renderers.Length == 0)
            {
                Debug.LogWarning($"[SafeShaderDissolveEffect] No renderers found on {gameObject.name}");
                return;
            }

            if (dissolveMaterial == null)
            {
                Debug.LogError($"[SafeShaderDissolveEffect] No dissolve material assigned to {gameObject.name}!");
                return;
            }

            List<Material> dissolveList = new List<Material>();

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;
                
                foreach (Material originalMaterial in renderer.materials)
                {
                    if (originalMaterial == null) continue;
                    
                    Material dissolveInstance = new Material(dissolveMaterial);
                    dissolveInstance.name = $"{originalMaterial.name}_Dissolve";
                    
                    // Copy properties from original material to preserve appearance
                    CopyMaterialProperties(originalMaterial, dissolveInstance);
                    
                    dissolveList.Add(dissolveInstance);
                }
            }

            dissolveMaterials = dissolveList.ToArray();
            Debug.Log($"[SafeShaderDissolveEffect] Setup {dissolveMaterials.Length} dissolve materials for {gameObject.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SafeShaderDissolveEffect] Error setting up materials: {e.Message}");
        }
    }

    public void StartDissolving()
    {
        if (isDissolving) return;
        
        // Setup materials again in case they were set after Awake()
        SetupMaterials();
        
        if (dissolveMaterials == null || dissolveMaterials.Length == 0)
        {
            Debug.LogError($"[SafeShaderDissolveEffect] No dissolve materials available for {gameObject.name}!");
            Destroy(gameObject); // Fallback
            return;
        }

        // Check if any materials have the dissolve property
        bool hasAnyDissolveProperty = false;
        foreach (Material mat in dissolveMaterials)
        {
            if (mat != null && mat.HasProperty(DissolveProperty))
            {
                hasAnyDissolveProperty = true;
                break;
            }
        }

        if (!hasAnyDissolveProperty)
        {
            Debug.LogError($"[SafeShaderDissolveEffect] The assigned dissolve material doesn't have '_Dissolve' property! Please use 'Shader Graphs_Dissolve_Dissolve_Metallic' instead of directional dissolve materials.");
            Destroy(gameObject); // Fallback
            return;
        }

        isDissolving = true;
        
        // Safely disable NavMeshAgent and related components
        SafeDisableNavMeshAgent();
        
        StartCoroutine(DissolveCoroutine());
    }

    private void SafeDisableNavMeshAgent()
    {
        try
        {
            // Disable NavMeshAgent
            var navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (navAgent != null && navAgent.enabled)
            {
                if (navAgent.isOnNavMesh)
                {
                    navAgent.isStopped = true;
                    navAgent.ResetPath();
                }
                navAgent.enabled = false;
            }
            
            // Disable movement scripts that use NavMeshAgent
            var enemyMovement = GetComponent<EnemyMovement>();
            if (enemyMovement != null)
            {
                enemyMovement.enabled = false;
            }
            
            // Also disable collider
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[SafeShaderDissolveEffect] Error disabling components: {e.Message}");
        }
    }

    private IEnumerator DissolveCoroutine()
    {
        Debug.Log($"[SafeShaderDissolveEffect] Starting dissolve for {gameObject.name}");

        bool success = false;
        
        // Setup materials (no yield returns here)
        try
        {
            // Replace materials with dissolve materials
            int materialIndex = 0;
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;
                
                Material[] newMaterials = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    if (materialIndex < dissolveMaterials.Length)
                    {
                        newMaterials[i] = dissolveMaterials[materialIndex];
                        materialIndex++;
                    }
                }
                renderer.materials = newMaterials;
            }

            // Configure dissolve materials
            foreach (Material dissolveMat in dissolveMaterials)
            {
                if (dissolveMat != null)
                {
                    // Debug: Check which properties are available
                    bool hasDissolve = dissolveMat.HasProperty(DissolveProperty);
                    bool hasEdgeColor = dissolveMat.HasProperty(EdgeColorProperty);
                    bool hasEdgeIntensity = dissolveMat.HasProperty(EdgeColorIntensityProperty);
                    
                    Debug.Log($"[SafeShaderDissolveEffect] Material '{dissolveMat.name}' properties: _Dissolve={hasDissolve}, _EdgeColor={hasEdgeColor}, _EdgeColorIntensity={hasEdgeIntensity}");
                    
                    if (hasDissolve)
                    {
                        dissolveMat.SetFloat(DissolveProperty, 0f);
                    }
                    else
                    {
                        Debug.LogWarning($"[SafeShaderDissolveEffect] Material '{dissolveMat.name}' doesn't have '_Dissolve' property! This may be a directional dissolve shader.");
                    }
                    
                    if (hasEdgeColor)
                        dissolveMat.SetColor(EdgeColorProperty, edgeColor);
                    
                    if (hasEdgeIntensity)
                        dissolveMat.SetFloat(EdgeColorIntensityProperty, 3f);
                }
            }
            
            success = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SafeShaderDissolveEffect] Error during setup: {e.Message}");
        }

        if (!success)
        {
            Destroy(gameObject);
            yield break;
        }

        // Animate dissolve (yield returns allowed here)
        float elapsedTime = 0f;
        
        while (elapsedTime < dissolveDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / dissolveDuration;
            float curveValue = dissolveCurve.Evaluate(progress);
            
            // Update dissolve value
            try
            {
                foreach (Material dissolveMat in dissolveMaterials)
                {
                    if (dissolveMat != null && dissolveMat.HasProperty(DissolveProperty))
                    {
                        dissolveMat.SetFloat(DissolveProperty, curveValue);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SafeShaderDissolveEffect] Error updating dissolve: {e.Message}");
                break;
            }

            yield return null;
        }

        // Final cleanup (no yield returns here)
        try
        {
            // Hide renderers
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                    renderer.enabled = false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SafeShaderDissolveEffect] Error hiding renderers: {e.Message}");
        }

        yield return new WaitForSeconds(0.1f);
        
        Debug.Log($"[SafeShaderDissolveEffect] Dissolve completed for {gameObject.name}");
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Clean up materials
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
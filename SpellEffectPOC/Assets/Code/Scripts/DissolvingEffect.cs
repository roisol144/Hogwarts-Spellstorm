using UnityEngine;
using System.Collections;

public class DissolvingEffect : MonoBehaviour
{
    [Header("Dissolving Settings")]
    [SerializeField] private float dissolveDuration = 2f;
    [SerializeField] private AnimationCurve dissolveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool includeChildRenderers = true;

    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Material[] dissolveMaterials;
    private bool isDissolving = false;

    private void Awake()
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
        Debug.Log($"[DissolvingEffect] Starting dissolving effect for {gameObject.name}");

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

            yield return null;
        }

        Debug.Log($"[DissolvingEffect] Dissolving effect completed for {gameObject.name}");
        
        // Completely hide the object
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        // Optionally destroy the object after dissolving
        Destroy(gameObject, 0.5f);
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
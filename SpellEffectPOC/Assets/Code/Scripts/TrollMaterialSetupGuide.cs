using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrollMaterialSetupGuide : MonoBehaviour
{
    [Header("Material Validation")]
    [SerializeField] private bool runValidationOnStart = true;
    [SerializeField] private bool showDetailedInfo = true;
    
    void Start()
    {
        if (runValidationOnStart)
        {
            ValidateTrollMaterials();
        }
    }
    
    [ContextMenu("Validate Troll Materials")]
    public void ValidateTrollMaterials()
    {
        Debug.Log("=== TROLL MATERIAL VALIDATION ===");
        
        // Get all renderers on this object and children
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Debug.Log($"Found {renderers.Length} renderers on {gameObject.name}");
        
        foreach (Renderer renderer in renderers)
        {
            Debug.Log($"\n--- Renderer: {renderer.name} ---");
            
            Material[] materials = renderer.materials;
            Debug.Log($"Materials count: {materials.Length}");
            
            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];
                if (mat == null)
                {
                    Debug.LogError($"❌ Material {i} is NULL!");
                    continue;
                }
                
                Debug.Log($"\n📦 Material {i}: {mat.name}");
                ValidateMaterial(mat);
            }
        }
        
        Debug.Log("\n=== MATERIAL VALIDATION COMPLETE ===");
    }
    
    void ValidateMaterial(Material mat)
    {
        // Check common texture slots
        CheckTextureSlot(mat, "_BaseMap", "Base Color/Albedo");
        CheckTextureSlot(mat, "_BumpMap", "Normal Map");
        CheckTextureSlot(mat, "_MetallicGlossMap", "Metallic/Smoothness");
        CheckTextureSlot(mat, "_EmissionMap", "Emission");
        CheckTextureSlot(mat, "_OcclusionMap", "Occlusion");
        
        if (showDetailedInfo)
        {
            // Log material properties
            Debug.Log($"   Shader: {mat.shader.name}");
            Debug.Log($"   Render Queue: {mat.renderQueue}");
            
            if (mat.HasProperty("_Metallic"))
                Debug.Log($"   Metallic: {mat.GetFloat("_Metallic"):F2}");
            if (mat.HasProperty("_Smoothness"))
                Debug.Log($"   Smoothness: {mat.GetFloat("_Smoothness"):F2}");
            if (mat.HasProperty("_BumpScale"))
                Debug.Log($"   Normal Scale: {mat.GetFloat("_BumpScale"):F2}");
        }
    }
    
    void CheckTextureSlot(Material mat, string propertyName, string displayName)
    {
        if (mat.HasProperty(propertyName))
        {
            Texture texture = mat.GetTexture(propertyName);
            if (texture != null)
            {
                Debug.Log($"   ✅ {displayName}: {texture.name}");
            }
            else
            {
                Debug.Log($"   ⚠️ {displayName}: Not assigned");
            }
        }
    }
    
    [ContextMenu("List All Available Textures")]
    public void ListAvailableTrollTextures()
    {
        Debug.Log("=== AVAILABLE TROLL TEXTURES ===");
        
#if UNITY_EDITOR
        // Find all textures in the troll folder (Editor only)
        string[] textureGuids = UnityEditor.AssetDatabase.FindAssets("t:Texture2D", new[] {"Assets/troll (1)/textures"});
        
        foreach (string guid in textureGuids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            Debug.Log($"📸 {fileName}");
        }
#else
        Debug.Log("Texture listing only available in Unity Editor");
#endif
    }
    
    [ContextMenu("Optimize Material Settings")]
    public void OptimizeMaterialSettings()
    {
        Debug.Log("=== OPTIMIZING MATERIAL SETTINGS ===");
        
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            
            for (int i = 0; i < materials.Length; i++)
            {
                Material mat = materials[i];
                if (mat == null) continue;
                
                Debug.Log($"Optimizing material: {mat.name}");
                
                // Set recommended values for better visual quality
                if (mat.HasProperty("_BumpScale"))
                {
                    mat.SetFloat("_BumpScale", 1.0f); // Normal map intensity
                }
                
                if (mat.HasProperty("_Smoothness"))
                {
                    if (mat.name.Contains("body"))
                        mat.SetFloat("_Smoothness", 0.3f); // Skin should be less smooth
                    else if (mat.name.Contains("weapon"))
                        mat.SetFloat("_Smoothness", 0.8f); // Metal should be smooth
                    else if (mat.name.Contains("hair"))
                        mat.SetFloat("_Smoothness", 0.4f); // Hair moderate smoothness
                    else
                        mat.SetFloat("_Smoothness", 0.5f); // Default
                }
                
                if (mat.HasProperty("_Metallic"))
                {
                    if (mat.name.Contains("weapon"))
                        mat.SetFloat("_Metallic", 0.8f); // Weapon should be metallic
                    else
                        mat.SetFloat("_Metallic", 0.0f); // Other parts non-metallic
                }
                
                Debug.Log($"   ✅ Optimized {mat.name}");
            }
        }
        
        Debug.Log("=== OPTIMIZATION COMPLETE ===");
    }
} 
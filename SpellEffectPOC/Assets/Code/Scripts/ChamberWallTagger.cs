using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script to automatically tag all mesh objects in the Chamber of Secrets model as "Wall"
/// This enables proper collision detection with spells.
/// </summary>
public class ChamberWallTagger : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("The root GameObject of the Chamber of Secrets model")]
    public GameObject chamberRootObject;
    
    [Header("Tagging Options")]
    [Tooltip("Tag to assign to all mesh objects")]
    public string wallTag = "Wall";
    
    [Tooltip("Also add colliders to objects that don't have them")]
    public bool addCollidersIfMissing = true;
    
    [Tooltip("Use mesh colliders (more accurate) or box colliders (better performance)")]
    public bool useMeshColliders = true;
    
    [Header("Pool/Water Detection")]
    [Tooltip("Keywords to identify pool/water objects (case insensitive)")]
    public string[] poolKeywords = { "pool", "water", "lake", "pond", "basin" };

    /// <summary>
    /// Tags all mesh objects in the chamber as walls
    /// </summary>
    [ContextMenu("Tag Chamber Walls")]
    public void TagChamberWalls()
    {
        if (chamberRootObject == null)
        {
            Debug.LogError("Chamber root object is not assigned!");
            return;
        }

        // Ensure the Wall tag exists
        if (!TagExists(wallTag))
        {
            Debug.LogError($"Tag '{wallTag}' does not exist! Please create it in the Tag Manager first.");
            return;
        }

        int taggedCount = 0;
        int collidersAdded = 0;
        int poolsFound = 0;

        // Get all MeshRenderer components in children
        MeshRenderer[] meshRenderers = chamberRootObject.GetComponentsInChildren<MeshRenderer>(true);
        
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            GameObject obj = meshRenderer.gameObject;
            
            // Check if this is a pool/water object
            bool isPoolObject = IsPoolObject(obj);
            
            // Tag the object
            if (obj.tag != wallTag)
            {
                obj.tag = wallTag;
                taggedCount++;
                
                if (isPoolObject)
                {
                    poolsFound++;
                    Debug.Log($"Tagged '{obj.name}' as '{wallTag}' (POOL/WATER OBJECT - will show splash effects)");
                }
                else
                {
                    Debug.Log($"Tagged '{obj.name}' as '{wallTag}'");
                }
            }
            
            // Add collider if missing and option is enabled
            if (addCollidersIfMissing && obj.GetComponent<Collider>() == null)
            {
                if (useMeshColliders)
                {
                    MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
                    meshCollider.convex = false; // For static geometry
                }
                else
                {
                    obj.AddComponent<BoxCollider>();
                }
                collidersAdded++;
                Debug.Log($"Added collider to '{obj.name}'");
            }
        }

        Debug.Log($"Tagging complete! Tagged {taggedCount} objects as '{wallTag}' (including {poolsFound} pool/water objects) and added {collidersAdded} colliders.");
    }

    /// <summary>
    /// Finds and automatically assigns the chamber root object
    /// </summary>
    [ContextMenu("Find Chamber Root")]
    public void FindChamberRoot()
    {
        // Look for objects with "chamber" in their name
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("chamber") && obj.transform.parent == null)
            {
                chamberRootObject = obj;
                Debug.Log($"Found chamber root object: {obj.name}");
                return;
            }
        }
        
        Debug.LogWarning("Could not automatically find chamber root object. Please assign it manually.");
    }

    /// <summary>
    /// Checks if a tag exists in the project
    /// </summary>
    private bool TagExists(string tag)
    {
#if UNITY_EDITOR
        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
        {
            if (UnityEditorInternal.InternalEditorUtility.tags[i].Equals(tag))
                return true;
        }
        return false;
#else
        // In build, assume tag exists (runtime cannot check tag existence)
        return true;
#endif
    }

    /// <summary>
    /// Checks if an object is a pool/water object based on its name
    /// </summary>
    /// <param name="obj">GameObject to check</param>
    /// <returns>True if it contains pool/water keywords</returns>
    private bool IsPoolObject(GameObject obj)
    {
        string objectName = obj.name.ToLower();
        
        foreach (string keyword in poolKeywords)
        {
            if (objectName.Contains(keyword.ToLower()))
            {
                return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// Removes wall tags from all objects (useful for testing)
    /// </summary>
    [ContextMenu("Remove Wall Tags")]
    public void RemoveWallTags()
    {
        if (chamberRootObject == null)
        {
            Debug.LogError("Chamber root object is not assigned!");
            return;
        }

        int untaggedCount = 0;
        MeshRenderer[] meshRenderers = chamberRootObject.GetComponentsInChildren<MeshRenderer>(true);
        
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            GameObject obj = meshRenderer.gameObject;
            
            if (obj.tag == wallTag)
            {
                obj.tag = "Untagged";
                untaggedCount++;
                Debug.Log($"Removed '{wallTag}' tag from '{obj.name}'");
            }
        }

        Debug.Log($"Removed '{wallTag}' tag from {untaggedCount} objects.");
    }
}

#if UNITY_EDITOR
/// <summary>
/// Custom editor for the ChamberWallTagger with improved UI
/// </summary>
[CustomEditor(typeof(ChamberWallTagger))]
public class ChamberWallTaggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ChamberWallTagger tagger = (ChamberWallTagger)target;

        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Find Chamber Root"))
        {
            tagger.FindChamberRoot();
        }

        if (GUILayout.Button("Tag Chamber Walls"))
        {
            tagger.TagChamberWalls();
        }

        if (GUILayout.Button("Remove Wall Tags"))
        {
            if (EditorUtility.DisplayDialog("Confirm", "Remove all wall tags from chamber objects?", "Yes", "No"))
            {
                tagger.RemoveWallTags();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("1. Assign the Chamber root object or click 'Find Chamber Root'\n2. Ensure 'Wall' tag exists in Tag Manager\n3. Click 'Tag Chamber Walls'", MessageType.Info);
    }
}
#endif 
using UnityEngine;

/// <summary>
/// Aggressive environmental collision setup for when automatic methods fail
/// </summary>
public class ForceEnvironmentalSetup : MonoBehaviour
{
    [Header("Setup Options")]
    [SerializeField] private bool setupAllMeshRenderers = false;
    [SerializeField] private bool useMeshColliders = true;
    [SerializeField] private bool tagAsWall = true;
    
    [Header("Filter Settings")]
    [SerializeField] private float minimumObjectSize = 0.5f;
    [SerializeField] private bool skipMovingObjects = true;
    
    [ContextMenu("Force Setup All Environmental Objects")]
    public void ForceSetupAllEnvironmentalObjects()
    {
        Debug.Log("[ForceEnvironmentalSetup] Starting aggressive environmental setup...");
        
        MeshRenderer[] allRenderers = FindObjectsOfType<MeshRenderer>();
        int objectsProcessed = 0;
        int collidersAdded = 0;
        
        Debug.Log($"[ForceEnvironmentalSetup] Found {allRenderers.Length} mesh renderers to check");
        
        foreach (MeshRenderer renderer in allRenderers)
        {
            GameObject obj = renderer.gameObject;
            
            // Skip if it's the player or player components
            if (IsPlayerObject(obj))
            {
                continue;
            }
            
            // Skip if it's too small (likely a small decoration)
            if (!setupAllMeshRenderers && IsObjectTooSmall(obj))
            {
                continue;
            }
            
            // Skip if it's moving (likely a projectile or dynamic object)
            if (skipMovingObjects && HasMovementComponent(obj))
            {
                continue;
            }
            
            // Skip if it already has a collider
            if (obj.GetComponent<Collider>() != null)
            {
                continue;
            }
            
            // Add collider
            if (useMeshColliders)
            {
                MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
                meshCollider.convex = false; // For static geometry
            }
            else
            {
                obj.AddComponent<BoxCollider>();
            }
            
            // Tag as wall if option enabled
            if (tagAsWall && obj.tag == "Untagged")
            {
                obj.tag = "Wall";
            }
            
            // Set to wall layer if it exists
            int wallLayer = LayerMask.NameToLayer("Wall");
            if (wallLayer != -1)
            {
                obj.layer = wallLayer;
            }
            
            collidersAdded++;
            objectsProcessed++;
            
            Debug.Log($"[ForceEnvironmentalSetup] Added collider to: {obj.name}");
        }
        
        Debug.Log($"[ForceEnvironmentalSetup] ✅ COMPLETED! Processed {objectsProcessed} objects, added {collidersAdded} colliders");
        
        // Test the result
        TestEnvironmentalSetup();
    }
    
    bool IsPlayerObject(GameObject obj)
    {
        // Check for player-related names and components
        string name = obj.name.ToLower();
        
        if (name.Contains("player") || name.Contains("xr") || name.Contains("camera") || 
            name.Contains("controller") || name.Contains("hand") || name.Contains("head"))
        {
            return true;
        }
        
        // Check for XR/VR components
        if (obj.GetComponent<Camera>() != null)
        {
            return true;
        }
        
        // Check if it's part of an XR Rig hierarchy
        Transform current = obj.transform;
        while (current != null)
        {
            if (current.name.ToLower().Contains("xr") || current.name.ToLower().Contains("rig"))
            {
                return true;
            }
            current = current.parent;
        }
        
        return false;
    }
    
    bool IsObjectTooSmall(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null) return true;
        
        Bounds bounds = renderer.bounds;
        float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        
        return maxSize < minimumObjectSize;
    }
    
    bool HasMovementComponent(GameObject obj)
    {
        // Check for common movement components
        return obj.GetComponent<Rigidbody>() != null ||
               obj.GetComponent<UnityEngine.AI.NavMeshAgent>() != null ||
               obj.GetComponentInChildren<EnemyMovement>() != null;
    }
    
    [ContextMenu("Test Environmental Setup")]
    public void TestEnvironmentalSetup()
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        Debug.Log($"[ForceEnvironmentalSetup] 📊 Found {walls.Length} objects tagged as 'Wall'");
        
        int wallsWithColliders = 0;
        foreach (GameObject wall in walls)
        {
            if (wall.GetComponent<Collider>() != null)
            {
                wallsWithColliders++;
            }
        }
        
        Debug.Log($"[ForceEnvironmentalSetup] 📊 {wallsWithColliders}/{walls.Length} walls have colliders");
        
        if (wallsWithColliders > 0)
        {
            Debug.Log($"[ForceEnvironmentalSetup] ✅ SUCCESS! Environmental collision should now work.");
        }
        else
        {
            Debug.LogError($"[ForceEnvironmentalSetup] ❌ Still no walls with colliders found!");
        }
        
        // Test physics
        TestPhysicsSystem();
    }
    
    void TestPhysicsSystem()
    {
        // Test if Physics.OverlapSphere finds any colliders
        Vector3 testPosition = transform.position;
        Collider[] colliders = Physics.OverlapSphere(testPosition, 5f);
        
        Debug.Log($"[ForceEnvironmentalSetup] 🧪 Physics test at {testPosition}: found {colliders.Length} colliders within 5m");
        
        foreach (Collider col in colliders)
        {
            Debug.Log($"   - {col.name} (tag: {col.tag})");
        }
    }
    
    [ContextMenu("Setup Specific Objects by Name")]
    public void SetupSpecificObjects()
    {
        // Look for specific object patterns that are definitely walls/environment
        string[] definiteWallPatterns = {
            "wall", "floor", "ground", "ceiling", "pillar", "column", 
            "building", "structure", "chamber", "room", "door", "window"
        };
        
        MeshRenderer[] allRenderers = FindObjectsOfType<MeshRenderer>();
        int objectsFound = 0;
        
        foreach (MeshRenderer renderer in allRenderers)
        {
            GameObject obj = renderer.gameObject;
            string name = obj.name.ToLower();
            
            bool isDefiniteWall = false;
            foreach (string pattern in definiteWallPatterns)
            {
                if (name.Contains(pattern))
                {
                    isDefiniteWall = true;
                    break;
                }
            }
            
            if (isDefiniteWall && obj.GetComponent<Collider>() == null)
            {
                if (useMeshColliders)
                {
                    obj.AddComponent<MeshCollider>();
                }
                else
                {
                    obj.AddComponent<BoxCollider>();
                }
                
                obj.tag = "Wall";
                objectsFound++;
                
                Debug.Log($"[ForceEnvironmentalSetup] Added collider to definite wall: {obj.name}");
            }
        }
        
        Debug.Log($"[ForceEnvironmentalSetup] ✅ Found and setup {objectsFound} definite wall objects");
    }
    
    [ContextMenu("List All Large Objects")]
    public void ListAllLargeObjects()
    {
        Debug.Log("[ForceEnvironmentalSetup] === LISTING ALL LARGE OBJECTS ===");
        
        MeshRenderer[] allRenderers = FindObjectsOfType<MeshRenderer>();
        int largeObjects = 0;
        
        foreach (MeshRenderer renderer in allRenderers)
        {
            GameObject obj = renderer.gameObject;
            
            if (IsPlayerObject(obj)) continue;
            
            Bounds bounds = renderer.bounds;
            float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            
            if (maxSize >= minimumObjectSize)
            {
                bool hasCollider = obj.GetComponent<Collider>() != null;
                Debug.Log($"   • {obj.name} - Size: {maxSize:F1}m - Collider: {hasCollider} - Tag: {obj.tag}");
                largeObjects++;
            }
        }
        
        Debug.Log($"[ForceEnvironmentalSetup] Found {largeObjects} large objects total");
    }
}
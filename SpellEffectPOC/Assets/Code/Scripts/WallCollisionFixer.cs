using UnityEngine;

/// <summary>
/// Fixes wall collision issues by ensuring walls are on the correct layer for player collision
/// </summary>
public class WallCollisionFixer : MonoBehaviour
{
    [Header("Fix Options")]
    [Tooltip("Move all walls back to Default layer (recommended)")]
    public bool moveWallsToDefaultLayer = true;
    
    [Tooltip("Ensure MeshColliders are not triggers")]
    public bool ensureCollidersNotTriggers = true;
    
    [Tooltip("Show detailed debug info")]
    public bool showDebugInfo = true;

    [ContextMenu("Fix Wall Collision Issues")]
    public void FixWallCollisionIssues()
    {
        Debug.Log("🔧 === FIXING WALL COLLISION ISSUES ===");
        
        // Find all wall objects
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        Debug.Log($"🔍 Found {walls.Length} objects tagged as 'Wall'");
        
        int wallsFixed = 0;
        int collidersFixed = 0;
        int layersChanged = 0;
        
        foreach (GameObject wall in walls)
        {
            bool wallWasFixed = false;
            
            // Check and fix layer
            if (moveWallsToDefaultLayer && wall.layer != 0)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"🔧 Moving '{wall.name}' from layer {wall.layer} ({LayerMask.LayerToName(wall.layer)}) to Default layer (0)");
                }
                wall.layer = 0; // Default layer
                layersChanged++;
                wallWasFixed = true;
            }
            
            // Check and fix collider settings
            Collider collider = wall.GetComponent<Collider>();
            if (collider != null)
            {
                if (ensureCollidersNotTriggers && collider.isTrigger)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log($"🔧 Setting '{wall.name}' collider to NOT be a trigger");
                    }
                    collider.isTrigger = false;
                    collidersFixed++;
                    wallWasFixed = true;
                }
                
                // Special check for MeshColliders
                MeshCollider meshCollider = collider as MeshCollider;
                if (meshCollider != null)
                {
                    // Ensure non-convex for better wall collision
                    if (meshCollider.convex)
                    {
                        if (showDebugInfo)
                        {
                            Debug.Log($"🔧 Setting '{wall.name}' MeshCollider to non-convex for better wall collision");
                        }
                        meshCollider.convex = false;
                        wallWasFixed = true;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Wall '{wall.name}' has no collider! This wall won't block anything.");
            }
            
            if (wallWasFixed)
            {
                wallsFixed++;
            }
        }
        
        Debug.Log($"🔧 === WALL COLLISION FIX COMPLETE ===");
        Debug.Log($"✅ Walls processed: {walls.Length}");
        Debug.Log($"✅ Walls fixed: {wallsFixed}");
        Debug.Log($"✅ Layers changed: {layersChanged}");
        Debug.Log($"✅ Colliders fixed: {collidersFixed}");
        
        // Test the results
        TestCollisionSetup();
        
        if (wallsFixed > 0)
        {
            Debug.Log("🎉 COLLISION SHOULD NOW WORK! Try walking into walls - you should NOT be able to go through them.");
        }
        else
        {
            Debug.Log("ℹ️ No changes were needed. If collision still doesn't work, check the Physics collision matrix in Project Settings > Physics.");
        }
    }
    
    [ContextMenu("Test Collision Setup")]
    public void TestCollisionSetup()
    {
        Debug.Log("🧪 === TESTING COLLISION SETUP ===");
        
        // Find XR Rig
        GameObject xrRig = GameObject.FindWithTag("Player");
        if (xrRig == null)
        {
            xrRig = FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.ActionBasedContinuousMoveProvider>()?.gameObject;
        }
        
        if (xrRig != null)
        {
            Debug.Log($"🤖 Player (XR Rig): '{xrRig.name}' on layer {xrRig.layer} ({LayerMask.LayerToName(xrRig.layer)})");
            
            // Check for CharacterController
            CharacterController charController = xrRig.GetComponent<CharacterController>();
            if (charController != null)
            {
                Debug.Log($"✅ CharacterController found - Height: {charController.height}, Radius: {charController.radius}");
            }
            else
            {
                Debug.LogError("❌ No CharacterController found on XR Rig! This is required for collision.");
            }
        }
        else
        {
            Debug.LogError("❌ Could not find XR Rig/Player object!");
        }
        
        // Check walls
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        int wallsOnDefaultLayer = 0;
        int wallsWithValidColliders = 0;
        
        foreach (GameObject wall in walls)
        {
            if (wall.layer == 0) wallsOnDefaultLayer++;
            
            Collider collider = wall.GetComponent<Collider>();
            if (collider != null && !collider.isTrigger)
            {
                wallsWithValidColliders++;
            }
        }
        
        Debug.Log($"🧱 Walls on Default layer: {wallsOnDefaultLayer}/{walls.Length}");
        Debug.Log($"🔒 Walls with solid colliders: {wallsWithValidColliders}/{walls.Length}");
        
        if (wallsOnDefaultLayer == walls.Length && wallsWithValidColliders == walls.Length)
        {
            Debug.Log("✅ ALL COLLISION SETUP LOOKS GOOD!");
        }
        else
        {
            Debug.LogWarning("⚠️ Some issues detected. Run 'Fix Wall Collision Issues' to resolve them.");
        }
    }
    
    [ContextMenu("Show Physics Layer Matrix Info")]
    public void ShowPhysicsLayerInfo()
    {
        Debug.Log("ℹ️ === PHYSICS LAYER INFORMATION ===");
        Debug.Log("To manually check/fix layer collision:");
        Debug.Log("1. Go to Edit > Project Settings > Physics");
        Debug.Log("2. Look at the 'Layer Collision Matrix' at the bottom");
        Debug.Log("3. Make sure 'Default' and 'Wall' layers have a checkmark between them");
        Debug.Log("4. If there's no checkmark, click to enable collision between these layers");
        
        // Test collision between layers
        bool defaultWallCollision = !Physics.GetIgnoreLayerCollision(0, LayerMask.NameToLayer("Wall"));
        Debug.Log($"Default ↔ Wall collision enabled: {defaultWallCollision}");
        
        if (!defaultWallCollision)
        {
            Debug.LogError("❌ Default and Wall layers do NOT collide! This is likely the problem.");
            Debug.LogError("Fix: Enable collision between Default (0) and Wall layers in Physics settings.");
        }
    }
}
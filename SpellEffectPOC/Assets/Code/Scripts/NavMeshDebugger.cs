using UnityEngine;
using UnityEngine.AI;

public class NavMeshDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showGizmos = true;
    public float testRadius = 10f;
    public int testPoints = 20;
    
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // Test multiple points around this position
        for (int i = 0; i < testPoints; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * testRadius;
            Vector3 testPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            // Test with AllAreas
            NavMeshHit hitAll;
            bool foundAll = NavMesh.SamplePosition(testPosition, out hitAll, 2f, NavMesh.AllAreas);
            
            // Test with only Walkable area
            NavMeshHit hitWalkable;
            int walkableAreaMask = 1 << 0;
            bool foundWalkable = NavMesh.SamplePosition(testPosition, out hitWalkable, 2f, walkableAreaMask);
            
            // Color coding:
            // Green = Found with walkable mask
            // Red = Found with AllAreas but NOT with walkable mask (water area)
            // Yellow = Not found at all
            
            if (foundWalkable)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(hitWalkable.position, 0.2f);
            }
            else if (foundAll)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(hitAll.position, 0.2f);
            }
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(testPosition, 0.1f);
            }
        }
    }
    
    [ContextMenu("Test Current Position")]
    public void TestCurrentPosition()
    {
        Vector3 pos = transform.position;
        
        NavMeshHit hitAll;
        bool foundAll = NavMesh.SamplePosition(pos, out hitAll, 2f, NavMesh.AllAreas);
        
        NavMeshHit hitWalkable;
        int walkableAreaMask = 1 << 0;
        bool foundWalkable = NavMesh.SamplePosition(pos, out hitWalkable, 2f, walkableAreaMask);
        
        Debug.Log($"Position {pos}:");
        Debug.Log($"  AllAreas: {foundAll} (Area: {(foundAll ? hitAll.mask : 0)})");
        Debug.Log($"  WalkableOnly: {foundWalkable} (Area: {(foundWalkable ? hitWalkable.mask : 0)})");
        
        if (foundAll && !foundWalkable)
        {
            Debug.Log("  *** This position is in a non-walkable area! ***");
        }
    }
} 
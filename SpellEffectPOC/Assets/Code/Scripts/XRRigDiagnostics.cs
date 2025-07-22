using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Diagnostic script to help troubleshoot XR Rig falling issues
/// Attach this to your XR Rig and check the console output
/// </summary>
public class XRRigDiagnostics : MonoBehaviour
{
    [Header("Diagnostic Controls")]
    [Tooltip("Run diagnostics automatically on start")]
    public bool autoRunOnStart = true;
    
    [Tooltip("Show continuous real-time info")]
    public bool showRealTimeInfo = false;
    
    [Tooltip("Check ground collision every frame")]
    public bool checkGroundCollision = true;
    
    private CharacterController characterController;
    private ActionBasedContinuousMoveProvider moveProvider;
    private Transform cameraTransform;
    
    void Start()
    {
        if (autoRunOnStart)
        {
            RunDiagnostics();
        }
    }
    
    void Update()
    {
        if (showRealTimeInfo)
        {
            ShowRealTimeInfo();
        }
        
        if (checkGroundCollision)
        {
            CheckGroundCollision();
        }
    }
    
    [ContextMenu("Run Diagnostics")]
    public void RunDiagnostics()
    {
        Debug.Log("🔍 === XR RIG DIAGNOSTICS ===");
        
        // Find components
        characterController = GetComponent<CharacterController>();
        moveProvider = GetComponent<ActionBasedContinuousMoveProvider>();
        
        // Find camera
        Camera camera = GetComponentInChildren<Camera>();
        if (camera != null)
        {
            cameraTransform = camera.transform;
        }
        
        CheckCharacterController();
        CheckMovementProvider();
        CheckPosition();
        CheckGroundAtPosition();
        CheckPhysicsSettings();
        CheckCollisionLayers();
        
        Debug.Log("🔍 === END DIAGNOSTICS ===");
    }
    
    void CheckCharacterController()
    {
        Debug.Log("📦 CHARACTER CONTROLLER CHECK:");
        
        if (characterController == null)
        {
            Debug.LogError("❌ NO CharacterController found! This is the problem - XR Rig needs a CharacterController component.");
            return;
        }
        
        Debug.Log($"✅ CharacterController found");
        Debug.Log($"   • Enabled: {characterController.enabled}");
        Debug.Log($"   • Height: {characterController.height}");
        Debug.Log($"   • Radius: {characterController.radius}");
        Debug.Log($"   • Center: {characterController.center}");
        Debug.Log($"   • Skin Width: {characterController.skinWidth}");
        Debug.Log($"   • Min Move Distance: {characterController.minMoveDistance}");
        Debug.Log($"   • Slope Limit: {characterController.slopeLimit}");
        Debug.Log($"   • Step Offset: {characterController.stepOffset}");
        
        // Check if character controller is grounded
        Debug.Log($"   • Is Grounded: {characterController.isGrounded}");
        
        if (!characterController.isGrounded)
        {
            Debug.LogWarning("⚠️ CharacterController is NOT grounded - this could be why it's falling!");
        }
    }
    
    void CheckMovementProvider()
    {
        Debug.Log("🚶 MOVEMENT PROVIDER CHECK:");
        
        if (moveProvider == null)
        {
            Debug.LogWarning("⚠️ No ActionBasedContinuousMoveProvider found on XR Rig");
            
            // Look for it anywhere in the scene
            moveProvider = FindObjectOfType<ActionBasedContinuousMoveProvider>();
            if (moveProvider != null)
            {
                Debug.Log($"✅ Found ActionBasedContinuousMoveProvider on: {moveProvider.gameObject.name}");
            }
            else
            {
                Debug.LogError("❌ No ActionBasedContinuousMoveProvider found anywhere! Movement won't work.");
                return;
            }
        }
        
        Debug.Log($"✅ ActionBasedContinuousMoveProvider found");
        Debug.Log($"   • Enabled: {moveProvider.enabled}");
        Debug.Log($"   • Move Speed: {moveProvider.moveSpeed}");
        Debug.Log($"   • Enable Strafe: {moveProvider.enableStrafe}");
        Debug.Log($"   • Enable Fly: {moveProvider.enableFly}");
        Debug.Log($"   • Use Gravity: {moveProvider.useGravity}");
        Debug.Log($"   • Gravity Application Mode: {moveProvider.gravityApplicationMode}");
        
        // Check the locomotion system reference
        if (moveProvider.system != null)
        {
            Debug.Log($"   • Locomotion System: {moveProvider.system.GetType().Name}");
            
            // Check if it's pointing to the right CharacterController
            if (moveProvider.system == characterController)
            {
                Debug.Log("✅ Movement provider correctly linked to CharacterController");
            }
            else
            {
                Debug.LogWarning($"⚠️ Movement provider linked to: {moveProvider.system.GetType().Name} instead of CharacterController!");
            }
        }
        else
        {
            Debug.LogError("❌ Movement provider has no locomotion system assigned!");
        }
    }
    
    void CheckPosition()
    {
        Debug.Log("📍 POSITION CHECK:");
        Debug.Log($"   • XR Rig Position: {transform.position}");
        Debug.Log($"   • XR Rig Rotation: {transform.eulerAngles}");
        
        if (cameraTransform != null)
        {
            Debug.Log($"   • Camera Position: {cameraTransform.position}");
        }
        
        // Calculate bottom of character controller
        if (characterController != null)
        {
            Vector3 bottom = transform.position + characterController.center - Vector3.up * (characterController.height * 0.5f);
            Debug.Log($"   • CharacterController Bottom: {bottom}");
            Debug.Log($"   • Distance from Ground (Y=0): {bottom.y}");
        }
    }
    
    void CheckGroundAtPosition()
    {
        Debug.Log("🌍 GROUND COLLISION CHECK:");
        
        Vector3 checkPosition = transform.position;
        
        // Raycast downward to check for ground
        RaycastHit hit;
        float rayDistance = 10f;
        
        if (Physics.Raycast(checkPosition, Vector3.down, out hit, rayDistance))
        {
            Debug.Log($"✅ Ground found at distance: {hit.distance:F2}m");
            Debug.Log($"   • Hit Object: {hit.collider.gameObject.name}");
            Debug.Log($"   • Hit Point: {hit.point}");
            Debug.Log($"   • Hit Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            Debug.Log($"   • Has Collider: {hit.collider != null}");
            Debug.Log($"   • Collider Type: {hit.collider.GetType().Name}");
        }
        else
        {
            Debug.LogError($"❌ NO GROUND FOUND within {rayDistance}m below XR Rig! This is likely the problem.");
            Debug.LogError("   • Make sure there are ground objects with colliders at this position");
            Debug.LogError("   • Check if ground objects are on the correct collision layer");
        }
        
        // Check for nearby colliders
        Collider[] nearbyColliders = Physics.OverlapSphere(checkPosition, 5f);
        Debug.Log($"   • Nearby Colliders (within 5m): {nearbyColliders.Length}");
        foreach (Collider col in nearbyColliders)
        {
            Debug.Log($"     - {col.gameObject.name} ({col.GetType().Name})");
        }
    }
    
    void CheckPhysicsSettings()
    {
        Debug.Log("⚙️ PHYSICS SETTINGS:");
        Debug.Log($"   • Gravity: {Physics.gravity}");
        Debug.Log($"   • Default Contact Offset: {Physics.defaultContactOffset}");
        Debug.Log($"   • Bounce Threshold: {Physics.bounceThreshold}");
    }
    
    void CheckCollisionLayers()
    {
        Debug.Log("🎭 COLLISION LAYERS:");
        Debug.Log($"   • XR Rig Layer: {LayerMask.LayerToName(gameObject.layer)} ({gameObject.layer})");
        
        if (characterController != null)
        {
            // Check what layers the character controller can collide with
            Debug.Log("   • CharacterController collision layers: Check Physics Settings > Layer Collision Matrix");
        }
    }
    
    void ShowRealTimeInfo()
    {
        if (characterController != null && Time.frameCount % 60 == 0) // Every 60 frames
        {
            Debug.Log($"[Real-time] Grounded: {characterController.isGrounded}, Velocity: {characterController.velocity}, Position: {transform.position}");
        }
    }
    
    void CheckGroundCollision()
    {
        if (characterController != null && !characterController.isGrounded)
        {
            // This will log once when the character becomes ungrounded
            if (Time.frameCount % 120 == 0) // Every 2 seconds
            {
                Debug.LogWarning($"⚠️ CharacterController is not grounded at position: {transform.position}");
            }
        }
    }
    
    void OnDrawGizmos()
    {
        // Draw diagnostic gizmos
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw raycast for ground check
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector3.down * 10f);
        
        // Draw character controller bounds (approximated with sphere and cylinder)
        if (characterController != null)
        {
            Gizmos.color = Color.green;
            Vector3 center = transform.position + characterController.center;
            
            // Draw radius as wire sphere
            Gizmos.DrawWireSphere(center, characterController.radius);
            
            // Draw height as vertical line
            Vector3 bottom = center - Vector3.up * (characterController.height * 0.5f);
            Vector3 top = center + Vector3.up * (characterController.height * 0.5f);
            Gizmos.DrawLine(bottom, top);
        }
    }
} 
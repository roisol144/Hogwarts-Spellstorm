using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class NimbusController : MonoBehaviour
{
    [Header("Nimbus Settings")]
    [SerializeField] private GameObject nimbusPrefab; // Will be created from the Nimbus 2000 model
    [SerializeField] private Material nimbusMaterial; // Material with the proper textures
    [SerializeField] private Transform leftHandTransform; // Left hand controller transform
    [SerializeField] private Vector3 nimbusOffset = new Vector3(0.05f, 0f, 0f); // Offset from hand - mirrored from wand position
    [SerializeField] private Vector3 nimbusRotationOffset = new Vector3(0f, 90f, 0); // Rotation offset - similar to wand
    [SerializeField] private float nimbusScale = 0.8f; // Scale of the nimbus (0.8 = 80% of original size)
    
    [Header("Flying Settings")]
    [SerializeField] private float normalMoveSpeed = 2f; // Normal walking speed
    [SerializeField] private float flyingMoveSpeed = 15f; // Flying speed
    [SerializeField] private bool enableGravityWhenFlying = false; // Disable gravity for flying
    
    [Header("Input Settings")]
    [SerializeField] private InputAction leftTriggerAction;
    
    [Header("Effects")]
    [SerializeField] private AudioClip summonSound;
    [SerializeField] private AudioClip dismissSound;
    [SerializeField] private ParticleSystem summonEffect; // Optional summoning effect
    
    // References
    private ActionBasedContinuousMoveProvider moveProvider;
    private AudioSource audioSource;
    private GameObject currentNimbus;
    private bool isFlying = false;
    private bool wasLeftTriggerPressed = false;
    
    // Store original movement settings
    private float originalMoveSpeed;
    private bool originalEnableFly;
    private bool originalUseGravity;

    void Awake()
    {
        // Find the move provider on the XR Rig
        moveProvider = FindObjectOfType<ActionBasedContinuousMoveProvider>();
        if (moveProvider == null)
        {
            Debug.LogError("[NimbusController] Could not find ActionBasedContinuousMoveProvider!");
        }
        
        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Find left hand transform if not assigned
        if (leftHandTransform == null)
        {
            GameObject leftHand = GameObject.Find("LeftHand Controller");
            if (leftHand != null)
            {
                leftHandTransform = leftHand.transform;
            }
            else
            {
                Debug.LogWarning("[NimbusController] Could not find LeftHand Controller. Please assign manually.");
            }
        }
        
        // Store original movement settings
        if (moveProvider != null)
        {
            originalMoveSpeed = moveProvider.moveSpeed;
            originalEnableFly = moveProvider.enableFly;
            originalUseGravity = moveProvider.useGravity;
        }
        
        // Setup input action if not configured
        if (leftTriggerAction.bindings.Count == 0)
        {
            leftTriggerAction = new InputAction("LeftTrigger", InputActionType.Button);
            leftTriggerAction.AddBinding("<XRController>{LeftHand}/triggerPressed");
            leftTriggerAction.AddBinding("<OculusTouchController>{LeftHand}/triggerPressed");
        }
    }

    void OnEnable()
    {
        leftTriggerAction.Enable();
    }

    void OnDisable()
    {
        leftTriggerAction.Disable();
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        bool isLeftTriggerPressed = leftTriggerAction.ReadValue<float>() > 0.5f;
        
        // Check for trigger press (not hold)
        if (isLeftTriggerPressed && !wasLeftTriggerPressed)
        {
            ToggleNimbus();
        }
        
        wasLeftTriggerPressed = isLeftTriggerPressed;
    }

    void ToggleNimbus()
    {
        if (isFlying)
        {
            DismissNimbus();
        }
        else
        {
            SummonNimbus();
        }
    }

    void SummonNimbus()
    {
        if (nimbusPrefab == null || leftHandTransform == null)
        {
            Debug.LogWarning("[NimbusController] Cannot summon Nimbus - missing prefab or left hand transform!");
            return;
        }

        // Create Nimbus at left hand position
        Vector3 spawnPosition = leftHandTransform.position + leftHandTransform.TransformDirection(nimbusOffset);
        Quaternion spawnRotation = leftHandTransform.rotation * Quaternion.Euler(nimbusRotationOffset);
        
        currentNimbus = Instantiate(nimbusPrefab, spawnPosition, spawnRotation);
        
        // Scale it to appropriate size for hand
        currentNimbus.transform.localScale = Vector3.one * nimbusScale;
        
        // Apply the proper material with textures
        if (nimbusMaterial != null)
        {
            Renderer[] renderers = currentNimbus.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material = nimbusMaterial;
            }
        }
        
        // Parent to left hand so it follows
        currentNimbus.transform.SetParent(leftHandTransform);
        
        // Adjust local position and rotation after parenting
        currentNimbus.transform.localPosition = nimbusOffset;
        currentNimbus.transform.localRotation = Quaternion.Euler(nimbusRotationOffset);
        
        // Enable flying mode
        EnableFlyingMode();
        
        // Play effects
        PlaySummonEffects();
        
        isFlying = true;
        
        Debug.Log("[NimbusController] Nimbus 2000 summoned! Flying mode enabled.");
    }

    void DismissNimbus()
    {
        if (currentNimbus != null)
        {
            Destroy(currentNimbus);
            currentNimbus = null;
        }
        
        // Disable flying mode
        DisableFlyingMode();
        
        // Play effects
        PlayDismissEffects();
        
        isFlying = false;
        
        Debug.Log("[NimbusController] Nimbus 2000 dismissed! Walking mode restored.");
    }

    void EnableFlyingMode()
    {
        if (moveProvider != null)
        {
            moveProvider.moveSpeed = flyingMoveSpeed;
            moveProvider.enableFly = true;
            moveProvider.useGravity = enableGravityWhenFlying;
            
            Debug.Log($"[NimbusController] Flying mode enabled - Speed: {flyingMoveSpeed}, Gravity: {enableGravityWhenFlying}");
        }
    }

    void DisableFlyingMode()
    {
        if (moveProvider != null)
        {
            moveProvider.moveSpeed = originalMoveSpeed;
            moveProvider.enableFly = originalEnableFly;
            moveProvider.useGravity = originalUseGravity;
            
            Debug.Log($"[NimbusController] Walking mode restored - Speed: {originalMoveSpeed}");
        }
    }

    void PlaySummonEffects()
    {
        // Play summon sound
        if (summonSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(summonSound);
        }
        
        // Play summon particle effect
        if (summonEffect != null)
        {
            summonEffect.transform.position = leftHandTransform.position;
            summonEffect.Play();
        }
    }

    void PlayDismissEffects()
    {
        // Play dismiss sound
        if (dismissSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dismissSound);
        }
    }

    void OnDestroy()
    {
        // Clean up if still flying
        if (isFlying)
        {
            DisableFlyingMode();
        }
    }

    // Public methods for external access
    public bool IsFlying => isFlying;
    public bool HasNimbus => currentNimbus != null;
    
    public void ForceEnableFlyingMode()
    {
        if (!isFlying) SummonNimbus();
    }
    
    public void ForceDisableFlyingMode()
    {
        if (isFlying) DismissNimbus();
    }
} 
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class RunningController : MonoBehaviour
{
    [Header("Running Settings")]
    [SerializeField] private float normalWalkSpeed = 2f; // Normal walking speed
    [SerializeField] private float runningSpeed = 4f; // Running speed (2x normal, not too fast)
    [SerializeField] private float speedTransitionTime = 0.3f; // How quickly to transition between speeds
    
    [Header("Input Settings")]
    [SerializeField] private InputAction leftGripAction;
    
    [Header("Audio")]
    [SerializeField] private AudioClip runningSound;
    [SerializeField] private AudioSource audioSource;
    
    // References
    private ActionBasedContinuousMoveProvider moveProvider;
    private NimbusController nimbusController;
    
    // State tracking
    private bool isRunning = false;
    private bool wasGripPressed = false;
    private float currentSpeed;
    private float targetSpeed;
    
    void Awake()
    {
        // Find the move provider on the XR Rig
        moveProvider = FindObjectOfType<ActionBasedContinuousMoveProvider>();
        if (moveProvider == null)
        {
            Debug.LogError("[RunningController] Could not find ActionBasedContinuousMoveProvider!");
        }
        
        // Find nimbus controller to check for flying mode
        nimbusController = FindObjectOfType<NimbusController>();
        
        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Setup input action if not configured
        if (leftGripAction.bindings.Count == 0)
        {
            leftGripAction = new InputAction("LeftGrip", InputActionType.Button);
            leftGripAction.AddBinding("<XRController>{LeftHand}/gripPressed");
            leftGripAction.AddBinding("<OculusTouchController>{LeftHand}/gripPressed");
        }
        
        // Initialize speed values
        if (moveProvider != null)
        {
            currentSpeed = moveProvider.moveSpeed;
            targetSpeed = currentSpeed;
            normalWalkSpeed = moveProvider.moveSpeed; // Use the current speed as normal speed
        }
    }

    void OnEnable()
    {
        leftGripAction.Enable();
    }

    void OnDisable()
    {
        leftGripAction.Disable();
    }

    void Update()
    {
        HandleInput();
        UpdateSpeed();
    }

    void HandleInput()
    {
        // Don't allow running while flying
        if (nimbusController != null && nimbusController.isActiveAndEnabled)
        {
            // Check if Nimbus is currently active (flying)
            // Note: We'd need to expose the isFlying state from NimbusController
            // For now, we'll allow running unless we add a public property
        }
        
        bool isGripPressed = leftGripAction.ReadValue<float>() > 0.5f;
        
        // Check for grip press/release
        if (isGripPressed && !wasGripPressed)
        {
            StartRunning();
        }
        else if (!isGripPressed && wasGripPressed)
        {
            StopRunning();
        }
        
        wasGripPressed = isGripPressed;
    }

    void StartRunning()
    {
        isRunning = true;
        targetSpeed = runningSpeed;
        
        // Play running sound
        if (runningSound != null && audioSource != null)
        {
            audioSource.clip = runningSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        Debug.Log($"[RunningController] Started running - Target speed: {runningSpeed}");
    }

    void StopRunning()
    {
        isRunning = false;
        targetSpeed = normalWalkSpeed;
        
        // Stop running sound
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        Debug.Log($"[RunningController] Stopped running - Target speed: {normalWalkSpeed}");
    }

    void UpdateSpeed()
    {
        if (moveProvider == null) return;
        
        // Smoothly transition to target speed
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / speedTransitionTime);
        
        // Apply speed to movement provider (only if not flying)
        bool isFlying = IsCurrentlyFlying();
        if (!isFlying)
        {
            moveProvider.moveSpeed = currentSpeed;
        }
    }

    bool IsCurrentlyFlying()
    {
        // Check if the player is currently flying (high move speed indicates flying)
        // This is a simple check - could be improved with direct communication to NimbusController
        return moveProvider != null && moveProvider.moveSpeed > 10f;
    }

    // Public getters for debugging/UI
    public bool IsRunning() => isRunning;
    public float GetCurrentSpeed() => currentSpeed;
    public float GetNormalSpeed() => normalWalkSpeed;
    public float GetRunningSpeed() => runningSpeed;
    
    // Public setters for configuration
    public void SetNormalSpeed(float speed)
    {
        normalWalkSpeed = Mathf.Max(0f, speed);
        if (!isRunning)
        {
            targetSpeed = normalWalkSpeed;
        }
    }
    
    public void SetRunningSpeed(float speed)
    {
        runningSpeed = Mathf.Max(normalWalkSpeed, speed);
        if (isRunning)
        {
            targetSpeed = runningSpeed;
        }
    }

    void OnDestroy()
    {
        // Clean up audio
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    // Context menu for testing
    [ContextMenu("Test Start Running")]
    public void TestStartRunning()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[RunningController] Test can only be run in Play mode");
            return;
        }
        
        Debug.Log("[RunningController] Testing running...");
        StartRunning();
    }
    
    [ContextMenu("Test Stop Running")]
    public void TestStopRunning()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[RunningController] Test can only be run in Play mode");
            return;
        }
        
        Debug.Log("[RunningController] Testing stop running...");
        StopRunning();
    }
} 
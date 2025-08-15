using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Spell Book UI that appears when holding A button on right Quest controller
/// Only works in Chamber of Secrets and Dungeons scenes
/// </summary>
public class SpellBookUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas spellBookCanvas;
    [SerializeField] private Image spellBookImage;
    
    [Header("Spell Book Image")]
    [SerializeField] private Sprite spellBookSprite;
    
    [Header("VR UI Positioning")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Vector3 offsetFromCamera = new Vector3(0f, 0f, 1.5f); // Position in front of player
    [SerializeField] private float followSpeed = 8f; // How fast to follow camera
    [SerializeField] private bool lookAtCamera = true; // Whether to face the camera
    [SerializeField] private float canvasScale = 0.002f; // Scale of the world space canvas (much smaller)
    [SerializeField] private Vector2 canvasSize = new Vector2(400f, 300f); // Canvas size in pixels (much smaller)
    
    [Header("Input Settings")]
    [SerializeField] private InputAction rightHandAPressAction;
    
    [Header("Scene Settings")]
    [SerializeField] private string[] allowedSceneNames = { "ChamberOfSecretsScene", "DungeonsScene" };
    
    // Private variables
    private bool isSpellBookVisible = false;
    private bool isInAllowedScene = false;
    private Coroutine followCoroutine;
    
    void Awake()
    {
        SetupInputAction();
        CheckCurrentScene();
    }
    
    void Start()
    {
        SetupSpellBookUI();
    }
    
    void OnEnable()
    {
        rightHandAPressAction?.Enable();
    }
    
    void OnDisable()
    {
        rightHandAPressAction?.Disable();
        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }
    }
    
    void Update()
    {
        HandleInput();
    }
    
    /// <summary>
    /// Setup the input action for the right hand A button
    /// </summary>
    private void SetupInputAction()
    {
        rightHandAPressAction = new InputAction("RightHandAPress", InputActionType.Button);
        
        // Add bindings for Quest controller A button (primary button on right hand)
        rightHandAPressAction.AddBinding("<XRController>{RightHand}/primaryButton");
        rightHandAPressAction.AddBinding("<OculusTouchController>{RightHand}/primaryButton");
        
        rightHandAPressAction.performed += OnAPress;
        rightHandAPressAction.canceled += OnARelease;
        
        Debug.Log("[SpellBookUI] Input action setup complete - A button on right Quest controller");
    }
    
    /// <summary>
    /// Check if we are in an allowed scene
    /// </summary>
    private void CheckCurrentScene()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        isInAllowedScene = System.Array.Exists(allowedSceneNames, scene => scene.Equals(currentSceneName, System.StringComparison.OrdinalIgnoreCase));
        
        Debug.Log($"[SpellBookUI] Current scene: {currentSceneName}, Allowed: {isInAllowedScene}");
    }
    
    /// <summary>
    /// Setup the spell book UI canvas and components
    /// </summary>
    private void SetupSpellBookUI()
    {
        // Find camera if not assigned
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindObjectOfType<Camera>();
            }
        }
        
        // Create canvas if not assigned
        if (spellBookCanvas == null)
        {
            CreateSpellBookCanvas();
        }
        
        // Load spell book sprite if not assigned
        if (spellBookSprite == null)
        {
            LoadSpellBookSprite();
        }
        
        // Setup spell book image
        if (spellBookImage != null && spellBookSprite != null)
        {
            spellBookImage.sprite = spellBookSprite;
            spellBookImage.preserveAspect = true;
            Debug.Log("[SpellBookUI] Set spell book sprite to image component");
        }
        
        // Close button removed - spell book only shows while A button is held
        
        // Initially hide the spell book
        SetSpellBookVisibility(false);
        
        Debug.Log("[SpellBookUI] Spell book UI setup complete");
    }
    
    /// <summary>
    /// Load the spell book sprite from Resources
    /// </summary>
    private void LoadSpellBookSprite()
    {
        // Try to load SpellBook.png as a Texture first (since it is configured as Texture, not Sprite)
        Texture2D spellBookTexture = Resources.Load<Texture2D>("SpellBook");
        if (spellBookTexture != null)
        {
            // Convert Texture2D to Sprite
            spellBookSprite = Sprite.Create(spellBookTexture, new Rect(0, 0, spellBookTexture.width, spellBookTexture.height), new Vector2(0.5f, 0.5f));
            Debug.Log("[SpellBookUI] Successfully loaded SpellBook.png as Texture and converted to Sprite");
        }
        else
        {
            Debug.LogWarning("[SpellBookUI] Could not load SpellBook.png as Texture from Resources folder!");
            
            // Try to load as Sprite (in case it was changed to Sprite mode)
            spellBookSprite = Resources.Load<Sprite>("SpellBook");
            if (spellBookSprite != null)
            {
                Debug.Log("[SpellBookUI] Successfully loaded SpellBook.png as Sprite from Resources");
            }
            else
            {
                Debug.LogWarning("[SpellBookUI] Could not load SpellBook.png as Sprite either!");
                
                // Try alternative - TestImageForSpellBook.jpeg
                spellBookSprite = Resources.Load<Sprite>("TestImageForSpellBook");
                if (spellBookSprite == null)
                {
                    Debug.LogError("[SpellBookUI] Failed to load any spell book sprite. Please ensure SpellBook.png or TestImageForSpellBook.jpeg exists in Assets/Resources/");
                }
                else
                {
                    Debug.Log("[SpellBookUI] Successfully loaded TestImageForSpellBook.jpeg sprite from Resources (fallback)");
                }
            }
        }
    }
    
    /// <summary>
    /// Create the spell book canvas
    /// </summary>
    private void CreateSpellBookCanvas()
    {
        GameObject canvasObject = new GameObject("SpellBookCanvas");
        canvasObject.transform.SetParent(transform, false);
        spellBookCanvas = canvasObject.AddComponent<Canvas>();
        spellBookCanvas.renderMode = RenderMode.WorldSpace;
        spellBookCanvas.worldCamera = targetCamera;
        spellBookCanvas.sortingOrder = 200; // High priority to appear above other UI
        
        // Setup canvas properties
        RectTransform canvasRect = spellBookCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = canvasSize;
        canvasRect.localScale = Vector3.one * canvasScale;
        
        // Add Canvas Scaler
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;
        
        // Add Graphic Raycaster for button interaction
        canvasObject.AddComponent<GraphicRaycaster>();
        
        // Create spell book image
        CreateSpellBookImage(canvasObject);
        
        // Close button removed - spell book only shows while A button is held
        
        Debug.Log("[SpellBookUI] Created spell book canvas");
    }
    
    /// <summary>
    /// Create the spell book image component
    /// </summary>
    private void CreateSpellBookImage(GameObject canvasObject)
    {
        GameObject imageObject = new GameObject("SpellBookImage");
        imageObject.transform.SetParent(canvasObject.transform, false);
        spellBookImage = imageObject.AddComponent<Image>();
        
        if (spellBookSprite != null)
        {
            spellBookImage.sprite = spellBookSprite;
            Debug.Log("[SpellBookUI] Set spell book sprite to image component");
        }
        else
        {
            Debug.LogWarning("[SpellBookUI] No spell book sprite available for image component");
        }
        
        spellBookImage.preserveAspect = true;
        spellBookImage.color = Color.white;
        
        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = Vector2.zero;
        imageRect.sizeDelta = new Vector2(350f, 250f); // Much smaller than canvas
    }
    

    
    /// <summary>
    /// Handle input for A button press/release
    /// </summary>
    private void HandleInput()
    {
        if (!isInAllowedScene) return;
        
        // Input is handled through the InputAction callbacks
    }
    
    /// <summary>
    /// Called when A button is pressed
    /// </summary>
    private void OnAPress(InputAction.CallbackContext context)
    {
        if (!isInAllowedScene) return;
        
        ShowSpellBook();
    }
    
    /// <summary>
    /// Called when A button is released
    /// </summary>
    private void OnARelease(InputAction.CallbackContext context)
    {
        if (!isInAllowedScene) return;
        
        HideSpellBook();
    }
    
    /// <summary>
    /// Show the spell book UI
    /// </summary>
    private void ShowSpellBook()
    {
        if (isSpellBookVisible) return;
        
        // Reset position to below camera for flying animation
        if (spellBookCanvas != null && targetCamera != null)
        {
            Vector3 startPosition = targetCamera.transform.position + 
                                  targetCamera.transform.forward * offsetFromCamera.z + 
                                  targetCamera.transform.up * (offsetFromCamera.y - 2f) + // Start 2 units below
                                  targetCamera.transform.right * offsetFromCamera.x;
            spellBookCanvas.transform.position = startPosition;
        }
        
        SetSpellBookVisibility(true);
        StartFollowingCamera();
        
        Debug.Log("[SpellBookUI] Spell book shown with flying animation");
    }
    
    /// <summary>
    /// Hide the spell book UI
    /// </summary>
    private void HideSpellBook()
    {
        if (!isSpellBookVisible) return;
        
        SetSpellBookVisibility(false);
        StopFollowingCamera();
        
        Debug.Log("[SpellBookUI] Spell book hidden");
    }
    
    /// <summary>
    /// Set the visibility of the spell book
    /// </summary>
    private void SetSpellBookVisibility(bool visible)
    {
        isSpellBookVisible = visible;
        
        if (spellBookCanvas != null)
        {
            spellBookCanvas.gameObject.SetActive(visible);
        }
    }
    
    /// <summary>
    /// Start following the camera
    /// </summary>
    private void StartFollowingCamera()
    {
        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
        }
        followCoroutine = StartCoroutine(FollowCameraCoroutine());
    }
    
    /// <summary>
    /// Stop following the camera
    /// </summary>
    private void StopFollowingCamera()
    {
        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }
    }
    
    /// <summary>
    /// Coroutine to follow the camera smoothly
    /// </summary>
    private IEnumerator FollowCameraCoroutine()
    {
        while (isSpellBookVisible && targetCamera != null)
        {
            Vector3 targetPosition = GetTargetPosition();
            
            // Smooth following
            spellBookCanvas.transform.position = Vector3.Lerp(
                spellBookCanvas.transform.position,
                targetPosition,
                Time.deltaTime * followSpeed
            );
            
            // Keep the canvas always facing forward (no rotation with head movement)
            spellBookCanvas.transform.rotation = targetCamera.transform.rotation;
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Get the target position for the spell book (in front of camera)
    /// </summary>
    private Vector3 GetTargetPosition()
    {
        if (targetCamera == null) return Vector3.zero;
        
        return targetCamera.transform.position + 
               targetCamera.transform.forward * offsetFromCamera.z + 
               targetCamera.transform.up * offsetFromCamera.y + 
               targetCamera.transform.right * offsetFromCamera.x;
    }
    
    /// <summary>
    /// Public method to manually show/hide spell book (for testing)
    /// </summary>
    [ContextMenu("Toggle Spell Book")]
    public void ToggleSpellBook()
    {
        if (isSpellBookVisible)
        {
            HideSpellBook();
        }
        else
        {
            ShowSpellBook();
        }
    }
    
    /// <summary>
    /// Public method to check if spell book is visible
    /// </summary>
    public bool IsSpellBookVisible()
    {
        return isSpellBookVisible;
    }
    
    /// <summary>
    /// Public method to check if we are in an allowed scene
    /// </summary>
    public bool IsInAllowedScene()
    {
        return isInAllowedScene;
    }
}

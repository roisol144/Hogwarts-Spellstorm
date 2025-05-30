using UnityEngine;
using TMPro;

public class MagicalDebugUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas debugCanvas;
    [SerializeField] private TextMeshProUGUI spellText;
    
    [Header("Camera Following")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Vector3 offsetFromCamera = new Vector3(-0.4f, -0.3f, 0.8f); // Position relative to camera
    [SerializeField] private float followSpeed = 5f; // How fast to follow (0 = instant, higher = smoother)
    [SerializeField] private bool lookAtCamera = true; // Whether to face the camera
    
    [Header("Canvas Settings")]
    [SerializeField] private float canvasScale = 0.003f; // Scale of the world space canvas
    [SerializeField] private Vector2 canvasSize = new Vector2(800f, 200f); // Wider canvas size in pixels
    
    [Header("Text Settings")]
    [SerializeField] private float fontSize = 48f; // Increased font size
    [SerializeField] private Color textColor = new Color(1f, 0.84f, 0f, 1f); // Golden color
    
    private Coroutine currentAnimation; // Track the current animation coroutine
    
    private void Start()
    {
        SetupDebugUI();
    }
    
    private void LateUpdate()
    {
        // Follow the camera smoothly
        if (targetCamera != null && debugCanvas != null)
        {
            FollowCamera();
        }
    }
    
    private void SetupDebugUI()
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
        
        // Create or setup the debug canvas
        if (debugCanvas == null)
        {
            CreateWorldSpaceCanvas();
        }
        else
        {
            SetupExistingCanvas();
        }
        
        // Setup the text component
        SetupSpellText();
        
        Debug.Log($"[MagicalDebugUI] Setup complete. Following camera: {targetCamera?.name}");
    }
    
    private void CreateWorldSpaceCanvas()
    {
        // Create a new GameObject for the canvas
        GameObject canvasGO = new GameObject("MagicalDebugCanvas_WorldSpace");
        debugCanvas = canvasGO.AddComponent<Canvas>();
        
        // Configure as World Space Canvas
        debugCanvas.renderMode = RenderMode.WorldSpace;
        debugCanvas.sortingOrder = 100;
        
        // Set initial position near camera
        if (targetCamera != null)
        {
            canvasGO.transform.position = GetTargetPosition();
            if (lookAtCamera)
        {
                canvasGO.transform.LookAt(targetCamera.transform);
            }
        }
        
        // Set canvas scale
        canvasGO.transform.localScale = Vector3.one * canvasScale;
        
        // Set canvas size
        var rectTransform = canvasGO.GetComponent<RectTransform>();
        rectTransform.sizeDelta = canvasSize;
        
        // Add CanvasScaler for consistent scaling
        var scaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;
        
        Debug.Log($"[MagicalDebugUI] Created World Space Canvas at position: {canvasGO.transform.position}");
    }
    
    private void SetupExistingCanvas()
    {
        // Configure existing canvas as World Space
        debugCanvas.renderMode = RenderMode.WorldSpace;
        debugCanvas.sortingOrder = 100;
        
        // Set scale and size
        debugCanvas.transform.localScale = Vector3.one * canvasScale;
        var rectTransform = debugCanvas.GetComponent<RectTransform>();
        rectTransform.sizeDelta = canvasSize;
        
        Debug.Log("[MagicalDebugUI] Configured existing canvas for world space following");
    }
    
    private void SetupSpellText()
    {
        if (spellText == null)
        {
            // Find existing spell text or create new one
            spellText = GetComponentInChildren<TextMeshProUGUI>();
            
            if (spellText == null)
            {
                // Create new text component
                GameObject textGO = new GameObject("SpellCastText");
                textGO.transform.SetParent(debugCanvas.transform, false);
                spellText = textGO.AddComponent<TextMeshProUGUI>();
            }
        }
        
        // Configure the text
        spellText.text = "Ready to cast...";
        spellText.fontSize = fontSize;
        spellText.color = textColor;
        spellText.alignment = TextAlignmentOptions.Center;
        spellText.enableWordWrapping = false; // Disable word wrapping to prevent vertical text
        spellText.fontStyle = FontStyles.Bold;
        spellText.overflowMode = TextOverflowModes.Overflow; // Allow overflow instead of wrapping
        
        // Position text to fill the canvas
        var textRect = spellText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = new Vector2(20, 20);
        textRect.offsetMax = new Vector2(-20, -20);
        
        // Try to load a magical font
        LoadMagicalFont();
        
        Debug.Log($"[MagicalDebugUI] Text setup complete. Font size: {spellText.fontSize}");
    }
    
    private void LoadMagicalFont()
    {
        // Try to load Harry Potter style font from Resources
        var magicalFont = Resources.Load<TMP_FontAsset>("Fonts/HarryPotter");
        if (magicalFont == null)
        {
            magicalFont = Resources.Load<TMP_FontAsset>("Fonts/MagicalFont");
        }
        if (magicalFont == null)
        {
            magicalFont = Resources.Load<TMP_FontAsset>("Fonts/WizardFont");
        }
        
        if (magicalFont != null)
        {
            spellText.font = magicalFont;
            Debug.Log("[MagicalDebugUI] Loaded magical font: " + magicalFont.name);
        }
        else
        {
            Debug.LogWarning("[MagicalDebugUI] No magical font found. Using default font.");
        }
    }
    
    private Vector3 GetTargetPosition()
    {
        if (targetCamera == null) return Vector3.zero;
        
        // Calculate position relative to camera
        Vector3 cameraForward = targetCamera.transform.forward;
        Vector3 cameraRight = targetCamera.transform.right;
        Vector3 cameraUp = targetCamera.transform.up;
        
        return targetCamera.transform.position +
               cameraForward * offsetFromCamera.z +
               cameraRight * offsetFromCamera.x +
               cameraUp * offsetFromCamera.y;
    }
    
    private void FollowCamera()
    {
        Vector3 targetPosition = GetTargetPosition();
        
        // Smooth following
        if (followSpeed > 0f)
        {
            debugCanvas.transform.position = Vector3.Lerp(
                debugCanvas.transform.position, 
                targetPosition, 
                Time.deltaTime * followSpeed
            );
            
            if (lookAtCamera)
            {
                // Make canvas face the camera (look away from camera position)
                Vector3 lookDirection = debugCanvas.transform.position - targetCamera.transform.position;
                debugCanvas.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
        else
        {
            // Instant following
        debugCanvas.transform.position = targetPosition;
            
            if (lookAtCamera)
            {
                // Make canvas face the camera (look away from camera position)
                Vector3 lookDirection = debugCanvas.transform.position - targetCamera.transform.position;
                debugCanvas.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }
    
    public void UpdateSpellText(string spellName)
    {
        if (spellText != null)
        {
            spellText.text = $"Casted Spell: {spellName}";
            
            // Stop any existing animation before starting a new one
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                currentAnimation = null;
            }
            
            // Reset to original color first
            spellText.color = textColor;
            
            // Start new animation
            currentAnimation = StartCoroutine(AnimateSpellText());
        }
        
        Debug.Log($"[MagicalDebugUI] Updated spell text to: {spellName}");
    }
    
    private System.Collections.IEnumerator AnimateSpellText()
    {
        if (spellText == null) yield break;
        
        Color originalColor = textColor; // Use the defined textColor instead of current color
        Color magicalColor = new Color(0.5f, 0.9f, 1f, 1f); // Magical blue
        
        // Animate to magical color
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            spellText.color = Color.Lerp(originalColor, magicalColor, t);
            yield return null;
        }
        
        // Ensure we're fully at the magical color
        spellText.color = magicalColor;
        
        // Wait a moment
        yield return new WaitForSeconds(1f);
        
        // Animate back to original color
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            spellText.color = Color.Lerp(magicalColor, originalColor, t);
            yield return null;
        }
        
        // Ensure we're fully back to original color
        spellText.color = originalColor;
        
        // Clear the coroutine reference
        currentAnimation = null;
        
        Debug.Log("[MagicalDebugUI] Animation completed, returned to original color");
    }
    
    // Public methods for runtime adjustments
    public void SetFollowSpeed(float speed)
    {
        followSpeed = Mathf.Max(0f, speed);
        Debug.Log($"[MagicalDebugUI] Set follow speed to: {followSpeed}");
    }
    
    public void SetOffsetFromCamera(Vector3 newOffset)
    {
        offsetFromCamera = newOffset;
        Debug.Log($"[MagicalDebugUI] Set camera offset to: {offsetFromCamera}");
    }
    
    public void SetCanvasScale(float scale)
    {
        canvasScale = Mathf.Max(0.001f, scale);
        if (debugCanvas != null)
        {
            debugCanvas.transform.localScale = Vector3.one * canvasScale;
        }
        Debug.Log($"[MagicalDebugUI] Set canvas scale to: {canvasScale}");
    }
    
    public void SetLookAtCamera(bool lookAt)
    {
        lookAtCamera = lookAt;
        Debug.Log($"[MagicalDebugUI] Look at camera: {lookAtCamera}");
    }
    
    // Public method to be called from SpellCastingManager
    public static void NotifySpellCast(string spellName)
    {
        MagicalDebugUI instance = FindObjectOfType<MagicalDebugUI>();
        if (instance != null)
        {
            instance.UpdateSpellText(spellName);
        }
        else
        {
            Debug.LogWarning("[MagicalDebugUI] No MagicalDebugUI instance found in scene!");
        }
    }
} 
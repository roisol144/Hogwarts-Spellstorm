using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DamageIndicator : MonoBehaviour
{
    [Header("Damage Indicator Settings")]
    [SerializeField] private Canvas indicatorCanvas;
    [SerializeField] private GameObject damageIndicatorPrefab;
    [SerializeField] private float indicatorDistance = 150f; // Distance from center of screen
    [SerializeField] private float indicatorFadeDuration = 2f;
    [SerializeField] private float indicatorIntensityDuration = 0.5f;
    
    [Header("Indicator Appearance")]
    [SerializeField] private Color indicatorColor = new Color(1f, 0f, 0f, 0.8f);
    [SerializeField] private float indicatorSize = 50f;
    [SerializeField] private Sprite indicatorSprite; // Assign arrow sprite if available
    
    [Header("Screen Flash")]
    [SerializeField] private Image screenFlash;
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private AnimationCurve flashCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
    
    // Private variables
    private Camera playerCamera;
    private List<GameObject> activeIndicators = new List<GameObject>();
    private Coroutine screenFlashCoroutine;
    
    // Singleton instance
    private static DamageIndicator instance;
    public static DamageIndicator Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DamageIndicator>();
                if (instance == null)
                {
                    GameObject go = new GameObject("DamageIndicator");
                    instance = go.AddComponent<DamageIndicator>();
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Find player camera
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        // Setup canvas
        SetupCanvas();
    }
    
    private void SetupCanvas()
    {
        if (indicatorCanvas == null)
        {
            // Create indicator canvas
            GameObject canvasObject = new GameObject("DamageIndicatorCanvas");
            indicatorCanvas = canvasObject.AddComponent<Canvas>();
            indicatorCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            indicatorCanvas.sortingOrder = 5; // Below health UI but above game objects
            
            // Add Canvas Scaler
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add GraphicRaycaster
            canvasObject.AddComponent<GraphicRaycaster>();
            
            // Create screen flash overlay
            CreateScreenFlash();
        }
        
        // Create damage indicator prefab if none assigned
        if (damageIndicatorPrefab == null)
        {
            CreateDamageIndicatorPrefab();
        }
    }
    
    private void CreateScreenFlash()
    {
        GameObject flashObject = new GameObject("ScreenFlash");
        flashObject.transform.SetParent(indicatorCanvas.transform, false);
        
        screenFlash = flashObject.AddComponent<Image>();
        screenFlash.color = Color.clear;
        screenFlash.raycastTarget = false;
        
        // Make it cover the entire screen
        RectTransform rect = screenFlash.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
    
    private void CreateDamageIndicatorPrefab()
    {
        GameObject indicatorObject = new GameObject("DamageIndicatorPrefab");
        
        // Add Image component
        Image indicatorImage = indicatorObject.AddComponent<Image>();
        
        // Use sprite if available, otherwise create a simple arrow
        if (indicatorSprite != null)
        {
            indicatorImage.sprite = indicatorSprite;
        }
        else
        {
            // Create a simple triangle sprite
            indicatorImage.sprite = CreateArrowSprite();
        }
        
        indicatorImage.color = indicatorColor;
        indicatorImage.raycastTarget = false;
        
        // Set size
        RectTransform rect = indicatorImage.rectTransform;
        rect.sizeDelta = Vector2.one * indicatorSize;
        
        // Add CanvasGroup for fading
        indicatorObject.AddComponent<CanvasGroup>();
        
        damageIndicatorPrefab = indicatorObject;
    }
    
    private Sprite CreateArrowSprite()
    {
        // Create a simple arrow texture
        Texture2D arrowTexture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[32 * 32];
        
        // Fill with transparent
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        // Draw a simple arrow pointing up
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                // Create arrow shape
                if (IsInsideArrow(x, y, 32))
                {
                    pixels[y * 32 + x] = Color.white;
                }
            }
        }
        
        arrowTexture.SetPixels(pixels);
        arrowTexture.Apply();
        
        return Sprite.Create(arrowTexture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f);
    }
    
    private bool IsInsideArrow(int x, int y, int size)
    {
        int centerX = size / 2;
        int centerY = size / 2;
        
        // Arrow pointing up
        if (y > centerY + 4)
        {
            // Arrow head
            int distFromCenter = Mathf.Abs(x - centerX);
            int maxWidth = (size - y) / 2;
            return distFromCenter <= maxWidth;
        }
        else if (y >= centerY - 8 && y <= centerY + 4)
        {
            // Arrow shaft
            return Mathf.Abs(x - centerX) <= 2;
        }
        
        return false;
    }
    
    /// <summary>
    /// Shows a damage indicator pointing towards the damage source
    /// </summary>
    /// <param name="damageSourcePosition">World position of the damage source</param>
    /// <param name="damageAmount">Amount of damage (affects indicator intensity)</param>
    public void ShowDamageIndicator(Vector3 damageSourcePosition, float damageAmount = 0f)
    {
        if (playerCamera == null || indicatorCanvas == null) return;
        
        // Trigger screen flash
        TriggerScreenFlash();
        
        // Calculate direction from player to damage source
        Vector3 playerPosition = playerCamera.transform.position;
        Vector3 damageDirection = (damageSourcePosition - playerPosition).normalized;
        
        // Convert 3D direction to screen direction
        Vector2 screenDirection = GetScreenDirection(damageDirection);
        
        // Create indicator
        CreateIndicator(screenDirection, damageAmount);
    }
    
    /// <summary>
    /// Shows a damage indicator in a specific screen direction
    /// </summary>
    /// <param name="screenDirection">Direction on screen (normalized)</param>
    /// <param name="damageAmount">Amount of damage (affects indicator intensity)</param>
    public void ShowDamageIndicator(Vector2 screenDirection, float damageAmount = 0f)
    {
        if (indicatorCanvas == null) return;
        
        // Trigger screen flash
        TriggerScreenFlash();
        
        // Create indicator
        CreateIndicator(screenDirection.normalized, damageAmount);
    }
    
    private Vector2 GetScreenDirection(Vector3 worldDirection)
    {
        // Project world direction to camera's local space
        Vector3 localDirection = playerCamera.transform.InverseTransformDirection(worldDirection);
        
        // Convert to 2D screen direction (ignore Z, use X and Y)
        Vector2 screenDirection = new Vector2(localDirection.x, localDirection.y);
        
        return screenDirection.normalized;
    }
    
    private void CreateIndicator(Vector2 direction, float damageAmount)
    {
        if (damageIndicatorPrefab == null) return;
        
        // Instantiate indicator
        GameObject indicator = Instantiate(damageIndicatorPrefab, indicatorCanvas.transform);
        activeIndicators.Add(indicator);
        
        // Position indicator
        RectTransform indicatorRect = indicator.GetComponent<RectTransform>();
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 indicatorPosition = screenCenter + direction * indicatorDistance;
        
        // Convert screen position to canvas position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            indicatorCanvas.transform as RectTransform,
            indicatorPosition,
            null,
            out Vector2 localPosition
        );
        
        indicatorRect.localPosition = localPosition;
        
        // Rotate indicator to point towards center
        float angle = Mathf.Atan2(-direction.y, -direction.x) * Mathf.Rad2Deg + 90f;
        indicatorRect.rotation = Quaternion.Euler(0, 0, angle);
        
        // Scale based on damage amount
        float damageScale = Mathf.Clamp(1f + (damageAmount / 50f) * 0.5f, 1f, 2f);
        indicatorRect.localScale = Vector3.one * damageScale;
        
        // Start fade animation
        StartCoroutine(FadeIndicator(indicator));
    }
    
    private IEnumerator FadeIndicator(GameObject indicator)
    {
        if (indicator == null) yield break;
        
        CanvasGroup canvasGroup = indicator.GetComponent<CanvasGroup>();
        Image indicatorImage = indicator.GetComponent<Image>();
        
        if (canvasGroup == null || indicatorImage == null) yield break;
        
        // Initial intensity phase
        float elapsedTime = 0f;
        Color originalColor = indicatorImage.color;
        
        while (elapsedTime < indicatorIntensityDuration)
        {
            if (indicator == null) yield break;
            
            float intensity = Mathf.Sin((elapsedTime / indicatorIntensityDuration) * Mathf.PI);
            Color currentColor = originalColor;
            currentColor.a = originalColor.a * (0.5f + intensity * 0.5f);
            indicatorImage.color = currentColor;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Fade out phase
        elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsedTime < indicatorFadeDuration)
        {
            if (indicator == null) yield break;
            
            float t = elapsedTime / indicatorFadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Cleanup
        if (indicator != null)
        {
            activeIndicators.Remove(indicator);
            Destroy(indicator);
        }
    }
    
    private void TriggerScreenFlash()
    {
        if (screenFlashCoroutine != null)
        {
            StopCoroutine(screenFlashCoroutine);
        }
        screenFlashCoroutine = StartCoroutine(ScreenFlashCoroutine());
    }
    
    private IEnumerator ScreenFlashCoroutine()
    {
        if (screenFlash == null) yield break;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < flashDuration)
        {
            float t = elapsedTime / flashDuration;
            float intensity = flashCurve.Evaluate(t);
            
            Color currentColor = flashColor;
            currentColor.a = flashColor.a * intensity;
            screenFlash.color = currentColor;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        screenFlash.color = Color.clear;
        screenFlashCoroutine = null;
    }
    
    /// <summary>
    /// Clears all active damage indicators
    /// </summary>
    public void ClearAllIndicators()
    {
        foreach (GameObject indicator in activeIndicators)
        {
            if (indicator != null)
            {
                Destroy(indicator);
            }
        }
        activeIndicators.Clear();
    }
    
    /// <summary>
    /// Sets the indicator color
    /// </summary>
    /// <param name="color">New indicator color</param>
    public void SetIndicatorColor(Color color)
    {
        indicatorColor = color;
        
        if (damageIndicatorPrefab != null)
        {
            Image indicatorImage = damageIndicatorPrefab.GetComponent<Image>();
            if (indicatorImage != null)
            {
                indicatorImage.color = indicatorColor;
            }
        }
    }
    
    /// <summary>
    /// Sets the screen flash color
    /// </summary>
    /// <param name="color">New flash color</param>
    public void SetFlashColor(Color color)
    {
        flashColor = color;
    }
} 
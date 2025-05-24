using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    [Header("Health Bar Components")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;

    [Header("Health Bar Settings")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color halfHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float hideDelay = 2f; // Time before health bar hides automatically

    private Coroutine hideCoroutine;
    private bool isVisible = false;

    private void Awake()
    {
        // Set up components immediately when instantiated
        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>();
        
        // Try multiple ways to find the fill image
        if (fillImage == null && healthSlider != null)
        {
            // Method 1: Use the slider's fillRect reference
            if (healthSlider.fillRect != null)
            {
                fillImage = healthSlider.fillRect.GetComponent<Image>();
                Debug.Log($"[HealthBar] Found fill image via fillRect: {fillImage != null}");
            }
            
            // Method 2: Search by name if fillRect method failed
            if (fillImage == null)
            {
                Transform fill = healthSlider.transform.Find("Fill");
                if (fill != null)
                {
                    fillImage = fill.GetComponent<Image>();
                    Debug.Log($"[HealthBar] Found fill image by name: {fillImage != null}");
                }
            }
            
            // Method 3: Search in all children
            if (fillImage == null)
            {
                Image[] allImages = GetComponentsInChildren<Image>();
                foreach (Image img in allImages)
                {
                    if (img.gameObject.name.Contains("Fill"))
                    {
                        fillImage = img;
                        Debug.Log($"[HealthBar] Found fill image in children: {fillImage.name}");
                        break;
                    }
                }
            }
        }

        // Debug the components
        Debug.Log($"[HealthBar] Awake - healthSlider: {healthSlider != null}, fillImage: {fillImage != null}");
        if (fillImage != null)
        {
            Debug.Log($"[HealthBar] Fill image found: {fillImage.name}");
        }
        else
        {
            Debug.LogError("[HealthBar] Fill image NOT found! Color changes will not work.");
        }
    }

    private void Start()
    {
        // Don't automatically hide - let the Enemy control visibility
        // The health bar should start visible when created and hide via timer
        Debug.Log($"[HealthBar] Start complete - ready for use");
    }

    public void UpdateHealthBar(float healthPercentage)
    {
        // Clamp health percentage between 0 and 1
        healthPercentage = Mathf.Clamp01(healthPercentage);

        Debug.Log($"[HealthBar] UpdateHealthBar called with {healthPercentage:F2} ({healthPercentage * 100:F1}%)");

        // Show health bar when updating
        ShowHealthBar();

        // Update slider value
        if (healthSlider != null)
        {
            healthSlider.value = healthPercentage;
            Debug.Log($"[HealthBar] Slider value set to: {healthSlider.value:F2}");
        }
        else
        {
            Debug.LogError("[HealthBar] HealthSlider is null!");
        }

        // Update color based on health percentage
        if (fillImage != null)
        {
            Color newColor;
            if (healthPercentage > 0.8f)
            {
                // High health: interpolate between yellow and green
                float t = (healthPercentage - 0.8f) / 0.2f;
                newColor = Color.Lerp(halfHealthColor, fullHealthColor, t);
                Debug.Log($"[HealthBar] High health ({healthPercentage:F2}) - lerp factor: {t:F2}");
            }
            else if (healthPercentage > 0.4f)
            {
                // Medium health: interpolate between red and yellow
                float t = (healthPercentage - 0.4f) / 0.4f;
                newColor = Color.Lerp(lowHealthColor, halfHealthColor, t);
                Debug.Log($"[HealthBar] Medium health ({healthPercentage:F2}) - lerp factor: {t:F2}");
            }
            else
            {
                // Low health: pure red
                newColor = lowHealthColor;
                Debug.Log($"[HealthBar] Low health ({healthPercentage:F2}) - using pure red");
            }
            
            fillImage.color = newColor;
            Debug.Log($"[HealthBar] Color set to: R:{newColor.r:F2} G:{newColor.g:F2} B:{newColor.b:F2}");
        }
        else
        {
            Debug.LogError("[HealthBar] FillImage is null! Cannot update color.");
        }

        // Start or restart the hide timer using Coroutine
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private void ShowHealthBar()
    {
        if (!isVisible)
        {
            gameObject.SetActive(true);
            isVisible = true;
            Debug.Log($"[HealthBar] Health bar shown");
        }
    }

    private void HideHealthBar()
    {
        if (isVisible)
        {
            gameObject.SetActive(false);
            isVisible = false;
            Debug.Log($"[HealthBar] Health bar hidden");
        }
    }

    private IEnumerator HideAfterDelay()
    {
        Debug.Log($"[HealthBar] Starting {hideDelay}s hide timer");
        yield return new WaitForSeconds(hideDelay);
        Debug.Log($"[HealthBar] Timer expired - hiding health bar");
        HideHealthBar();
    }

    // Force show the health bar (called when taking damage)
    public void ForceShow()
    {
        ShowHealthBar();
    }

    // Force hide the health bar (called when enemy dies)
    public void ForceHide()
    {
        HideHealthBar();
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }
} 
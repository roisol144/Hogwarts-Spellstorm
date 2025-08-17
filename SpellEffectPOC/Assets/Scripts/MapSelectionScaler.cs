using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapSelectionScaler : MonoBehaviour
{
    [Header("Scaling Settings")]
    [SerializeField] private float buttonScaleMultiplier = 1.5f;
    [SerializeField] private float textScaleMultiplier = 1.3f;
    [SerializeField] private float spacingMultiplier = 1.2f;
    
    [Header("Target Components")]
    [SerializeField] private Button dungeonsMapButton;
    [SerializeField] private Button chamberMapButton;
    [SerializeField] private Button mapBackButton;
    
    private void Start()
    {
        ScaleMapSelectionMenu();
    }
    
    private void ScaleMapSelectionMenu()
    {
        Debug.Log("🎯 Scaling map selection menu...");
        
        // Scale the buttons
        ScaleButton(dungeonsMapButton, "Dungeons");
        ScaleButton(chamberMapButton, "Chamber of Secrets");
        ScaleButton(mapBackButton, "Back");
        
        Debug.Log("✅ Map selection menu scaled successfully!");
    }
    
    private void ScaleButton(Button button, string buttonName)
    {
        if (button == null)
        {
            Debug.LogWarning($"Button {buttonName} is null!");
            return;
        }
        
        // Get the RectTransform of the button
        RectTransform buttonRect = button.GetComponent<RectTransform>();
        if (buttonRect == null)
        {
            Debug.LogError($"RectTransform not found on {buttonName} button!");
            return;
        }
        
        // Scale the button size
        Vector2 currentSize = buttonRect.sizeDelta;
        Vector2 newSize = new Vector2(
            currentSize.x * buttonScaleMultiplier,
            currentSize.y * buttonScaleMultiplier
        );
        buttonRect.sizeDelta = newSize;
        
        // Find and scale the text
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            // Scale the font size
            float currentFontSize = buttonText.fontSize;
            float newFontSize = currentFontSize * textScaleMultiplier;
            buttonText.fontSize = newFontSize;
            
            // Ensure text stays on one line
            buttonText.enableWordWrapping = false;
            buttonText.overflowMode = TextOverflowModes.Overflow;
            
            Debug.Log($"🎨 Scaled {buttonName}: Button {currentSize} → {newSize}, Font {currentFontSize} → {newFontSize}");
        }
        else
        {
            Debug.LogWarning($"Text component not found on {buttonName} button!");
        }
    }
    
    // Public method to adjust spacing between buttons
    public void AdjustButtonSpacing()
    {
        if (dungeonsMapButton != null && chamberMapButton != null)
        {
            RectTransform dungeonsRect = dungeonsMapButton.GetComponent<RectTransform>();
            RectTransform chamberRect = chamberMapButton.GetComponent<RectTransform>();
            
            if (dungeonsRect != null && chamberRect != null)
            {
                // Get current positions
                Vector2 dungeonsPos = dungeonsRect.anchoredPosition;
                Vector2 chamberPos = chamberRect.anchoredPosition;
                
                // Calculate new spacing
                float currentSpacing = Mathf.Abs(dungeonsPos.y - chamberPos.y);
                float newSpacing = currentSpacing * spacingMultiplier;
                
                // Adjust positions to maintain center alignment
                float centerY = (dungeonsPos.y + chamberPos.y) / 2f;
                dungeonsRect.anchoredPosition = new Vector2(dungeonsPos.x, centerY + newSpacing / 2f);
                chamberRect.anchoredPosition = new Vector2(chamberPos.x, centerY - newSpacing / 2f);
                
                Debug.Log($"📏 Adjusted button spacing: {currentSpacing} → {newSpacing}");
            }
        }
    }
    
    // Public method to refresh scaling (useful for runtime adjustments)
    public void RefreshScaling()
    {
        ScaleMapSelectionMenu();
        AdjustButtonSpacing();
    }
    
    // Public methods to adjust scaling at runtime
    public void SetButtonScale(float scale)
    {
        buttonScaleMultiplier = scale;
        RefreshScaling();
    }
    
    public void SetTextScale(float scale)
    {
        textScaleMultiplier = scale;
        RefreshScaling();
    }
    
    public void SetSpacingScale(float scale)
    {
        spacingMultiplier = scale;
        AdjustButtonSpacing();
    }
}

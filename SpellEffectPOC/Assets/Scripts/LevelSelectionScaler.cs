using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSelectionScaler : MonoBehaviour
{
    [Header("Scaling Settings")]
    [SerializeField] private float buttonScaleMultiplier = 1.5f;
    [SerializeField] private float textScaleMultiplier = 1.3f;
    [SerializeField] private float spacingMultiplier = 1.2f;
    
    [Header("Target Components")]
    [SerializeField] private Button beginnerButton;
    [SerializeField] private Button intermediateButton;
    [SerializeField] private Button advancedButton;
    [SerializeField] private Button difficultyBackButton;
    
    private void Start()
    {
        ScaleLevelSelectionMenu();
    }
    
    private void ScaleLevelSelectionMenu()
    {
        Debug.Log("🎯 Scaling level selection menu...");
        
        // Scale the difficulty buttons
        ScaleButton(beginnerButton, "Beginner");
        ScaleButton(intermediateButton, "Intermediate");
        ScaleButton(advancedButton, "Advanced");
        ScaleButton(difficultyBackButton, "Back");
        
        Debug.Log("✅ Level selection menu scaled successfully!");
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
    
    // Public method to adjust spacing between difficulty buttons
    public void AdjustButtonSpacing()
    {
        if (beginnerButton != null && intermediateButton != null && advancedButton != null)
        {
            RectTransform beginnerRect = beginnerButton.GetComponent<RectTransform>();
            RectTransform intermediateRect = intermediateButton.GetComponent<RectTransform>();
            RectTransform advancedRect = advancedButton.GetComponent<RectTransform>();
            
            if (beginnerRect != null && intermediateRect != null && advancedRect != null)
            {
                // Get current positions (they should be stacked vertically)
                Vector2 beginnerPos = beginnerRect.anchoredPosition;
                Vector2 intermediatePos = intermediateRect.anchoredPosition;
                Vector2 advancedPos = advancedRect.anchoredPosition;
                
                // Calculate current spacing between buttons
                float spacing1 = Mathf.Abs(beginnerPos.y - intermediatePos.y);
                float spacing2 = Mathf.Abs(intermediatePos.y - advancedPos.y);
                float currentSpacing = Mathf.Max(spacing1, spacing2);
                
                // Calculate new spacing
                float newSpacing = currentSpacing * spacingMultiplier;
                
                // Adjust positions to maintain center alignment
                // Keep the middle button (intermediate) in the same position
                // Adjust beginner and advanced buttons accordingly
                beginnerRect.anchoredPosition = new Vector2(beginnerPos.x, intermediatePos.y + newSpacing);
                advancedRect.anchoredPosition = new Vector2(advancedPos.x, intermediatePos.y - newSpacing);
                
                Debug.Log($"📏 Adjusted difficulty button spacing: {currentSpacing} → {newSpacing}");
            }
        }
    }
    
    // Public method to refresh scaling (useful for runtime adjustments)
    public void RefreshScaling()
    {
        ScaleLevelSelectionMenu();
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
    
    // Method to get current scaling values (useful for UI sliders)
    public float GetButtonScale() => buttonScaleMultiplier;
    public float GetTextScale() => textScaleMultiplier;
    public float GetSpacingScale() => spacingMultiplier;
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerNamePanelScaler : MonoBehaviour
{
    [Header("Scaling Settings")]
    [SerializeField] private float buttonScaleMultiplier = 1.5f;
    [SerializeField] private float textScaleMultiplier = 1.3f;
    [SerializeField] private float inputFieldScaleMultiplier = 1.4f;
    [SerializeField] private float spacingMultiplier = 1.2f;
    
    [Header("Target Components")]
    [SerializeField] private Button saveAndPlayButton;
    [SerializeField] private Button nameBackButton;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI instructionText;
    
    private void Start()
    {
        ScalePlayerNamePanel();
    }
    
    private void ScalePlayerNamePanel()
    {
        Debug.Log("🎯 Scaling player name panel...");
        
        // Scale the buttons
        ScaleButton(saveAndPlayButton, "Save & Play");
        ScaleButton(nameBackButton, "Back");
        
        // Scale the input field
        ScaleInputField(playerNameInput, "Player Name Input");
        
        // Scale text elements
        ScaleText(titleText, "Title Text");
        ScaleText(instructionText, "Instruction Text");
        
        // Adjust spacing
        AdjustElementSpacing();
        
        Debug.Log("✅ Player name panel scaled successfully!");
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
    
    private void ScaleInputField(TMP_InputField inputField, string fieldName)
    {
        if (inputField == null)
        {
            Debug.LogWarning($"Input field {fieldName} is null!");
            return;
        }
        
        // Get the RectTransform of the input field
        RectTransform inputRect = inputField.GetComponent<RectTransform>();
        if (inputRect == null)
        {
            Debug.LogError($"RectTransform not found on {fieldName}!");
            return;
        }
        
        // Scale the input field size
        Vector2 currentSize = inputRect.sizeDelta;
        Vector2 newSize = new Vector2(
            currentSize.x * inputFieldScaleMultiplier,
            currentSize.y * inputFieldScaleMultiplier
        );
        inputRect.sizeDelta = newSize;
        
        // Scale the text component
        if (inputField.textComponent != null)
        {
            float currentFontSize = inputField.textComponent.fontSize;
            float newFontSize = currentFontSize * textScaleMultiplier;
            inputField.textComponent.fontSize = newFontSize;
            
            // Ensure text stays on one line
            inputField.textComponent.enableWordWrapping = false;
            inputField.textComponent.overflowMode = TextOverflowModes.Overflow;
            
            Debug.Log($"🎨 Scaled {fieldName}: Input {currentSize} → {newSize}, Font {currentFontSize} → {newFontSize}");
        }
        
        // Scale the placeholder text if it exists
        if (inputField.placeholder != null)
        {
            TextMeshProUGUI placeholderText = inputField.placeholder as TextMeshProUGUI;
            if (placeholderText != null)
            {
                float currentPlaceholderSize = placeholderText.fontSize;
                float newPlaceholderSize = currentPlaceholderSize * textScaleMultiplier;
                placeholderText.fontSize = newPlaceholderSize;
                
                Debug.Log($"🎨 Scaled {fieldName} placeholder: Font {currentPlaceholderSize} → {newPlaceholderSize}");
            }
        }
    }
    
    private void ScaleText(TextMeshProUGUI textComponent, string textName)
    {
        if (textComponent == null)
        {
            Debug.LogWarning($"Text component {textName} is null!");
            return;
        }
        
        // Scale the font size
        float currentFontSize = textComponent.fontSize;
        float newFontSize = currentFontSize * textScaleMultiplier;
        textComponent.fontSize = newFontSize;
        
        // Ensure text stays on one line (for titles and instructions)
        textComponent.enableWordWrapping = false;
        textComponent.overflowMode = TextOverflowModes.Overflow;
        
        Debug.Log($"🎨 Scaled {textName}: Font {currentFontSize} → {newFontSize}");
    }
    
    // Public method to adjust spacing between elements
    public void AdjustElementSpacing()
    {
        if (saveAndPlayButton != null && nameBackButton != null)
        {
            RectTransform saveRect = saveAndPlayButton.GetComponent<RectTransform>();
            RectTransform backRect = nameBackButton.GetComponent<RectTransform>();
            
            if (saveRect != null && backRect != null)
            {
                // Get current positions
                Vector2 savePos = saveRect.anchoredPosition;
                Vector2 backPos = backRect.anchoredPosition;
                
                // Calculate current spacing
                float currentSpacing = Mathf.Abs(savePos.x - backPos.x);
                float newSpacing = currentSpacing * spacingMultiplier;
                
                // Adjust positions to maintain center alignment
                float centerX = (savePos.x + backPos.x) / 2f;
                saveRect.anchoredPosition = new Vector2(centerX + newSpacing / 2f, savePos.y);
                backRect.anchoredPosition = new Vector2(centerX - newSpacing / 2f, backPos.y);
                
                Debug.Log($"📏 Adjusted button spacing: {currentSpacing} → {newSpacing}");
            }
        }
    }
    
    // Public method to refresh scaling (useful for runtime adjustments)
    public void RefreshScaling()
    {
        ScalePlayerNamePanel();
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
    
    public void SetInputFieldScale(float scale)
    {
        inputFieldScaleMultiplier = scale;
        RefreshScaling();
    }
    
    public void SetSpacingScale(float scale)
    {
        spacingMultiplier = scale;
        AdjustElementSpacing();
    }
    
    // Method to get current scaling values (useful for UI sliders)
    public float GetButtonScale() => buttonScaleMultiplier;
    public float GetTextScale() => textScaleMultiplier;
    public float GetInputFieldScale() => inputFieldScaleMultiplier;
    public float GetSpacingScale() => spacingMultiplier;
}

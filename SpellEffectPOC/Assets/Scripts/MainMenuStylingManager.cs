using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuStylingManager : MonoBehaviour
{
    [Header("Magical Button Materials")]
    [SerializeField] private Material normalButtonMaterial;
    [SerializeField] private Material hoverButtonMaterial;
    [SerializeField] private Material pressedButtonMaterial;
    
    [Header("Magical Fonts")]
    [SerializeField] private TMP_FontAsset primaryMagicalFont;
    [SerializeField] private TMP_FontAsset secondaryMagicalFont;
    
    [Header("Button Styling")]
    [SerializeField] private Color primaryTextColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color secondaryTextColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    [SerializeField] private Color accentTextColor = new Color(1f, 0.9f, 0.7f, 1f);
    
    [Header("Menu Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Material backgroundMaterial;
    
    private void Start()
    {
        ApplyMagicalStyling();
    }
    
    private void ApplyMagicalStyling()
    {
        // Find all buttons in the menu
        Button[] allButtons = FindObjectsOfType<Button>();
        
        foreach (Button button in allButtons)
        {
            ApplyButtonStyling(button);
        }
        
        // Apply background styling
        if (backgroundImage != null && backgroundMaterial != null)
        {
            backgroundImage.material = backgroundMaterial;
        }
    }
    
    private void ApplyButtonStyling(Button button)
    {
        // Add the magical button styler component if it doesn't exist
        MagicalButtonStyler styler = button.GetComponent<MagicalButtonStyler>();
        if (styler == null)
        {
            styler = button.gameObject.AddComponent<MagicalButtonStyler>();
        }
        
        // Set materials
        if (normalButtonMaterial != null && hoverButtonMaterial != null && pressedButtonMaterial != null)
        {
            styler.SetMaterials(normalButtonMaterial, hoverButtonMaterial, pressedButtonMaterial);
        }
        
        // Set font based on button importance
        TMP_FontAsset fontToUse = DetermineButtonFont(button);
        if (fontToUse != null)
        {
            styler.SetFont(fontToUse);
        }
        
        // Apply text styling
        ApplyTextStyling(button);
        
        // Add magical effects
        AddMagicalEffects(button);
    }
    
    private TMP_FontAsset DetermineButtonFont(Button button)
    {
        string buttonName = button.name.ToLower();
        
        // Primary buttons get the main magical font
        if (buttonName.Contains("start") || buttonName.Contains("play") || buttonName.Contains("new"))
        {
            return primaryMagicalFont;
        }
        
        // Secondary buttons get the secondary font
        if (buttonName.Contains("tutorial") || buttonName.Contains("scoreboard") || buttonName.Contains("exit"))
        {
            return secondaryMagicalFont;
        }
        
        // Default to primary font
        return primaryMagicalFont;
    }
    
    private void ApplyTextStyling(Button button)
    {
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText == null) return;
        
        string buttonName = button.name.ToLower();
        
        // Set text color based on button importance
        if (buttonName.Contains("start") || buttonName.Contains("play") || buttonName.Contains("new"))
        {
            buttonText.color = primaryTextColor;
        }
        else if (buttonName.Contains("exit") || buttonName.Contains("quit"))
        {
            buttonText.color = accentTextColor;
        }
        else
        {
            buttonText.color = secondaryTextColor;
        }
        
        // Add text shadow for magical effect
        if (buttonText.GetComponent<Shadow>() == null)
        {
            Shadow shadow = buttonText.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.6f);
            shadow.effectDistance = new Vector2(2, -2);
        }
        
        // Add outline for extra magical effect
        if (buttonText.GetComponent<Outline>() == null)
        {
            Outline outline = buttonText.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.1f, 0.2f, 0.4f, 0.8f);
            outline.effectDistance = new Vector2(1, -1);
        }
    }
    
    private void AddMagicalEffects(Button button)
    {
        // Add a subtle glow effect
        if (button.GetComponent<Outline>() == null)
        {
            Outline buttonOutline = button.gameObject.AddComponent<Outline>();
            buttonOutline.effectColor = new Color(0.3f, 0.6f, 1f, 0.3f);
            buttonOutline.effectDistance = new Vector2(3, -3);
        }
        
        // Add a subtle shadow
        if (button.GetComponent<Shadow>() == null)
        {
            Shadow buttonShadow = button.gameObject.AddComponent<Shadow>();
            buttonShadow.effectColor = new Color(0, 0, 0, 0.4f);
            buttonShadow.effectDistance = new Vector2(4, -4);
        }
    }
    
    // Public method to refresh styling (useful for dynamic menus)
    public void RefreshStyling()
    {
        ApplyMagicalStyling();
    }
    
    // Public method to set materials at runtime
    public void SetButtonMaterials(Material normal, Material hover, Material pressed)
    {
        normalButtonMaterial = normal;
        hoverButtonMaterial = hover;
        pressedButtonMaterial = pressed;
        
        // Refresh all buttons
        RefreshStyling();
    }
    
    // Public method to set fonts at runtime
    public void SetFonts(TMP_FontAsset primary, TMP_FontAsset secondary)
    {
        primaryMagicalFont = primary;
        secondaryMagicalFont = secondary;
        
        // Refresh all buttons
        RefreshStyling();
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MagicalMenuStyler : MonoBehaviour
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
    
    [Header("Animation Settings")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private float glowPulseSpeed = 2f;
    
    [Header("Menu Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Material backgroundMaterial;
    
    private MainMenuManager menuManager;
    
    private void Start()
    {
        // Find the MainMenuManager
        menuManager = FindObjectOfType<MainMenuManager>();
        if (menuManager == null)
        {
            Debug.LogError("MagicalMenuStyler: Could not find MainMenuManager!");
            return;
        }
        
        // Apply styling after a short delay to ensure everything is initialized
        Invoke(nameof(ApplyMagicalStyling), 0.1f);
    }
    
    private void ApplyMagicalStyling()
    {
        Debug.Log("🎨 Applying magical styling to main menu...");
        
        // Style all buttons in the MainMenuManager
        StyleButton(menuManager.playButton, "play", true);
        StyleButton(menuManager.tutorialButton, "tutorial", false);
        StyleButton(menuManager.scoreboardButton, "scoreboard", false);
        StyleButton(menuManager.spellsBookButton, "spellsbook", false);
        StyleButton(menuManager.quitButton, "quit", false);
        
        // Style map selection buttons
        StyleButton(menuManager.dungeonsMapButton, "dungeons", false);
        StyleButton(menuManager.chamberMapButton, "chamber", false);
        StyleButton(menuManager.mapBackButton, "back", false);
        
        // Style difficulty buttons
        StyleButton(menuManager.beginnerButton, "beginner", false);
        StyleButton(menuManager.intermediateButton, "intermediate", false);
        StyleButton(menuManager.advancedButton, "advanced", false);
        StyleButton(menuManager.difficultyBackButton, "back", false);
        
        // Style player name buttons
        StyleButton(menuManager.saveAndPlayButton, "saveplay", true);
        StyleButton(menuManager.nameBackButton, "back", false);
        
        // Style popup buttons
        StyleButton(menuManager.closeScoreboardButton, "close", false);
        StyleButton(menuManager.closeSpellsBookButton, "close", false);
        
        // Apply background styling
        ApplyBackgroundStyling();
        
        Debug.Log("✨ Magical styling applied successfully!");
    }
    
    private void StyleButton(Button button, string buttonType, bool isPrimary)
    {
        if (button == null) return;
        
        // Add the magical button styler component
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
        TMP_FontAsset fontToUse = isPrimary ? primaryMagicalFont : secondaryMagicalFont;
        if (fontToUse != null)
        {
            styler.SetFont(fontToUse);
        }
        
        // Apply text styling
        ApplyTextStyling(button, buttonType, isPrimary);
        
        // Add magical effects
        AddMagicalEffects(button);
        
        Debug.Log($"🎨 Styled button: {button.name} ({buttonType})");
    }
    
    private void ApplyTextStyling(Button button, string buttonType, bool isPrimary)
    {
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText == null) return;
        
        // Set text color based on button importance
        if (isPrimary || buttonType.Contains("play") || buttonType.Contains("save"))
        {
            buttonText.color = primaryTextColor;
        }
        else if (buttonType.Contains("quit") || buttonType.Contains("close"))
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
    
    private void ApplyBackgroundStyling()
    {
        // Find the main menu background
        if (backgroundImage == null)
        {
            // Try to find the background automatically
            GameObject gameMenuUI = GameObject.Find("Game Menu UI");
            if (gameMenuUI != null)
            {
                Image[] images = gameMenuUI.GetComponentsInChildren<Image>();
                foreach (Image img in images)
                {
                    if (img.rectTransform.anchorMin == Vector2.zero && img.rectTransform.anchorMax == Vector2.one)
                    {
                        backgroundImage = img;
                        break;
                    }
                }
            }
        }
        
        if (backgroundImage != null && backgroundMaterial != null)
        {
            backgroundImage.material = backgroundMaterial;
            Debug.Log("🎨 Applied magical background material");
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

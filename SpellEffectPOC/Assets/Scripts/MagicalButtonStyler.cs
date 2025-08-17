using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class MagicalButtonStyler : MonoBehaviour
{
    [Header("Button Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private Material pressedMaterial;
    
    [Header("Text Styling")]
    [SerializeField] private TMP_FontAsset magicalFont;
    [SerializeField] private Color textColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color textHoverColor = new Color(1f, 0.9f, 0.7f, 1f);
    [SerializeField] private float textGlowIntensity = 1.2f;
    
    [Header("Animation Settings")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float animationDuration = 0.2f;
    [SerializeField] private float glowPulseSpeed = 2f;
    
    private Button button;
    private Image buttonImage;
    private TextMeshProUGUI buttonText;
    private Material originalMaterial;
    private Vector3 originalScale;
    private Color originalTextColor;
    private bool isHovering = false;
    private Sequence glowSequence;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        
        if (buttonImage != null)
        {
            originalMaterial = buttonImage.material;
        }
        
        if (buttonText != null)
        {
            originalTextColor = buttonText.color;
        }
        
        originalScale = transform.localScale;
    }
    
    private void Start()
    {
        SetupButtonStyling();
        SetupEventTriggers();
    }
    
    private void SetupButtonStyling()
    {
        // Apply magical font if available
        if (magicalFont != null && buttonText != null)
        {
            buttonText.font = magicalFont;
            buttonText.color = textColor;
            
            // Add text shadow for magical effect
            if (buttonText.GetComponent<Shadow>() == null)
            {
                Shadow shadow = buttonText.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0, 0, 0, 0.5f);
                shadow.effectDistance = new Vector2(2, -2);
            }
        }
        
        // Apply normal material
        if (normalMaterial != null && buttonImage != null)
        {
            buttonImage.material = normalMaterial;
        }
    }
    
    private void SetupEventTriggers()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = gameObject.AddComponent<EventTrigger>();
        }
        
        // Clear existing triggers
        trigger.triggers.Clear();
        
        // Pointer Enter
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => OnPointerEnter());
        trigger.triggers.Add(enterEntry);
        
        // Pointer Exit
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => OnPointerExit());
        trigger.triggers.Add(exitEntry);
        
        // Pointer Down
        EventTrigger.Entry downEntry = new EventTrigger.Entry();
        downEntry.eventID = EventTriggerType.PointerDown;
        downEntry.callback.AddListener((data) => OnPointerDown());
        trigger.triggers.Add(downEntry);
        
        // Pointer Up
        EventTrigger.Entry upEntry = new EventTrigger.Entry();
        upEntry.eventID = EventTriggerType.PointerUp;
        upEntry.callback.AddListener((data) => OnPointerUp());
        trigger.triggers.Add(upEntry);
    }
    
    private void OnPointerEnter()
    {
        isHovering = true;
        
        // Scale animation
        transform.DOScale(originalScale * hoverScale, animationDuration).SetEase(Ease.OutBack);
        
        // Material change
        if (hoverMaterial != null && buttonImage != null)
        {
            buttonImage.material = hoverMaterial;
        }
        
        // Text color change
        if (buttonText != null)
        {
            buttonText.DOColor(textHoverColor, animationDuration);
        }
        
        // Start glow effect
        StartGlowEffect();
    }
    
    private void OnPointerExit()
    {
        isHovering = false;
        
        // Scale animation
        transform.DOScale(originalScale, animationDuration).SetEase(Ease.OutBack);
        
        // Material change
        if (normalMaterial != null && buttonImage != null)
        {
            buttonImage.material = normalMaterial;
        }
        
        // Text color change
        if (buttonText != null)
        {
            buttonText.DOColor(textColor, animationDuration);
        }
        
        // Stop glow effect
        StopGlowEffect();
    }
    
    private void OnPointerDown()
    {
        // Scale animation
        transform.DOScale(originalScale * pressScale, animationDuration * 0.5f).SetEase(Ease.OutBack);
        
        // Material change
        if (pressedMaterial != null && buttonImage != null)
        {
            buttonImage.material = pressedMaterial;
        }
    }
    
    private void OnPointerUp()
    {
        // Scale animation
        float targetScale = isHovering ? hoverScale : 1f;
        transform.DOScale(originalScale * targetScale, animationDuration).SetEase(Ease.OutBack);
        
        // Material change
        if (isHovering && hoverMaterial != null)
        {
            buttonImage.material = hoverMaterial;
        }
        else if (normalMaterial != null)
        {
            buttonImage.material = normalMaterial;
        }
    }
    
    private void StartGlowEffect()
    {
        if (glowSequence != null)
        {
            glowSequence.Kill();
        }
        
        glowSequence = DOTween.Sequence();
        
        // Pulse the emission intensity
        if (buttonImage != null && buttonImage.material != null)
        {
            Color emissionColor = buttonImage.material.GetColor("_EmissionColor");
            glowSequence.Append(DOTween.To(() => emissionColor.a, x => {
                emissionColor.a = x;
                buttonImage.material.SetColor("_EmissionColor", emissionColor);
            }, emissionColor.a * textGlowIntensity, glowPulseSpeed / 2f).SetEase(Ease.InOutSine));
            
            glowSequence.Append(DOTween.To(() => emissionColor.a, x => {
                emissionColor.a = x;
                buttonImage.material.SetColor("_EmissionColor", emissionColor);
            }, emissionColor.a / textGlowIntensity, glowPulseSpeed / 2f).SetEase(Ease.InOutSine));
            
            glowSequence.SetLoops(-1, LoopType.Yoyo);
        }
    }
    
    private void StopGlowEffect()
    {
        if (glowSequence != null)
        {
            glowSequence.Kill();
        }
        
        // Reset emission to normal
        if (buttonImage != null && buttonImage.material != null)
        {
            Color emissionColor = buttonImage.material.GetColor("_EmissionColor");
            emissionColor.a = normalMaterial != null ? normalMaterial.GetColor("_EmissionColor").a : 0.5f;
            buttonImage.material.SetColor("_EmissionColor", emissionColor);
        }
    }
    
    private void OnDestroy()
    {
        if (glowSequence != null)
        {
            glowSequence.Kill();
        }
    }
    
    // Public method to set materials at runtime
    public void SetMaterials(Material normal, Material hover, Material pressed)
    {
        normalMaterial = normal;
        hoverMaterial = hover;
        pressedMaterial = pressed;
        
        if (buttonImage != null && normalMaterial != null)
        {
            buttonImage.material = normalMaterial;
        }
    }
    
    // Public method to set font at runtime
    public void SetFont(TMP_FontAsset font)
    {
        magicalFont = font;
        if (buttonText != null && magicalFont != null)
        {
            buttonText.font = magicalFont;
        }
    }
}

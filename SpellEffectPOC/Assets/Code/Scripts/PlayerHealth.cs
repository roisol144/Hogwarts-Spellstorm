using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float healthRegenRate = 5f; // Health per second
    [SerializeField] private float healthRegenDelay = 3f; // Delay before regen starts
    
    [Header("Health UI")]
    [SerializeField] private Canvas healthCanvas; // Will be created if null
    [SerializeField] private Slider healthBarSlider;
    [SerializeField] private Image healthBarFill; // This will be the slider's fill area
    [SerializeField] private Image damageOverlay;
    [SerializeField] private Text healthText;
    
    [Header("VR UI Positioning")]
    [SerializeField] private Vector3 offsetFromCamera = new Vector3(0f, -0.4f, 0.8f); // Position relative to camera (center, below spell debug, forward)
    [SerializeField] private float followSpeed = 5f; // How fast to follow (0 = instant, higher = smoother)
    [SerializeField] private bool lookAtCamera = true; // Whether to face the camera
    [SerializeField] private float canvasScale = 0.003f; // Scale of the world space canvas (same as spell debug)
    [SerializeField] private Vector2 canvasSize = new Vector2(400f, 30f); // Canvas size in pixels (adjustable in editor)
    
    [Header("Damage Effects")]
    [SerializeField] private Color damageOverlayColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private float damageFlashDuration = 0.2f;
    [SerializeField] private AnimationCurve damageFlashCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
    
    [Header("Low Health Warning")]
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float heartbeatPulseSpeed = 2f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip lowHealthHeartbeat;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioSource audioSource;
    
    // Private variables
    private bool isDead = false;
    private bool isRegenerating = false;
    private Coroutine regenCoroutine;
    private Coroutine damageEffectCoroutine;
    private Coroutine lowHealthEffectCoroutine;
    private Camera playerCamera;
    
    // Events
    public System.Action<float> OnHealthChanged; // float = health percentage (0-1)
    public System.Action OnPlayerDied;
    public System.Action OnPlayerRevived;
    
    private void Awake()
    {
        // Initialize health
        currentHealth = maxHealth;
        
        // Find player camera
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Setup UI
        SetupHealthUI();
    }
    
    private void Start()
    {
        UpdateHealthUI();
        OnHealthChanged?.Invoke(GetHealthPercentage());
    }
    
    private void LateUpdate()
    {
        // Follow the camera smoothly for VR UI
        if (playerCamera != null && healthCanvas != null)
        {
            FollowCamera();
        }
    }
    
    private void SetupHealthUI()
    {
        if (healthCanvas == null)
        {
            // Create health canvas for VR (World Space)
            GameObject canvasObject = new GameObject("PlayerHealthCanvas_WorldSpace");
            healthCanvas = canvasObject.AddComponent<Canvas>();
            
            // Configure as World Space Canvas (like MagicalDebugUI)
            healthCanvas.renderMode = RenderMode.WorldSpace;
            healthCanvas.sortingOrder = 100;
            
            // Set initial position near camera
            if (playerCamera != null)
            {
                canvasObject.transform.position = GetTargetPosition();
                if (lookAtCamera)
                {
                    canvasObject.transform.LookAt(playerCamera.transform);
                }
            }
            
            // Set canvas scale
            canvasObject.transform.localScale = Vector3.one * canvasScale;
            
            // Set canvas size
            var rectTransform = canvasObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = canvasSize;
            
            // Add CanvasScaler for consistent scaling
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;
            
            // Add GraphicRaycaster
            canvasObject.AddComponent<GraphicRaycaster>();
            
            // Create health bar (no damage overlay for now)
            CreateHealthBar();
            
            Debug.Log($"[PlayerHealth] Created World Space Canvas at position: {canvasObject.transform.position}");
        }
    }
    

    
    private void CreateHealthBar()
    {
        // Health bar container
        GameObject healthBarContainer = new GameObject("HealthBarContainer");
        healthBarContainer.transform.SetParent(healthCanvas.transform, false);
        
        RectTransform containerRect = healthBarContainer.AddComponent<RectTransform>();
        // For WorldSpace canvas, use center anchoring and explicit size
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(380f, 25f); // Health bar (slightly smaller than canvas)
        
        // Create slider
        healthBarSlider = healthBarContainer.AddComponent<Slider>();
        healthBarSlider.minValue = 0f;
        healthBarSlider.maxValue = 1f;
        healthBarSlider.value = 1f;
        healthBarSlider.interactable = false; // Player can't interact with it
        
        // Background
        GameObject bgObject = new GameObject("Background");
        bgObject.transform.SetParent(healthBarContainer.transform, false);
        Image bgImage = bgObject.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.7f);
        
        RectTransform bgRect = bgImage.rectTransform;
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Assign background to slider
        healthBarSlider.targetGraphic = bgImage;
        
        // Fill area
        GameObject fillAreaObject = new GameObject("Fill Area");
        fillAreaObject.transform.SetParent(healthBarContainer.transform, false);
        RectTransform fillAreaRect = fillAreaObject.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(5, 5);
        fillAreaRect.offsetMax = new Vector2(-5, -5);
        
        // Health fill
        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(fillAreaObject.transform, false);
        healthBarFill = fillObject.AddComponent<Image>();
        healthBarFill.color = Color.green;
        
        RectTransform fillRect = healthBarFill.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        // Assign fill to slider
        healthBarSlider.fillRect = fillRect;
        
        // Health text
        GameObject textObject = new GameObject("HealthText");
        textObject.transform.SetParent(healthBarContainer.transform, false);
        healthText = textObject.AddComponent<Text>();
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        healthText.fontSize = 12; // Smaller font for thin bar
        healthText.color = Color.white;
        healthText.alignment = TextAnchor.MiddleCenter;
        healthText.fontStyle = FontStyle.Bold;
        
        RectTransform textRect = healthText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Debug.Log("[PlayerHealth] Created slider-based health bar");
    }
    
    private Vector3 GetTargetPosition()
    {
        if (playerCamera == null) return Vector3.zero;
        
        // Calculate position relative to camera (like MagicalDebugUI)
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;
        Vector3 cameraUp = playerCamera.transform.up;
        
        return playerCamera.transform.position +
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
            healthCanvas.transform.position = Vector3.Lerp(
                healthCanvas.transform.position, 
                targetPosition, 
                Time.deltaTime * followSpeed
            );
            
            if (lookAtCamera)
            {
                // Make canvas face the camera (look away from camera position)
                Vector3 lookDirection = healthCanvas.transform.position - playerCamera.transform.position;
                healthCanvas.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
        else
        {
            // Instant following
            healthCanvas.transform.position = targetPosition;
            
            if (lookAtCamera)
            {
                // Make canvas face the camera (look away from camera position)
                Vector3 lookDirection = healthCanvas.transform.position - playerCamera.transform.position;
                healthCanvas.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }
    
    public void TakeDamage(float damage, Vector3 damageSourcePosition = default)
    {
        if (isDead) return;
        
        float previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        Debug.Log($"[PlayerHealth] Player took {damage} damage. Health: {previousHealth:F1} → {currentHealth:F1}/{maxHealth}");
        
        // Play damage sound
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
        
        // Trigger damage visual effects
        TriggerDamageEffect();
        
        // Show damage indicator if position provided
        if (damageSourcePosition != Vector3.zero)
        {
            DamageIndicator.Instance.ShowDamageIndicator(damageSourcePosition, damage);
        }
        
        // Stop current regeneration
        StopHealthRegeneration();
        
        // Update UI
        UpdateHealthUI();
        OnHealthChanged?.Invoke(GetHealthPercentage());
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Start regeneration timer
            StartHealthRegeneration();
            
            // Check for low health effects
            if (GetHealthPercentage() <= lowHealthThreshold)
            {
                StartLowHealthEffects();
            }
        }
    }
    
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthUI();
        OnHealthChanged?.Invoke(GetHealthPercentage());
        
        Debug.Log($"[PlayerHealth] Player healed {amount}. Health: {currentHealth}/{maxHealth}");
        
        // Stop low health effects if above threshold
        if (GetHealthPercentage() > lowHealthThreshold)
        {
            StopLowHealthEffects();
        }
    }
    
    private void TriggerDamageEffect()
    {
        if (damageEffectCoroutine != null)
        {
            StopCoroutine(damageEffectCoroutine);
        }
        damageEffectCoroutine = StartCoroutine(DamageEffectCoroutine());
    }
    
    private IEnumerator DamageEffectCoroutine()
    {
        // For VR, we'll use health bar flash instead of screen overlay
        if (healthBarFill != null)
        {
            Color originalColor = healthBarFill.color;
            Color flashColor = Color.white;
            
            float elapsedTime = 0f;
            while (elapsedTime < damageFlashDuration)
            {
                float t = elapsedTime / damageFlashDuration;
                float intensity = damageFlashCurve.Evaluate(t);
                
                healthBarFill.color = Color.Lerp(originalColor, flashColor, intensity);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Restore original color
            healthBarFill.color = originalColor;
        }
        
        damageEffectCoroutine = null;
    }
    
    private void StartHealthRegeneration()
    {
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
        }
        regenCoroutine = StartCoroutine(HealthRegenerationCoroutine());
    }
    
    private void StopHealthRegeneration()
    {
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }
        isRegenerating = false;
    }
    
    private IEnumerator HealthRegenerationCoroutine()
    {
        yield return new WaitForSeconds(healthRegenDelay);
        
        isRegenerating = true;
        
        while (currentHealth < maxHealth && !isDead)
        {
            float healthToRegen = healthRegenRate * Time.deltaTime;
            Heal(healthToRegen);
            yield return null;
        }
        
        isRegenerating = false;
        regenCoroutine = null;
    }
    
    private void StartLowHealthEffects()
    {
        if (lowHealthEffectCoroutine != null)
        {
            StopCoroutine(lowHealthEffectCoroutine);
        }
        lowHealthEffectCoroutine = StartCoroutine(LowHealthEffectCoroutine());
    }
    
    private void StopLowHealthEffects()
    {
        if (lowHealthEffectCoroutine != null)
        {
            StopCoroutine(lowHealthEffectCoroutine);
            lowHealthEffectCoroutine = null;
        }
        
        // Reset health bar color
        if (healthBarFill != null)
        {
            healthBarFill.color = Color.Lerp(Color.red, Color.green, GetHealthPercentage());
        }
    }
    
    private IEnumerator LowHealthEffectCoroutine()
    {
        // Play heartbeat sound
        if (lowHealthHeartbeat != null && audioSource != null)
        {
            audioSource.clip = lowHealthHeartbeat;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        while (GetHealthPercentage() <= lowHealthThreshold && !isDead)
        {
            // Pulse effect on health bar
            float pulse = Mathf.Sin(Time.time * heartbeatPulseSpeed) * 0.5f + 0.5f;
            Color pulsedColor = Color.Lerp(lowHealthColor, Color.white, pulse * 0.3f);
            
            if (healthBarFill != null)
            {
                healthBarFill.color = pulsedColor;
            }
            
            yield return null;
        }
        
        // Stop heartbeat sound
        if (audioSource != null && audioSource.clip == lowHealthHeartbeat)
        {
            audioSource.loop = false;
            audioSource.Stop();
        }
        
        lowHealthEffectCoroutine = null;
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        currentHealth = 0;
        
        Debug.Log("[PlayerHealth] Player died!");
        
        // Stop all effects
        StopHealthRegeneration();
        StopLowHealthEffects();
        
        // Play death sound
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Update UI
        UpdateHealthUI();
        
        // Trigger death event
        OnPlayerDied?.Invoke();
        
        // You can add additional death effects here (screen fade, game over UI, etc.)
    }
    
    public void Revive(float healthAmount = -1)
    {
        if (!isDead) return;
        
        isDead = false;
        currentHealth = healthAmount < 0 ? maxHealth : Mathf.Min(maxHealth, healthAmount);
        
        Debug.Log($"[PlayerHealth] Player revived with {currentHealth} health!");
        
        UpdateHealthUI();
        OnHealthChanged?.Invoke(GetHealthPercentage());
        OnPlayerRevived?.Invoke();
        
        StartHealthRegeneration();
    }
    
    private void UpdateHealthUI()
    {
        float healthPercentage = GetHealthPercentage();
        Debug.Log($"[PlayerHealth] UpdateHealthUI called - Health: {currentHealth:F1}/{maxHealth} ({healthPercentage:F2})");
        
        if (healthBarSlider != null)
        {
            Debug.Log($"[PlayerHealth] Setting slider value to {healthPercentage:F2}");
            healthBarSlider.value = healthPercentage;
            Debug.Log($"[PlayerHealth] Slider value is now: {healthBarSlider.value:F2}");
            
            // Update color based on health percentage (unless low health effects are active)
            if (healthBarFill != null && lowHealthEffectCoroutine == null)
            {
                Color newColor = Color.Lerp(Color.red, Color.green, healthPercentage);
                healthBarFill.color = newColor;
                Debug.Log($"[PlayerHealth] Setting health bar color to {newColor}");
            }
        }
        else
        {
            Debug.LogError("[PlayerHealth] healthBarSlider is NULL! Health bar won't update visually.");
        }
        
        if (healthText != null)
        {
            string newText = $"{Mathf.Ceil(currentHealth)}/{maxHealth}";
            healthText.text = newText;
            Debug.Log($"[PlayerHealth] Setting health text to: {newText}");
        }
        else
        {
            Debug.LogError("[PlayerHealth] healthText is NULL! Health text won't update.");
        }
    }
    
    // Public getters
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
    public bool IsRegenerating() => isRegenerating;
    
    // Public setters
    public void SetMaxHealth(float newMaxHealth)
    {
        float healthPercentage = GetHealthPercentage();
        maxHealth = newMaxHealth;
        currentHealth = maxHealth * healthPercentage;
        UpdateHealthUI();
        OnHealthChanged?.Invoke(GetHealthPercentage());
    }
} 
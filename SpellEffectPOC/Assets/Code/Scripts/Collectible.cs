using UnityEngine;
using System.Collections;

/// <summary>
/// Individual collectible item (chocolate frog or golden egg) that can be collected by the player
/// </summary>
public class Collectible : MonoBehaviour
{
    [Header("Collection Settings")]
    [Tooltip("How close the player needs to be to auto-collect")]
    [SerializeField] private float collectionRadius = 2f;
    
    [Tooltip("Whether to collect automatically when player is close, or require trigger interaction")]
    [SerializeField] private bool autoCollectOnProximity = true;
    
    [Tooltip("How often to check for player proximity (in seconds)")]
    [SerializeField] private float proximityCheckInterval = 0.1f;
    
    [Header("Visual Effects")]
    [Tooltip("Particle effect to play when collected")]
    [SerializeField] private GameObject collectionEffect;
    
    [Tooltip("Should the item bob up and down?")]
    [SerializeField] private bool enableBobbing = true;
    
    [Tooltip("Height of the bobbing motion")]
    [SerializeField] private float bobHeight = 0.3f;
    
    [Tooltip("Speed of the bobbing motion")]
    [SerializeField] private float bobSpeed = 2f;
    
    [Tooltip("Should the item rotate?")]
    [SerializeField] private bool enableRotation = true;
    
    [Tooltip("Speed of rotation")]
    [SerializeField] private float rotationSpeed = 50f;
    
    [Tooltip("Glow effect intensity")]
    [SerializeField] private float glowIntensity = 1.5f;
    
    [Header("Audio")]
    [Tooltip("Sound to play when collected")]
    [SerializeField] private AudioClip collectionSound;
    
    [Tooltip("Ambient sound to play while active (optional)")]
    [SerializeField] private AudioClip ambientSound;
    
    // Private variables
    private CollectibleChallenge parentChallenge;
    private int itemIndex;
    private bool isCollected = false;
    private Vector3 initialPosition;
    private AudioSource audioSource;
    private Transform playerTransform;
    private Coroutine proximityCheckCoroutine;
    private Coroutine animationCoroutine;
    private Renderer itemRenderer;
    private Material originalMaterial;
    private Material glowMaterial;
    
    // Events
    public System.Action OnCollected;
    
    void Awake()
    {
        // Get initial position for bobbing
        initialPosition = transform.position;
        
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Get renderer for glow effects
        itemRenderer = GetComponent<Renderer>();
        if (itemRenderer != null)
        {
            originalMaterial = itemRenderer.material;
        }
        
        // Find player
        Camera playerCamera = Camera.main;
        if (playerCamera != null)
        {
            playerTransform = playerCamera.transform;
        }
    }
    
    void Start()
    {
        // Setup visual effects
        SetupGlowEffect();
        
        // Start animations
        if (enableBobbing || enableRotation)
        {
            animationCoroutine = StartCoroutine(AnimationCoroutine());
        }
        
        // Play ambient sound
        if (ambientSound != null && audioSource != null)
        {
            audioSource.clip = ambientSound;
            audioSource.loop = true;
            audioSource.volume = 0.3f;
            audioSource.Play();
        }
        
        // Start proximity checking if auto-collect is enabled
        if (autoCollectOnProximity && playerTransform != null)
        {
            proximityCheckCoroutine = StartCoroutine(ProximityCheckCoroutine());
        }
        
        Debug.Log($"[Collectible] {gameObject.name} initialized at {transform.position}");
    }
    
    /// <summary>
    /// Initializes the collectible with its parent challenge
    /// </summary>
    public void Initialize(CollectibleChallenge challenge, int index)
    {
        parentChallenge = challenge;
        itemIndex = index;
    }
    
    /// <summary>
    /// Sets up glow effect for the collectible
    /// </summary>
    private void SetupGlowEffect()
    {
        if (itemRenderer == null) return;
        
        // Create a glow material based on the original
        glowMaterial = new Material(originalMaterial);
        
        // If the material supports emission, enable it
        if (glowMaterial.HasProperty("_EmissionColor"))
        {
            glowMaterial.EnableKeyword("_EMISSION");
            Color emissionColor = originalMaterial.color * glowIntensity;
            glowMaterial.SetColor("_EmissionColor", emissionColor);
        }
        
        // Apply the glow material
        itemRenderer.material = glowMaterial;
    }
    
    /// <summary>
    /// Animation coroutine for bobbing and rotation
    /// </summary>
    private IEnumerator AnimationCoroutine()
    {
        float time = 0f;
        
        while (!isCollected)
        {
            time += Time.deltaTime;
            
            // Bobbing motion
            if (enableBobbing)
            {
                float newY = initialPosition.y + Mathf.Sin(time * bobSpeed) * bobHeight;
                transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);
            }
            
            // Rotation
            if (enableRotation)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Proximity check coroutine for auto-collection
    /// </summary>
    private IEnumerator ProximityCheckCoroutine()
    {
        while (!isCollected && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distance <= collectionRadius)
            {
                Collect();
                yield break;
            }
            
            yield return new WaitForSeconds(proximityCheckInterval);
        }
    }
    
    /// <summary>
    /// Collects this item
    /// </summary>
    public void Collect()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        Debug.Log($"[Collectible] {gameObject.name} collected!");
        
        // Stop all coroutines
        if (proximityCheckCoroutine != null)
        {
            StopCoroutine(proximityCheckCoroutine);
        }
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        // Play collection effects
        StartCoroutine(CollectionEffectsCoroutine());
        
        // Notify the challenge
        OnCollected?.Invoke();
    }
    
    /// <summary>
    /// Handles collection effects and cleanup
    /// </summary>
    private IEnumerator CollectionEffectsCoroutine()
    {
        // Play collection sound
        if (collectionSound != null && audioSource != null)
        {
            // Stop ambient sound
            if (audioSource.isPlaying && audioSource.clip == ambientSound)
            {
                audioSource.Stop();
            }
            
            audioSource.PlayOneShot(collectionSound);
        }
        
        // Spawn collection effect
        if (collectionEffect != null)
        {
            GameObject effect = Instantiate(collectionEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f); // Clean up effect after 3 seconds
        }
        
        // Animate collection (scale down and fade)
        yield return StartCoroutine(CollectionAnimationCoroutine());
        
        // Destroy the collectible
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Collection animation (scale down and fade out)
    /// </summary>
    private IEnumerator CollectionAnimationCoroutine()
    {
        float animationDuration = 0.5f;
        Vector3 initialScale = transform.localScale;
        
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;
            
            // Scale down
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
            
            // Fade out
            if (itemRenderer != null && glowMaterial != null)
            {
                Color color = glowMaterial.color;
                color.a = Mathf.Lerp(1f, 0f, t);
                glowMaterial.color = color;
                
                // Fade emission too if supported
                if (glowMaterial.HasProperty("_EmissionColor"))
                {
                    Color emissionColor = glowMaterial.GetColor("_EmissionColor");
                    emissionColor.a = color.a;
                    glowMaterial.SetColor("_EmissionColor", emissionColor);
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    
    /// <summary>
    /// Manual collection trigger (for non-proximity collection)
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (!autoCollectOnProximity && other.CompareTag("Player"))
        {
            Collect();
        }
    }
    
    /// <summary>
    /// Called when player might interact (for VR hand interaction)
    /// </summary>
    public void OnHandInteraction()
    {
        Collect();
    }
    
    void OnDrawGizmos()
    {
        // Draw collection radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, collectionRadius);
    }
    
    void OnDestroy()
    {
        // Clean up materials
        if (glowMaterial != null && glowMaterial != originalMaterial)
        {
            if (Application.isPlaying)
            {
                Destroy(glowMaterial);
            }
            else
            {
                DestroyImmediate(glowMaterial);
            }
        }
    }
} 
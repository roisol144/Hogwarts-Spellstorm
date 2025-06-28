using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProtegoShield : MonoBehaviour
{
    [Header("Shield Settings")]
    [SerializeField] private float shieldRadius = 5f;
    [SerializeField] private float shieldDuration = 5f;
    [SerializeField] private LayerMask enemyLayerMask = -1; // Default to all layers
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject shieldVisualEffect;
    [SerializeField] private Material shieldMaterial;
    [SerializeField] private Color shieldColor = new Color(0.2f, 0.6f, 1f, 0.3f);
    
    [Header("Audio")]
    [SerializeField] private AudioClip shieldActivateSound;
    [SerializeField] private AudioClip shieldDeactivateSound;
    [SerializeField] private AudioSource audioSource;
    
    // Private variables
    private bool isActive = false;
    private Coroutine shieldCoroutine;
    private List<EnemyMovement> affectedEnemies = new List<EnemyMovement>();
    private List<EnemyAttack> affectedAttacks = new List<EnemyAttack>();
    private GameObject shieldVisual;
    private GameObject protegoPrefabInstance; // Reference to the protego prefab spawned by SpellCastingManager
    
    // Events
    public System.Action OnShieldActivated;
    public System.Action OnShieldDeactivated;
    
    private void Awake()
    {
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }
    
    private void Start()
    {
        // Start the shield immediately
        ActivateShield();
    }
    
    public void ActivateShield()
    {
        if (isActive) return;
        
        isActive = true;
        Debug.Log("[ProtegoShield] Shield activated!");
        
        // Create visual effect
        CreateShieldVisual();
        
        // Play activation sound
        if (shieldActivateSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shieldActivateSound);
        }
        
        // Start shield duration
        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
        }
        shieldCoroutine = StartCoroutine(ShieldDurationCoroutine());
        
        // Notify enemies about the shield
        NotifyEnemiesAboutShield(true);
        
        OnShieldActivated?.Invoke();
    }
    
    public void DeactivateShield()
    {
        if (!isActive) return;
        
        isActive = false;
        Debug.Log("[ProtegoShield] Shield deactivated!");
        
        // Remove visual effect
        if (shieldVisual != null)
        {
            Destroy(shieldVisual);
        }
        
        // Destroy the protego prefab instance if it exists
        if (protegoPrefabInstance != null)
        {
            Debug.Log("[ProtegoShield] Destroying protego prefab instance");
            Destroy(protegoPrefabInstance);
            protegoPrefabInstance = null;
        }
        
        // Play deactivation sound
        if (shieldDeactivateSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(shieldDeactivateSound);
        }
        
        // Stop shield coroutine
        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
            shieldCoroutine = null;
        }
        
        // Notify enemies that shield is gone
        NotifyEnemiesAboutShield(false);
        
        // Clean up affected enemies
        affectedEnemies.Clear();
        affectedAttacks.Clear();
        
        OnShieldDeactivated?.Invoke();
        
        // Destroy this component
        Destroy(this);
    }
    
    private void CreateShieldVisual()
    {
        if (shieldVisualEffect != null)
        {
            // Use the provided visual effect
            shieldVisual = Instantiate(shieldVisualEffect, transform.position, Quaternion.identity);
            shieldVisual.transform.SetParent(transform);
        }
        else
        {
            // Create a simple sphere shield
            shieldVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shieldVisual.name = "ProtegoShieldVisual";
            shieldVisual.transform.SetParent(transform);
            shieldVisual.transform.localPosition = Vector3.zero;
            shieldVisual.transform.localScale = Vector3.one * shieldRadius * 2f;
            
            // Remove collider from visual
            Collider visualCollider = shieldVisual.GetComponent<Collider>();
            if (visualCollider != null)
            {
                Destroy(visualCollider);
            }
            
            // Set material
            Renderer visualRenderer = shieldVisual.GetComponent<Renderer>();
            if (visualRenderer != null)
            {
                if (shieldMaterial != null)
                {
                    visualRenderer.material = shieldMaterial;
                }
                else
                {
                    // Create a simple transparent material
                    Material material = new Material(Shader.Find("Standard"));
                    material.SetFloat("_Mode", 3); // Transparent mode
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.renderQueue = 3000;
                    material.color = shieldColor;
                    visualRenderer.material = material;
                }
            }
        }
    }
    
    private IEnumerator ShieldDurationCoroutine()
    {
        Debug.Log($"[ProtegoShield] Shield will last for {shieldDuration} seconds");
        yield return new WaitForSeconds(shieldDuration);
        Debug.Log($"[ProtegoShield] Shield duration expired, deactivating...");
        DeactivateShield();
    }
    
    private void NotifyEnemiesAboutShield(bool shieldActive)
    {
        // Target the enemy this shield is attached to specifically
        EnemyMovement enemyMovement = GetComponent<EnemyMovement>();
        EnemyAttack enemyAttack = GetComponent<EnemyAttack>();
        
        if (enemyMovement != null)
        {
            if (shieldActive)
            {
                // Add to affected enemies if not already there
                if (!affectedEnemies.Contains(enemyMovement))
                {
                    affectedEnemies.Add(enemyMovement);
                }
                
                // Stop this enemy's movement
                enemyMovement.SetProtegoShieldActive(true);
                Debug.Log($"[ProtegoShield] Freezing enemy movement: {gameObject.name}");
            }
            else
            {
                // Remove from affected enemies
                affectedEnemies.Remove(enemyMovement);
                
                // Resume this enemy's movement
                enemyMovement.SetProtegoShieldActive(false);
                Debug.Log($"[ProtegoShield] Unfreezing enemy movement: {gameObject.name}");
            }
        }
        
        if (enemyAttack != null)
        {
            if (shieldActive)
            {
                // Add to affected attacks if not already there
                if (!affectedAttacks.Contains(enemyAttack))
                {
                    affectedAttacks.Add(enemyAttack);
                }
                
                // Stop this enemy's attacks
                enemyAttack.SetProtegoShieldActive(true);
                Debug.Log($"[ProtegoShield] Disabling enemy attacks: {gameObject.name}");
            }
            else
            {
                // Remove from affected attacks
                affectedAttacks.Remove(enemyAttack);
                
                // Resume this enemy's attacks
                enemyAttack.SetProtegoShieldActive(false);
                Debug.Log($"[ProtegoShield] Re-enabling enemy attacks: {gameObject.name}");
            }
        }
    }
    
    private void Update()
    {
        // No longer needed since we're only affecting the specific enemy this shield is attached to
        // The Update method can be removed or used for other visual effects if needed
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw shield radius in editor
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, shieldRadius);
    }
    
    // Public getters
    public bool IsActive() => isActive;
    public float GetShieldRadius() => shieldRadius;
    public float GetRemainingTime()
    {
        if (!isActive || shieldCoroutine == null) return 0f;
        // This is a simplified version - in a real implementation you'd track the actual remaining time
        return shieldDuration;
    }
    
    // Method to set the protego prefab instance that should be destroyed with the shield
    public void SetProtegoPrefabInstance(GameObject prefabInstance)
    {
        protegoPrefabInstance = prefabInstance;
        Debug.Log($"[ProtegoShield] Protego prefab instance set: {prefabInstance?.name}");
    }
} 
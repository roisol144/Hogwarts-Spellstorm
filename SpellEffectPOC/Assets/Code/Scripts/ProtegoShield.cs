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
        yield return new WaitForSeconds(shieldDuration);
        DeactivateShield();
    }
    
    private void NotifyEnemiesAboutShield(bool shieldActive)
    {
        // Find all enemies within the shield radius
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, shieldRadius, enemyLayerMask);
        
        foreach (Collider enemyCollider in enemiesInRange)
        {
            EnemyMovement enemyMovement = enemyCollider.GetComponent<EnemyMovement>();
            EnemyAttack enemyAttack = enemyCollider.GetComponent<EnemyAttack>();
            
            if (enemyMovement != null)
            {
                if (shieldActive)
                {
                    // Add to affected enemies if not already there
                    if (!affectedEnemies.Contains(enemyMovement))
                    {
                        affectedEnemies.Add(enemyMovement);
                    }
                    
                    // Stop enemy movement
                    enemyMovement.SetProtegoShieldActive(true);
                }
                else
                {
                    // Remove from affected enemies
                    affectedEnemies.Remove(enemyMovement);
                    
                    // Resume enemy movement
                    enemyMovement.SetProtegoShieldActive(false);
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
                    
                    // Stop enemy attacks
                    enemyAttack.SetProtegoShieldActive(true);
                }
                else
                {
                    // Remove from affected attacks
                    affectedAttacks.Remove(enemyAttack);
                    
                    // Resume enemy attacks
                    enemyAttack.SetProtegoShieldActive(false);
                }
            }
        }
    }
    
    private void Update()
    {
        if (!isActive) return;
        
        // Check for new enemies entering the shield radius
        NotifyEnemiesAboutShield(true);
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
} 
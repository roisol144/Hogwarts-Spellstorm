using UnityEngine;

/// <summary>
/// Helper script to automatically set up existing prefabs as collectibles
/// Add this to your existing chocolate frog and golden egg prefabs
/// </summary>
public class CollectibleSetup : MonoBehaviour
{
    [Header("Collectible Configuration")]
    [Tooltip("How close the player needs to be to auto-collect")]
    [SerializeField] private float collectionRadius = 2.5f;
    
    [Tooltip("Whether to collect automatically when player is close")]
    [SerializeField] private bool autoCollectOnProximity = true;
    
    [Header("Visual Effects")]
    [Tooltip("Should the item bob up and down?")]
    [SerializeField] private bool enableBobbing = true;
    
    [Tooltip("Height of the bobbing motion")]
    [SerializeField] private float bobHeight = 0.2f;
    
    [Tooltip("Speed of the bobbing motion")]
    [SerializeField] private float bobSpeed = 1.5f;
    
    [Tooltip("Should the item rotate?")]
    [SerializeField] private bool enableRotation = true;
    
    [Tooltip("Speed of rotation")]
    [SerializeField] private float rotationSpeed = 30f;
    
    [Tooltip("Glow effect intensity")]
    [SerializeField] private float glowIntensity = 1.2f;
    
    [Header("Audio")]
    [Tooltip("Sound to play when collected")]
    [SerializeField] private AudioClip collectionSound;
    
    [Tooltip("Ambient sound to play while active (optional)")]
    [SerializeField] private AudioClip ambientSound;
    
    void Awake()
    {
        SetupCollectible();
    }
    
    private void SetupCollectible()
    {
        // Add Collectible component if it doesn't exist
        Collectible collectible = GetComponent<Collectible>();
        if (collectible == null)
        {
            collectible = gameObject.AddComponent<Collectible>();
        }
        
        // Configure collectible settings using reflection to set private fields
        System.Reflection.FieldInfo[] fields = typeof(Collectible).GetFields(
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance
        );
        
        foreach (var field in fields)
        {
            switch (field.Name)
            {
                case "collectionRadius":
                    field.SetValue(collectible, collectionRadius);
                    break;
                case "autoCollectOnProximity":
                    field.SetValue(collectible, autoCollectOnProximity);
                    break;
                case "enableBobbing":
                    field.SetValue(collectible, enableBobbing);
                    break;
                case "bobHeight":
                    field.SetValue(collectible, bobHeight);
                    break;
                case "bobSpeed":
                    field.SetValue(collectible, bobSpeed);
                    break;
                case "enableRotation":
                    field.SetValue(collectible, enableRotation);
                    break;
                case "rotationSpeed":
                    field.SetValue(collectible, rotationSpeed);
                    break;
                case "glowIntensity":
                    field.SetValue(collectible, glowIntensity);
                    break;
                case "collectionSound":
                    field.SetValue(collectible, collectionSound);
                    break;
                case "ambientSound":
                    field.SetValue(collectible, ambientSound);
                    break;
            }
        }
        
        // Add collider if none exists
        Collider existingCollider = GetComponent<Collider>();
        if (existingCollider == null)
        {
            // Add a trigger collider for collection
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = Vector3.one * collectionRadius;
        }
        else
        {
            // Make sure existing collider is a trigger
            existingCollider.isTrigger = true;
        }
        
        // Add AudioSource if none exists
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.5f;
        }
        
        // Set tag for easy identification
        if (gameObject.tag == "Untagged")
        {
            gameObject.tag = "Collectible";
        }
        
        Debug.Log($"[CollectibleSetup] {gameObject.name} configured as collectible");
    }
} 
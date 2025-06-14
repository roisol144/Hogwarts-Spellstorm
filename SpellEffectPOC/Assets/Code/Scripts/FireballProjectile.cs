using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 15f;
    public float lifetime = 5f;
    private float timer;

    [Header("Impact Settings")]
    public GameObject explosionPrefab; // Optional: assign an explosion effect prefab
    public AudioClip impactSound;      // Optional: assign an impact sound

    private AudioSource audioSource;
    private bool hasCollided = false; // Prevent multiple collision handling

    private void Start()
    {
        // Cache the AudioSource if needed
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Move forward in local space
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // Update the lifetime timer and destroy after exceeding lifetime
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Prevent multiple collision handling
        if (hasCollided)
        {
            Debug.Log($"[Fireball] Already handled collision, ignoring duplicate");
            return;
        }
        hasCollided = true;

        Debug.Log($"[Fireball] Fireball collided with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        bool hitEnemy = false;

        // Check if we hit an enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log($"[Fireball] Found Enemy component on {collision.gameObject.name}");
                Debug.Log($"[Fireball] Applying regular damage (Enemy will calculate 1/3 max health)");
                
                // Apply regular damage (Enemy calculates actual damage as maxHealth/3)
                enemy.TakeDamage(0f, false);
                hitEnemy = true;
                Debug.Log($"[Fireball] Successfully applied damage to enemy!");
            }
            else
            {
                Debug.LogError($"[Fireball] GameObject {collision.gameObject.name} has 'Enemy' tag but no Enemy component!");
            }
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            // Handle wall collision with special effects for water/pool
            HandleWallCollision(collision);
        }
        else
        {
            Debug.Log($"[Fireball] Hit non-enemy object: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");
        }

        // Only show the fireball's explosion if we did NOT hit an enemy and it's not a wall (walls are handled separately)
        if (!hitEnemy && !collision.gameObject.CompareTag("Wall") && explosionPrefab != null)
        {
            Debug.Log($"[Fireball] Spawning explosion effect for non-wall object");
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (impactSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(impactSound);
        }

        Debug.Log($"[Fireball] Destroying fireball projectile");
        Destroy(gameObject);
    }

    /// <summary>
    /// Handles collision with wall objects, creating appropriate effects
    /// </summary>
    /// <param name="collision">Collision information</param>
    private void HandleWallCollision(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        
        // Check if this is a water/pool object
        bool isWater = IsWaterObject(collision.gameObject);
        
        if (isWater)
        {
            Debug.Log($"[Fireball] ✅ Hit water object: {collision.gameObject.name} - Creating ONLY directional splash effect (NO explosion)");
            try
            {
                // Calculate impact direction from spell's movement
                Vector3 impactDirection = transform.forward;
                Vector3 surfaceNormal = contact.normal;
                
                // Create single directional splash
                WaterSplashEffect.CreateSplash(contact.point, impactDirection, surfaceNormal);
                Debug.Log($"[Fireball] ✅ Single directional water splash created at {contact.point} facing {impactDirection}!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Fireball] ❌ Error creating splash effect: {e.Message}");
            }
        }
        else
        {
            Debug.Log($"[Fireball] Hit regular wall: {collision.gameObject.name} - Creating explosion effect");
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, contact.point, Quaternion.identity);
                Debug.Log($"[Fireball] ✅ Explosion effect created successfully!");
            }
            else
            {
                Debug.LogWarning($"[Fireball] ❌ No explosion prefab assigned!");
            }
        }
    }

    /// <summary>
    /// Checks if the collided object is a water/pool object based on its name
    /// </summary>
    /// <param name="obj">The GameObject to check</param>
    /// <returns>True if it's a water object</returns>
    private bool IsWaterObject(GameObject obj)
    {
        string objectName = obj.name.ToLower();
        string[] waterKeywords = { "pool", "water", "lake", "pond", "basin" };
        
        foreach (string keyword in waterKeywords)
        {
            if (objectName.Contains(keyword.ToLower()))
            {
                return true;
            }
        }
        
        return false;
    }
}

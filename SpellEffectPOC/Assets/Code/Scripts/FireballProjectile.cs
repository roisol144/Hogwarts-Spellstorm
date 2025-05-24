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
        else
        {
            Debug.Log($"[Fireball] Hit non-enemy object: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");
        }

        // Only show the fireball's explosion if we did NOT hit an enemy
        if (!hitEnemy && explosionPrefab != null)
        {
            Debug.Log($"[Fireball] Spawning explosion effect");
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (impactSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(impactSound);
        }

        Debug.Log($"[Fireball] Destroying fireball projectile");
        Destroy(gameObject);
    }
}

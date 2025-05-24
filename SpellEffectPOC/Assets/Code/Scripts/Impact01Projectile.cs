using UnityEngine;

public class Impact01Projectile : MonoBehaviour
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
        Debug.Log($"[Impact01] Impact01 collided with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        bool hitEnemy = false;

        // Check if we hit an enemy
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log($"[Impact01] Found Enemy component on {collision.gameObject.name}");
                Debug.Log($"[Impact01] Applying special damage (instant kill, isSpecial: true)");
                
                // Apply special damage (instant kill)
                enemy.TakeDamage(0f, true);
                hitEnemy = true;
                Debug.Log($"[Impact01] Successfully applied special damage - enemy should be dead!");
            }
            else
            {
                Debug.LogError($"[Impact01] GameObject {collision.gameObject.name} has 'Enemy' tag but no Enemy component!");
            }
        }
        else
        {
            Debug.Log($"[Impact01] Hit non-enemy object: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");
        }

        // Only show the Impact01's explosion if we did NOT hit an enemy
        if (!hitEnemy && explosionPrefab != null)
        {
            Debug.Log($"[Impact01] Spawning explosion effect");
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (impactSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(impactSound);
        }

        Debug.Log($"[Impact01] Destroying Impact01 projectile");
        Destroy(gameObject);
    }
} 
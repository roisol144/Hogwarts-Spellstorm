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
    public float damage = 10f;         // Optional: if you plan on applying damage

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
        // Optionally, you can apply damage to hit objects here

        // Instantiate explosion effect if assigned
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Play impact sound if assigned
        if (impactSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(impactSound);
        }

        // Destroy the fireball on impact
        Destroy(gameObject);
    }
}

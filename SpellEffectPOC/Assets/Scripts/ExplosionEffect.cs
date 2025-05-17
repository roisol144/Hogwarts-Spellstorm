using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [Header("Mesh Effect Settings")]
    public float expandSpeed = 5f;
    public float maxScale = 3f;
    public float fadeSpeed = 2f;
    
    [Header("Particle System")]
    public ParticleSystem explosionParticles;
    
    private Material material;
    private float currentScale = 0f;
    private float currentAlpha = 1f;
    private bool isParticleSystem = false;

    void Start()
    {
        // Check if we have a particle system
        if (explosionParticles != null)
        {
            isParticleSystem = true;
            explosionParticles.Play();
            // Destroy the object after the particle system finishes
            Destroy(gameObject, explosionParticles.main.duration);
            return;
        }

        // Original mesh-based effect code
        material = GetComponent<Renderer>().material;
        currentScale = 0f;
        currentAlpha = 1f;
    }

    void Update()
    {
        if (isParticleSystem)
            return;

        // Expand the explosion
        currentScale += expandSpeed * Time.deltaTime;
        transform.localScale = Vector3.one * Mathf.Min(currentScale, maxScale);

        // Fade out the explosion
        currentAlpha -= fadeSpeed * Time.deltaTime;
        Color color = material.color;
        color.a = Mathf.Max(0f, currentAlpha);
        material.color = color;

        // Destroy the object when fully faded
        if (currentAlpha <= 0f)
        {
            Destroy(gameObject);
        }
    }
} 
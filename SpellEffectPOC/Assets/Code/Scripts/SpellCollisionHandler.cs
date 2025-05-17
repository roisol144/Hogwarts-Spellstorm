using UnityEngine;

public class SpellCollisionHandler : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float explosionForce = 10f;
    [SerializeField] private float explosionLifetime = 2f;

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collision is with a wall (you can adjust the layer or tag check as needed)
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Visual debug: create a small sphere at the contact point
            GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugSphere.transform.position = collision.contacts[0].point;
            debugSphere.transform.localScale = Vector3.one * 0.2f;
            Destroy(debugSphere, 2f);

            // Get the contact point
            ContactPoint contact = collision.contacts[0];
            
            // Create explosion effect
            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, contact.point, Quaternion.identity);
                Destroy(explosion, explosionLifetime);
            }

            // Apply explosion force to nearby objects
            Collider[] colliders = Physics.OverlapSphere(contact.point, explosionRadius);
            foreach (Collider hit in colliders)
            {
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, contact.point, explosionRadius);
                }
            }

            // Destroy the spell
            Destroy(gameObject);
        }
    }
} 
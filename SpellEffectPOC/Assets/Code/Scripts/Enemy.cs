using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject deathEffectPrefab; // Assign a prefab in the Inspector

    public virtual void Die()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
} 